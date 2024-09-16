namespace Projekt_LiterallyCounting.Models
{
    public class AllWordsViewModel
    {
        public IEnumerable<WordViewModel> allWords { get; set; }
        public WordViewModel newWord { get; set; }
    }
}