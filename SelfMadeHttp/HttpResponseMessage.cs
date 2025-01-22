using Htlvb.Http;
using System.Net.Sockets;
using System.Text;

namespace SelfMadeHttp;

public class HttpResponseMessage(string version, int statusCode, string statusText, HttpHeaders headers, byte[] body)
{
    public byte[] Body { get; } = body;
    public int StatusCode { get; } = statusCode;
    public string StatusText { get; } = statusText;
    public string Version { get; } = version;
    public HttpHeaders Headers { get; } = headers;


    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
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

    public override string ToString()
    {
        string body = BodyEncoding == null
            ? $"{Body.Length} bytes"
            : BodyEncoding.GetString(Body);
        return $"{Version} {StatusCode} {StatusText}{Environment.NewLine}{Headers.ToString()}{Environment.NewLine}{Environment.NewLine}{body}";
    }

    public static HttpResponseMessage ReadFromStream(Stream httpStream)
    {
        HttpStreamReader httpReader = new HttpStreamReader(httpStream) { Encoding = Encoding.ASCII };
        (string version, int statuscode, string statusText) = ReadStartLine(httpReader);

        HttpHeaders headers = HttpHeaders.ReadHeaders(httpReader);
        headers.TryGetValue("Content-Length", out string? contentLength);
        byte[] body = httpReader.ReadBytes(Convert.ToInt32(contentLength));

        return new HttpResponseMessage(version, statuscode, statusText, headers, body);
    }

    private static (string version, int statusCode, string statusText) ReadStartLine(HttpStreamReader httpReader)
    {
        string line = httpReader.ReadLine();

        string[] parts = line.Split(' ', 3);
        if (parts.Length != 3) throw new ArgumentOutOfRangeException($"The startline wasn't in the correct format! \"{line}\"");
        if (!int.TryParse(parts[1], out int statusCode)) throw new ArgumentOutOfRangeException("StatusCode wasn't in the correct format!");

        return (parts[0], statusCode, parts[2]);
    }

    public void WriteTo(NetworkStream httpStream)
    {
        httpStream.Write(Encoding.ASCII.GetBytes($"{Version} {StatusCode} {StatusText}\r\n"));
        Headers.WriteTo(httpStream);
        httpStream.Write(Encoding.ASCII.GetBytes("\r\n"));
        httpStream.Write(Encoding.ASCII.GetBytes("\r\n"));
        httpStream.Write(Body);
        httpStream.Flush();
    }

    public static HttpResponseMessage Ok(string contentType, byte[] body)
    {
        return new("HTTP/1.1", 200, "OK",
            new HttpHeaders
            {
                {"Content-Type",contentType },
                {"Content-Length",  body.Length.ToString() }
            },
            body
        );

    }

    public static HttpResponseMessage Ok(string contentType, Encoding encoding, string text)
    {
        return Ok($"{contentType}; charset={encoding.WebName}", encoding.GetBytes(text));

    }

    public static HttpResponseMessage? Create(int statusCode, string statusText)
    {
        return new HttpResponseMessage("HTTP/1.1", statusCode, statusText, new HttpHeaders(), []);
    }
}
