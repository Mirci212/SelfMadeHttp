using Htlvb.Http;
using System.Net.Sockets;
using System.Text;

namespace SelfMadeHttp;

public class HttpHeaders : Dictionary<string, string>
{
    public HttpHeaders() : base(StringComparer.OrdinalIgnoreCase) { }

    public override string ToString()
    {
        return string.Join("\r\n", this.Select(h => $"{h.Key}: {h.Value}"));
    }

    public static HttpHeaders ReadHeaders(HttpStreamReader httpReader)
    {
        HttpHeaders headers = new();
        string line;

        while ((line = httpReader.ReadLine()) != null && line != "")
        {
            var parts = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                headers[parts[0]] = parts[1];
            }
        }

        return headers;
    }

    internal void WriteTo(Stream httpStream)
    {
        httpStream.Write(Encoding.ASCII.GetBytes(this.ToString() + "\r\n"));
    }
}
