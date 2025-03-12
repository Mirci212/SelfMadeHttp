using SelfMadeHttp;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using HttpResponseMessage = SelfMadeHttp.HttpResponseMessage;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine($"==🎉  Self-Made HTTP server 🎉==");

GuessGame game = new();


using TcpListener httpServer = new(IPAddress.Any, 80);
httpServer.Start();
while (true)
{
    Console.WriteLine("Waiting for client ⏳");
    TcpClient httpClient = httpServer.AcceptTcpClient();
    Task work = Task.Run(() =>
    {
        Console.WriteLine("Client as connected. 🎉");
        using NetworkStream httpStream = httpClient.GetStream();

        HttpRequest httpRequest = HttpRequest.ReadFromStream(httpStream);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(httpRequest.ToString());
        Console.ResetColor();
        Console.WriteLine();

        GenerateResponse(httpRequest).WriteTo(httpStream);
        httpClient.Dispose();
    });
    
}

HttpResponseMessage GenerateResponse(HttpRequest httpRequest)
{
    HttpResponseMessage httpResponseMessage = HttpResponseMessage.Create(404, "Not Found");
    try
    {
        
        if (httpRequest.Methode == "GET" && httpRequest.Path == "/game")
        {
            httpResponseMessage = HttpResponseMessage.Ok("text/plain", Encoding.UTF8, $"Hallo zu meinen Spiel. Machen sie als naechstes einen POST(/guessGame/new/<playerName>(nur Buchstaben erlaubt);<min>;<max>;<maxTries>)\n" +
                $"GET /game\r\nPOST /guessGame/new/<playerName>(nur Buchstaben erlaubt);<min>;<max>;<maxTries> [Player erstellen]\r\nPOST /guessGame/guess/<playername>;<guessNumber> [Nummer raten]\r\nDELETE /guessGame/player/<playerName> [Spieler löschen]");
        }

        else if (httpRequest.Methode == "POST" && Regex.IsMatch(httpRequest.Path, @"^/guessGame/new/([a-zA-Z]+);(\d+);(\d+);(\d+)$"))
        {
            string[] splitText = httpRequest.Path.Split('/')[3].Split(";");
            if (!game.AddPlayer(splitText[0], Convert.ToInt32(splitText[1]), Convert.ToInt32(splitText[2]), Convert.ToInt32(splitText[3])))
                httpResponseMessage = HttpResponseMessage.Create(404, "Player existiert schon" );
            else
            {
                httpResponseMessage = HttpResponseMessage.Ok("text/plain", Encoding.UTF8, "Jetzt können Sie mit dem Raten beginnen: POST(/guessGame/guess/<playername>;<guessNumber>)");
            }
        }

        else if (httpRequest.Methode == "DELETE" && Regex.IsMatch(httpRequest.Path, @"^/guessGame/player/([a-zA-Z]+)$"))
        {
            string splitText = httpRequest.Path.Split('/')[3];
            if (!game.playerList.Remove(game[splitText]))
                httpResponseMessage = HttpResponseMessage.Create(404, "Invalid Input");
            else
            {
                httpResponseMessage = HttpResponseMessage.Ok("text/plain", Encoding.UTF8, $"Player {splitText} wurde gelöscht");
            }
        }

        else if (httpRequest.Methode == "POST" && Regex.IsMatch(httpRequest.Path, @"^/guessGame/guess/([a-zA-Z]+);(\d+)$"))
        {
            string[] splitText = httpRequest.Path.Split('/')[3].Split(";");
            httpResponseMessage = HttpResponseMessage.Ok("text/plain",Encoding.UTF8,$"{game.GuessNumber(Convert.ToInt32(splitText[1]), splitText[0])}");
        }

    }
    catch (Exception ex)
    {
        httpResponseMessage = HttpResponseMessage.Create(404, "Invalid Input");
    }

    return httpResponseMessage;
}