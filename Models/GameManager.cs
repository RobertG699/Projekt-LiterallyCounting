//using MySQLiteApp.WordDataAccess;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Projekt_LiterallyCounting.Models;
namespace MySQLiteApp
{
    namespace Game
    {
        public class Player
        {
            internal string? username;
            internal int score;
            internal int fails;
            internal bool answerd;
            public Player ()
            {
                score = 0;
                fails = 0;
                answerd = false;
            }
        }           


        public class Game
        {
            private Player[] players = new Player[10];
            private string? currentWord;
            private int currentRound;
            public Game ()
            {
                currentRound = 0;
                currentWord ="";
            }
            public string GetWord()
            {
                int maxCount = MySQLiteApp.WordDataAccess.WordCount();
                currentWord = "";

                while (currentWord == "")
                {
                    Random r = new Random();
                    int rInt = r.Next(0, maxCount);
                    if (MySQLiteApp.WordDataAccess.wordExists(rInt))
                    {
                        WordViewModel result = MySQLiteApp.WordDataAccess.ReadWord(rInt);
                        if (result.Word.Length > 1)
                            {
                                currentWord = result.Word;
                            }
                    }
                }
                return currentWord;
            }

            public bool PlayerJoin(string user)
            {
                for(int i = 0; i<10; i++)
                {
                    if (string.IsNullOrWhiteSpace(players[i].username))
                    {
                        players[i].username = user;
                        players[i].score = 0;
                        break;
                    }
                    if(i == 9)
                    {
                        return false;
                    }
                }                
                return true;
            }

            private void AddScore(string user, int score)
            {
                players.Single(x => x.username == user).score = players.Single(x => x.username == user).score+score;
            }

            public bool Answer(string user, int ans, int time)
            {
                if (players.Single(x=>x.username==user).fails > 2)
                {
                    return false;
                }
                // time beinhaltet wieviel noch von den 20 sekunden übrig ist
                // max dreimal falsch antworten pro wort
                // eigene funktion für antwort verifikaton
                // spiel wird erstellt, spieler joinen, spiel wird gestartet, spieler geben antworten, spieler bekommen punkte
                // score wird erhöt, nächste runde nach countdown, letzte runde ende sieger wird gekürt.
                if (CountDidstinctLetters(currentWord) == ans)
                {
                    AddScore(user, time-players.Single(x=>x.username==user).fails);
                    players.Single(x=>x.username==user).answerd = true;
                    return false;
                }
                else 
                {
                    players.Single(x=>x.username==user).fails++;
                }
                return true;

            }
            public void StartRound()
            {
                RefreshRoundScoreTable();
                foreach (Player player in players)
                {
                    player.answerd = false;
                    player.fails = 0;
                }
                currentRound++;

            }


            public void StartGame()
            {
                currentRound = 0;
                foreach (Player player in players)
                {
                    player.score = 0;
                }
                StartRound();
            }
            public int GetCurrentRound()
            {
                return currentRound;
            }

            private int CountDidstinctLetters(string word)
            {
                    return word.Distinct().Count();
            }
            public void GetWinner()
            {
                foreach (Player player in players)
                {
                    player.answerd = false;
                    player.fails = 0;
                }
                currentRound++;

            }
            public Player[] RefreshRoundScoreTable()
            {
                Array.Sort(players,delegate(Player x, Player y) { return x.score.CompareTo(y.score); });
                return players;
            }
        }
    }
}