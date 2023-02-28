using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIGuesserGame.Models
{
    /// <summary>
    /// The game difficulties.
    /// Author(s): Lukasz Bednarek
    /// Date: November 27, 2022
    /// </summary>
    public enum GameDifficulty { Easy, Normal, Expert}

    /// <summary>
    /// The statistics saved from a game of AI guesser onto the User.
    /// Author(s): Lukasz Bednarek
    /// Date: November 27, 2022
    /// </summary>
    public class GameStats
    {
        /// <summary>
        /// Unqiue identifier for a game's stats.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The difficulty of the game.
        /// </summary>
        [Required]
        [Column(TypeName = "nvarchar(16)")]
        public GameDifficulty GameDifficulty { get; set; }
        /// <summary>
        /// The total guesses before the game reached its end state.
        /// </summary>
        [Required]
        public int TotalGuesses { get; set; }
        /// <summary>
        /// The number of correct keyword guesses in the game.
        /// </summary>
        [Required]
        public int GuessHits { get; set; }
        /// <summary>
        /// The total number of prompts needed to be guessed.
        /// </summary>
        [Required]
        public int TotalPrompts { get; set; }
        /// <summary>
        /// If the user won the game.
        /// </summary>
        [Required]
        public bool WonGame { get; set; }
        /// <summary>
        /// If the user correctly guessed every prompt without fail.
        /// </summary>
        [Required]
        public bool PerfectGame { get; set; }
        /// <summary>
        /// The identifier for which user played the game.
        /// </summary>
        [Required]
        [ForeignKey("AspNetUserId")]
        public string UserId { get; set; }
    }
}
