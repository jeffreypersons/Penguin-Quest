using UnityEngine;
using PenguinQuest.Extensions;


namespace PenguinQuest
{
    public class PlayerProgressionInfo
    {
        public static int MIN_SCORE = 0;
        public static int MAX_SCORE = 10000;
        public static int MIN_LIVES_GIVEN = 1;
        public static int MAX_LIVES_GIVEN = 100;
        public static readonly string DEFAULT_LEVEL = "The Beginnings";

        public string LevelName  { get; private set; }
        public int    Score      { get; private set; }
        public int    Lives      { get; private set; }
        public int    LivesGiven { get; private set; }

        
        public override string ToString()
        {
            return $"Player is in level {LevelName}, " +
                   $"score is currently set to {Score}, " +
                   $"lives started at {LivesGiven} and is now {Lives}";
        }

        public PlayerProgressionInfo(int livesGiven)
        {
            Reset(livesGiven);
        }

        public void Reset(int livesGiven)
        {
            LevelName = DEFAULT_LEVEL;
            if (ValidateBounds(livesGiven, MIN_LIVES_GIVEN, MAX_LIVES_GIVEN))
            {
                Score      = MIN_SCORE;
                Lives      = livesGiven;
                LivesGiven = livesGiven;
            }
        }

        public void AddToScore(int score)
        {
            int newScore = Score + score;
            if (ValidateBounds(newScore, MIN_SCORE, MAX_SCORE))
            {
                Score = newScore;
            }
        }

        public void AddToLives(int lives)
        {
            int newLives = Lives + lives;
            if (ValidateBounds(newLives, MIN_LIVES_GIVEN, LivesGiven))
            {
                LivesGiven = newLives;
            }
        }


        private static bool ValidateBounds(int value, int min, int max)
        {
            if (!MathExtensions.IsWithinRange(value, min, max))
            {
                Debug.LogError($"{nameof(value)} must be within range of [{min} and {max}], received {value} instead");
                return false;
            }
            return true;
        }
    }
}
