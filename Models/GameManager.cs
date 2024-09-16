//using MySQLiteApp.WordDataAccess;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Projekt_LiterallyCounting.Models;
namespace MySQLiteApp
{
    namespace Game
    {
        public class Player
        {
            internal string username;
            internal int score;
            internal int fails;
            internal bool answerd;
            public Player ()
            {
                username = "";
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
                for(int i = 0; i<10; i++)
                {
                    players[i] = new();
                }
                currentRound = 0;
                currentWord ="";
            }
            public string GetWord()
            {
                int maxCount = MySQLiteApp.WordDataAccess.IdMax();
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
                foreach (Player player in players)
                {
                    if (player.username == user )
                    {
                        player.score = player.score+score;
                    }
                }
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
            public Player GetWinner()
            {
                RefreshRoundScoreTable();
                return players [0];
            }
            public Player[] RefreshRoundScoreTable()
            {
                Array.Sort(players,delegate(Player x, Player y) { return y.score.CompareTo(x.score); });
                return players;
            }
        }
    }
}