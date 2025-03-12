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

    public static HttpRequest Get(string path)
    {
        return new HttpRequest("GET", path, "HTTP/1.1", new(), []);
    }

    public static HttpRequest Post(string path)
    {
        return new HttpRequest("POST", path, "HTTP/1.1", new(), []);
    }
    public static HttpRequest Post(string path, string contentType, Encoding encoding, string body)
    {
        byte[] bodyBytes = encoding.GetBytes(body);
        HttpHeaders headers = new HttpHeaders
        {
            { "Content-Type", $"{contentType}; charset={encoding.WebName}" },
            {"Content-Length",$"{bodyBytes.Length}" }

        };
        return new HttpRequest("POST", path, "HTTP/1.1", headers, bodyBytes);
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
        string[] parts = null;
        string line = httpReader.ReadLine();
        if(line == null) throw new ArgumentOutOfRangeException("The startline wasn't in the correct format! (line was null)");
        try {parts = line.Split(' ', 3); } catch (Exception ex) { throw new ArgumentOutOfRangeException($"The startline wasn't in the correct format! \"{line}\"", ex); }
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

    public void WriteTo(Stream stream)
    {
        stream.Write(Encoding.ASCII.GetBytes($"{Methode} {path} {Version}\r\n"));
        Headers.WriteTo(stream);
        stream.Write(Encoding.ASCII.GetBytes($"\r\n"));
        stream.Write(Body);
        stream.Flush();
    }
}
