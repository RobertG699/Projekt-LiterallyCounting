//Robert Glowacki//
using System.Data.SQLite;
using Projekt_LiterallyCounting.Models;

namespace MySQLiteApp
{
    public static class UserDataAccess
    {
        private static SQLiteConnection createUserConnection(){
            string db = "./Models/Data/users.db";

            string dbPath = Path.Combine(Environment.CurrentDirectory, db);
            string connString = string.Format("Data Source={0}", dbPath);
            
            // Create a connection to the database
            var con = new SQLiteConnection(connString);

            return con;
        }

        public static void createuserUserTable()
        {
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = @"CREATE TABLE users (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            email TEXT NOT NULL,
                            password TEXT NOT NULL,
                            is_admin BOOLEAN NOT NULL,
                            is_blocked BOOLEAN NOT NULL,
                            points INTEGER NOT NULL
                            )";
            cmd.ExecuteNonQuery();

            con.Close();
        }

        public static bool userIsAdmin(string email){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = "SELECT is_admin FROM users WHERE email = @Email";
            cmd.Parameters.AddWithValue("@Email", email);

            object result = cmd.ExecuteScalar();
            con.Close();

            if (result is bool value){
                return value;
            }
            else{
                return false;
            }
        }

        public static bool userIsBlocked(string email){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = "SELECT is_blocked FROM users WHERE email = @Email";
            cmd.Parameters.AddWithValue("@Email", email);

            object result = cmd.ExecuteScalar();
            con.Close();

            if (result is bool value){
                return value;
            }
            else{
                return false;
            }
        }

        public static void insertUser(string email, string password, bool admin, bool blocked)
        {
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"INSERT INTO users(email, password, is_admin, is_blocked, points) VALUES( @Email, @Pw, @Admin, @Blocked, 0)";
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Pw", password);
            cmd.Parameters.AddWithValue("@Admin", admin);
            cmd.Parameters.AddWithValue("@Blocked", blocked);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        public static void insertUser(string email, string password, bool admin, bool blocked, int points)
        {
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"INSERT INTO users(email, password, is_admin, is_blocked, points) VALUES( @Email, @Pw, @Admin, @Blocked, @Points)";
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Pw", password);
            cmd.Parameters.AddWithValue("@Admin", admin);
            cmd.Parameters.AddWithValue("@Blocked", blocked);
            cmd.Parameters.AddWithValue("@Points", points);
            cmd.ExecuteNonQuery();

            con.Close();
        }

        public static bool emailExists(string email){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"SELECT * FROM users WHERE email = @Email LIMIT 1";
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

        /*public static bool validatedUser(string email, string password){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"SELECT * FROM users WHERE email = @Email AND password = @Password LIMIT 1";
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Password", password);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if(rdr.HasRows){
                con.Close();
                return true;
            }
            else{
                con.Close();
                return false;
            }
        }*/

        public static void updateUserEmail(string emailNew, string email){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"Update users SET email = @EmailNew WHERE email = @Email";
            cmd.Parameters.AddWithValue("@EmailNew", emailNew);
            cmd.Parameters.AddWithValue("@Email", email);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            con.Close();
        }

        public static void updateUserPoints(string email, int points){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"Update users SET points = points + @Points WHERE email = @Email";
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Points", points);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            con.Close();
        }

        public static void updateUserPassword(string pwNew, string email){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"Update users SET password = @PwNew WHERE email = @Email";
            cmd.Parameters.AddWithValue("@PwNew", pwNew);
            cmd.Parameters.AddWithValue("@Email", email);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            con.Close();
        }

        public static void unblockUser(string email){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"Update users SET is_blocked = False WHERE email = @Email";
            cmd.Parameters.AddWithValue("@Email", email);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            con.Close();
        }

        public static void deleteUser(string email){
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"DELETE FROM users WHERE email = @Email";
            cmd.Parameters.AddWithValue("@Email", email);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            con.Close();
        }

        public static List<UserViewModel> readUsers(bool orderByPoints = false)
        {
            List<UserViewModel> users = new List<UserViewModel>();

            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            if(orderByPoints){
                cmd.CommandText = "SELECT * FROM users ORDER BY points DESC LIMIT 5";
            }
            else{
                cmd.CommandText = "SELECT * FROM users";
            }
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                UserViewModel user = new UserViewModel
                {
                    Email = rdr["email"].ToString() == null ? "" : rdr["email"].ToString(),
                    IsAdmin = Convert.ToBoolean(rdr["is_admin"]),
                    Password = "",
                    Blocked = Convert.ToBoolean(rdr["is_blocked"]),
                    Points = Int32.Parse(rdr["points"].ToString())
                };

                users.Add(user);
            }

            con.Close();
            return users;
        }

        public static string getPasswordHash(string email)
        {
            SQLiteConnection con = createUserConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"SELECT password FROM users WHERE email = @Email LIMIT 1";
            cmd.Parameters.AddWithValue("@Email", email);

            object result = cmd.ExecuteScalar();
            con.Close();

            if (result is string value){
                return value;
            }
            else{
                return "";
            }
        }
    }
}