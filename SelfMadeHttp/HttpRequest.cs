using Htlvb.Http;
using System.Text;

namespace SelfMadeHttp;

public class HttpRequest(string methode, string path, string version, HttpHeaders headers, byte[] body)
{
    public byte[] Body { get; } = body;
    public string Methode { get; } = methode;
    public string Path { get; } = path;
    public string Version { get; } = version;
    public HttpHeaders Headers { get; } = headers;

    public Encoding? BodyEncoding
    {
        get
        {
            if (!Headers.TryGetValue("Content-Type", out string? contentType) || !contentType.Contains("charset="))
            {
                return null;
            }
            return Encoding.GetEncoding(contentType.Split("charset=", StringSplitOptions.TrimEntries)[1]);
        }
    }

    // ReadFromStream
    // start line parsen (Methode, Path, Version)
    // Headers parsen (HttpHeaders.ReadFromStream)
    public static HttpRequest ReadFromStream(Stream stream)
    {
        HttpStreamReader httpStreamReader = new(stream);
        httpStreamReader.Encoding = Encoding.ASCII;
        (string version, string path, string statusText) = ReadStartLine(httpStreamReader);
        HttpHeaders headers = HttpHeaders.ReadHeaders(httpStreamReader);
        headers.TryGetValue("Content-Length", out string? contentLength);
        byte[] body = httpStreamReader.ReadBytes(Convert.ToInt32(contentLength));

        return new HttpRequest(version, path, statusText, headers, body);

    }

    private static (string methode, string path, string version) ReadStartLine(HttpStreamReader httpReader)
    {
        string line = httpReader.ReadLine();

        string[] parts = line.Split(' ', 3);
        if (parts.Length != 3) throw new ArgumentOutOfRangeException($"The startline wasn't in the correct format! \"{line}\"");

        return (parts[0], parts[1], parts[2]);
    }

    public override string ToString()
    {
        string body = BodyEncoding == null
            ? $"{Body.Length} bytes"
            : BodyEncoding.GetString(Body);
        return $"{Methode} {Path} {Version}{Environment.NewLine}{Headers.ToString()}{Environment.NewLine}{Environment.NewLine}{body}";
    }
}
