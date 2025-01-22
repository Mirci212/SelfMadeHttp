using SelfMadeHttp;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HttpResponseMessage = SelfMadeHttp.HttpResponseMessage;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine($"==🎉  Self-Made HTTP server 🎉==");

using TcpListener httpServer = new(IPAddress.Any, 80);
httpServer.Start();
while (true)
{
    using TcpClient httpClient = httpServer.AcceptTcpClient();
    Console.WriteLine("Client as connected. 🎉");
    using NetworkStream httpStream = httpClient.GetStream();

    HttpRequest httpRequest = HttpRequest.ReadFromStream(httpStream);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(httpRequest.ToString());
    Console.ResetColor();
    Console.WriteLine();

    GenerateResponse(httpRequest).WriteTo(httpStream);
}

static HttpResponseMessage GenerateResponse(HttpRequest httpRequest)
{
    HttpResponseMessage httpResponseMessage;
    if (httpRequest.Methode == "GET" && httpRequest.Path == "/")
    {
        httpResponseMessage = HttpResponseMessage.Ok("text/html", Encoding.UTF8, "<h1>Hallihallo</h1>");
    }

    else if (httpRequest.Methode == "GET" && httpRequest.Path == "/news")
    {
        httpResponseMessage = HttpResponseMessage.Ok("text/html", Encoding.UTF8, "<h1>Heute ist ein schöner Tag</h1>");
    }
    else
    {
        httpResponseMessage = HttpResponseMessage.Create(404, "Not Found");
    }

    return httpResponseMessage;
}