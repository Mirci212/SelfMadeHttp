using SelfMadeHttp;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime;
using System.Text;
using HttpResponseMessage = SelfMadeHttp.HttpResponseMessage;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine($"❤  {(char)83}elf-Made HTTP client client ❤");

// HttpGet(host: "wttr.in", port: 443, ssl: true, path: "/voecklabruck?format=3");
// HttpGet(host: "google.com", port: 443, ssl: true, path: "/");
// HttpGet(host: "httpbin.org", port: 80, ssl: false, path: "/status/200");

// HttpGet(host: "www.oefb.at", port: 443, ssl: true, path: "/oefb2/images/1278650591628556536_215ee0a99f28ed3579d8-1,0-320x320.png");
Send(host: "localhost", port: 80, ssl: false, HttpRequest.Get(path: "/"));
Send(host: "localhost", port: 80, ssl: false, HttpRequest.Get(path: "/weather"));
Send(host: "localhost", port: 80, ssl: false, HttpRequest.Post(path: "/news"));

void Send(string host, int port, bool ssl, HttpRequest request)
{
    request.Headers.Add("Host", host);

    using TcpClient httpClient = new TcpClient();
    httpClient.Connect(host, port);
    NetworkStream httpStream = httpClient.GetStream();

    if (ssl)
    {
        using SslStream encryptedHttpStream = new SslStream(httpStream, leaveInnerStreamOpen: true);
        encryptedHttpStream.AuthenticateAsClient(host);

        request.WriteTo(encryptedHttpStream);
        ReadHttpResponse(encryptedHttpStream);
        return;
    }

    request.WriteTo(httpStream);
    ReadHttpResponse(httpStream);
}

static void ReadHttpResponse(Stream httpStream)
{
    HttpResponseMessage httpResponse = HttpResponseMessage.ReadFromStream(httpStream);
    Console.ForegroundColor = httpResponse.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red;
    Console.WriteLine(httpResponse);
    Console.ResetColor();
    File.WriteAllBytes("../../../lets-code.png", httpResponse.Body);
}