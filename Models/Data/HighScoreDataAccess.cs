//Robert Glowacki wird nicht verwendet//
using System.Data.SQLite;
using Projekt_LiterallyCounting.Models;

namespace MySQLiteApp
{
    public static class HighScoreDataAccess
    {
        private static SQLiteConnection createUserConnection()
        {
            string db = "./Models/Data/users.db";

            string dbPath = Path.Combine(Environment.CurrentDirectory, db);
            string connString = string.Format("Data Source={0}", dbPath);

            // Create a connection to the database
            var con = new SQLiteConnection(connString);

            return con;
        }

        public static void createuserHighScoreTable()
        {
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = @"CREATE TABLE highScore (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            email TEXT NOT NULL,
                            points INTEGER NOT NULL,
                            FOREIGN KEY (email) REFERENCES users(email)
                            );";
            cmd.ExecuteNonQuery();

            con.Close();
        }

        public static bool emailExists(string email){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"SELECT * FROM highScore WHERE email = @Email LIMIT 1";
            cmd.Parameters.AddWithValue("@Email", email);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if(rdr.HasRows){
                con.Close();
                return true;
            }
            else{
                con.Close();
                return false;
            }
        }

        public static void insertUser(string email, int points)
        {
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"INSERT INTO highScore(email, points) VALUES( @Email, @Points)";
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Points", points);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        public static void updatePoints(int points, string email){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"Update highScore SET points = @Points WHERE email = @Email";
            cmd.Parameters.AddWithValue("@Points", points);
            cmd.Parameters.AddWithValue("@Email", email);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            con.Close();
        }

        public static List<PointsViewModel> readHighscores()
        {
            List<PointsViewModel> scores = new List<PointsViewModel>();

            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM highScore";
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                PointsViewModel score = new PointsViewModel
                {
                    email = rdr["email"].ToString() == null ? "" : rdr["email"].ToString(),
                    points = Int32.Parse(rdr["points"].ToString())
                };

                scores.Add(score);
            }

            foreach(PointsViewModel score in scores)
            {
                Console.WriteLine($"Mail: {score.email} | Points: {score.points}");
            }

            con.Close();
            return scores;
        }
    }
}