namespace AIGuesserGame.Models
{
    public class GameData
    {
        public int lifelines { get; set; }
        public int promptCount { get; set; }
        public int wordsLeft { get; set; }

        public List<String>? promptWords { get; set; }

        public List<String>? promptWordsGuessed { get; set; }

        public List<String>?  guessedWords { get; set; }

        public string? imageData { get; set; }
    }
}
