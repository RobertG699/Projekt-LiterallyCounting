using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;


namespace GlobalParameter
{
    public class Wraper   
    {
        [JsonProperty("lemma")]
        public string? lemma { get; set; }
        [JsonProperty("date")]
        public string? date{ get; set; }
        [JsonProperty("url")]
        public string? url{ get; set; }
        [JsonProperty("pos")]
        public string? pos{ get; set; }
        [JsonProperty("type")]
        public string? type{ get; set; }

    }
    
    public class WordsJson   
    {
        [JsonProperty("lema")]
        public string? lemma { get; set; }
        [JsonProperty("date")]
        public string? date{ get; set; }
        [JsonProperty("url")]
        public string? url{ get; set; }
        [JsonProperty("pos")]
        public string? pos{ get; set; }
        [JsonProperty("type")]
        public string? type{ get; set; }
        
        public Dictionary<string, object> jsonDict = new Dictionary<string, object>();

        public static List<Wraper>? GetListfromJson()
        {
            //string fileName = "Projekt_LiterallyCounting//Models//GlobalParameters//Json//dwds_lemmata_2024-08-28.json";C:\Users\tohmer\Documents\GitHub\Literally-Counting\Projekt_LiterallyCounting\Models\GlobalParameters\Json\dwds_lemmata_2024-08-28.json
            var path = Path.Combine(Directory.GetCurrentDirectory(),"Models","GlobalParameters","Json", "dwds_lemmata_2024-08-28.json");
            string jsonString = File.ReadAllText(path);
            //Console.WriteLine(jsonString);
            //var words = new List<WordsJson>;
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return null;
            }
            var words = new List<Wraper>();

            words = JsonConvert.DeserializeObject<List<Wraper>>(jsonString);

            if (words != null && words.Count < 0)  
            {
                return null;
            } 
            return words;

            
        }

        public static List<Wraper>? Filter(List<Wraper> words)
        {
            if (words == null)
            {
                return null;
            }

            words = words.Where(x=> 
            Regex.IsMatch(x.lemma, @"^[a-zA-Z]+$") &&
            (
                x.pos == "Adjektiv" ||
                x.pos == "Eigenname" ||
                x.pos == "Adverb" ||
                x.pos == "Kardinalzahl" ||
                x.pos == "Bruchzahl" ||
                x.pos == "Indefinitpronomen" ||
                x.pos == "Verb" ||
                x.pos == "partizipiales Adjektiv" ||
                x.pos == "Substantiv"
            )
            )
        .ToList();

            


            return words;
        }
        public static void FillDatabaseFromJson()
        {
            var words = new List<Wraper>();
            words = GetListfromJson();
            words = Filter(words);

            MySQLiteApp.WordDataAccess.readWords();
            //MySQLiteApp.WordDataAccess.recreateWordTable();

            foreach (var word in words)
            {

                MySQLiteApp.WordDataAccess.insertWord(word.lemma,word.pos,word.type,null);
                Console.WriteLine(word.pos + " lema: " + word.lemma);
            }
            
            MySQLiteApp.WordDataAccess.readWords();
        }
    }
    

}