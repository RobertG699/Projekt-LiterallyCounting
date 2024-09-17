using System.Data.SQLite;
using Projekt_LiterallyCounting.Models;

namespace MySQLiteApp
{
    public static class WordDataAccess
    {
        private static SQLiteConnection createWordConnection(){
            string db = "./Models/Data/words.db";

            string dbPath = Path.Combine(Environment.CurrentDirectory, db);
            string connString = string.Format("Data Source={0}", dbPath);
            
            // Create a connection to the database
            var con = new SQLiteConnection(connString);

            return con;
        }

        private static bool selectNewStatus(string word, SQLiteConnection con){
            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = "SELECT status FROM words WHERE word = @Word";
            cmd.Parameters.AddWithValue("@Word", word);

            object result = cmd.ExecuteScalar();

            if (result == DBNull.Value){
                return true;
            }
            else if (result is bool value){
                return value;
            }
            else{
                return false;
            }
        }

        public static void createuserWordTable()
        {
            SQLiteConnection con = createWordConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = @"CREATE TABLE words (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            word TEXT NOT NULL UNIQUE,
                            pos TEXT NOT NULL,
                            type TEXT NOT NULL,
                            status BOOLEAN
                            )";
            cmd.ExecuteNonQuery();

            con.Close();
        }
        public static void recreateWordTable()
        {
            SQLiteConnection con = createWordConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            
            cmd.CommandText = @"drop TABLE words";
            cmd.ExecuteNonQuery();

            con.Close();
            createuserWordTable();
        }

        public static void insertWord(string word, string pos, string type, bool? status = null)
        {
            SQLiteConnection con = createWordConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = $"INSERT INTO words(word, pos, type, status) VALUES(@Word, @Pos, @Type, @Status)";
            cmd.Parameters.AddWithValue("@Word", word);
            cmd.Parameters.AddWithValue("@Pos", pos);
            cmd.Parameters.AddWithValue("@Type", type);
            cmd.Parameters.AddWithValue("@Status", status);
            try{
                cmd.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Console.WriteLine(word+" konnte nicht hinzugefügt werden. (" +word+", "+pos+", "+type+", "+status+")"+e.Message);
                Console.WriteLine(e.StackTrace);
            }
            

            con.Close();
        }

        public static bool wordExists(string word){
            SQLiteConnection con = createWordConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"SELECT * FROM words WHERE word = @Word LIMIT 1";
            cmd.Parameters.AddWithValue("@Word", word);
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
        public static bool wordExists(int id){
            SQLiteConnection con = createWordConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"SELECT * FROM words WHERE id = @id LIMIT 1";
            cmd.Parameters.AddWithValue("@id", id);
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

        public static void updateWord(string wordNew, string word){
            
            SQLiteConnection con = createWordConnection();
            con.Open();

            bool statusNew = selectNewStatus(word, con);

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = $"Update words SET word = @WordNew, status = @StatusNew WHERE word = @Word";
            cmd.Parameters.AddWithValue("@WordNew", wordNew);
            cmd.Parameters.AddWithValue("@Word", word);
            cmd.Parameters.AddWithValue("@StatusNew", statusNew);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            con.Close();
        }

        public static void deleteWord(string word){
            SQLiteConnection con = createWordConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"DELETE FROM words WHERE word = @Word";
            cmd.Parameters.AddWithValue("@Word", word);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            con.Close();
        }

        
        public static void deletesmalWords(){
            SQLiteConnection con = createWordConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"DELETE FROM words WHERE LENGTH(word) < 3";
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            con.Close();
        }

        public static List<WordViewModel> readWords()
        {
            List<WordViewModel> words = new List<WordViewModel>();

            SQLiteConnection con = createWordConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM words";
            using SQLiteDataReader rdr = cmd.ExecuteReader();
            
            while (rdr.Read())
            {
                WordViewModel word = new WordViewModel
                {
                    Word = rdr["word"].ToString() == null ? "" : rdr["word"].ToString(),
                    Pos = rdr["pos"].ToString() == null ? "" : rdr["pos"].ToString(),
                    Type = rdr["type"].ToString() == null ? "" : rdr["type"].ToString(),
                    Status = rdr["status"].ToString() == null ? "Original" : rdr["status"].ToString(),
                };

                words.Add(word);
            }

            /*foreach(WordViewModel word in words)
            {
                Console.WriteLine($"Word: {word.Word} | Position: {word.Pos} | Type: {word.Type} | Status: {word.Status}");
            }*/

            con.Close();
            return words;
        }
        public static WordViewModel ReadWord(int id)
        {
            SQLiteConnection con = createWordConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = $"SELECT * FROM words WHERE id = @id LIMIT 1";
            cmd.Parameters.AddWithValue("@id", id);     
            WordViewModel result = new();
            try
            {
                using SQLiteDataReader rdr = cmd.ExecuteReader();
                
                
                while (rdr.Read())
                {
                    WordViewModel word = new WordViewModel
                    {
                        Word = rdr["word"].ToString() == null ? "" : rdr["word"].ToString(),
                        Pos = rdr["pos"].ToString() == null ? "" : rdr["pos"].ToString(),
                        Type = rdr["type"].ToString() == null ? "" : rdr["type"].ToString(),
                        Status = rdr["status"].ToString() == null ? "Original" : rdr["status"].ToString(),
                    };

                    result = word;
                }    
            }  
            catch(Exception)
            {

            }    

            con.Close();
            return result;
        }
        public static int IdMax()
        {
            int count = 0;
            SQLiteConnection con = createWordConnection();
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = $"SELECT MAX(ID) FROM words";
            using (SQLiteDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    //Console.WriteLine("TTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTest");
                    //Console.WriteLine(rdr["MAX(ID)"].ToString() == null ? "" : rdr["MAX(ID)"].ToString());
                    int.TryParse( rdr["MAX(ID)"].ToString() == null ? "" : rdr["MAX(ID)"].ToString(), out count);
                }
                con.Close();
            }
            return count;
        }
    }
}