using System;
using System.Data.SQLite;

namespace MySQLiteApp
{
    
    public static class DataAccess
    {
        public static void fillDBfromJson()
        {
        
            string dbPath = Path.Combine(Environment.CurrentDirectory, "./example.db");
            string connString = string.Format("Data Source={0}", dbPath);
            
            // Create a connection to the database
            using var con = new SQLiteConnection(connString);
            con.Open();

            // Create a command object  
            using var cmd = new SQLiteCommand(con);
            
            // Example: Create a table
            /*cmd.CommandText = @"CREATE TABLE IF NOT EXISTS users (
                                id INTEGER PRIMARY KEY,
                                name TEXT,
                                age INTEGER)";
            cmd.ExecuteNonQuery();

            // Example: Insert data into the table
            cmd.CommandText = "INSERT INTO users(name, age) VALUES('Marry Doe', 25)";
            cmd.ExecuteNonQuery();*/

            // Example: Query the data
            cmd.CommandText = "SELECT * FROM users";
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Console.WriteLine($"ID: {rdr["id"]}, Name: {rdr["name"]}, Age: {rdr["age"]}");
            }

        }
    }

    public static class ExampleDB
    {
        public static void exampleTask()
        {
            string dbPath = Path.Combine(Environment.CurrentDirectory, "./example.db");
            string connString = string.Format("Data Source={0}", dbPath);
            
            // Create a connection to the database
            using var con = new SQLiteConnection(connString);
            con.Open();

            // Create a command object
            using var cmd = new SQLiteCommand(con);

            // Example: Create a table
            /*cmd.CommandText = @"CREATE TABLE IF NOT EXISTS users (
                                id INTEGER PRIMARY KEY,
                                name TEXT,
                                age INTEGER)";
            cmd.ExecuteNonQuery();

            // Example: Insert data into the table
            cmd.CommandText = "INSERT INTO users(name, age) VALUES('Marry Doe', 25)";
            cmd.ExecuteNonQuery();*/

            // Example: Query the data
            cmd.CommandText = "SELECT * FROM users";
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Console.WriteLine($"ID: {rdr["id"]}, Name: {rdr["name"]}, Age: {rdr["age"]}");
            }
        }
    }
}