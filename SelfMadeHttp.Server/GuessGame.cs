using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class GuessGame
{
    public List<Player> playerList { get; set; }

    public GuessGame()
    {
        playerList = new List<Player>();
    }

    public Player this[string name]
    {
        get
        {
            return playerList.First(p => p.Name.Equals(name));
        }
    }

    public bool AddPlayer(string name, int min, int max, int Tries)
    {
        if (playerList.Exists(p => p.Name.Equals(name))) return false;
        playerList.Add(new Player(name, min, max, Tries));
        return true;
    }

    public string GuessNumber(int guessNumber, string playerName)
    {
        Player player = playerList.FirstOrDefault(p => p.Name.Equals(playerName));
        if (player == null) return "Spieler existiert nicht";

        if(guessNumber < player.Min || guessNumber > player.Max) return "Die Nummer liegt nicht im zu ratenden Bereich.";

        if (player.numberToGuess == guessNumber)
        {
            playerList.Remove(player);
            return $"Nummer wurde erraten {guessNumber} und Spieler {playerName} wurde gelöscht";
        }
        else
        {
            player.Tries--;
            if (player.Tries == 0)
            {
                string text = $"Die richtige Nummer wurde nicht erraten ({player.numberToGuess}) und Benutzer wird gelöscht";
                playerList.Remove(player);
                return text;
            }
        }
        return "Nummer wurde nicht erraten";

    }
}

public class Player
{
    public string Name {  get; set; }
    public int Min { get; set; }
    public int Max { get; set; }
    public int numberToGuess { get; set; }
    public int Tries {  get; set; }

    public Player(string name, int min, int max, int tries)
    {
        Name = name;
        numberToGuess = Random.Shared.Next(min,max+1);
        Tries = tries;
        Min = min;
        Max = max;
    }

    

}
