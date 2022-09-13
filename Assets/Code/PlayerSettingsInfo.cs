using UnityEngine;
using System.Diagnostics.Contracts;
using PQ.Common;


namespace PQ
{
    public struct PlayerSettingsInfo
    {
        public int   NumberOfLives   { get; private set; }
        public float DifficultyLevel { get; private set; }
        public float SoundVolume     { get; private set; }
        public float MusicVolume     { get; private set; }
        public override string ToString() =>
            $"NumberOfLives is {NumberOfLives}, and difficulty is {DifficultyLevel}%, " +
            $"SoundVolume is {SoundVolume}%, and MusicVolume is {MusicVolume}%";


        public PlayerSettingsInfo(int numberOfLives, int difficultyPercent, int soundVolumePercent, int musicVolumePercent)
        {
            if (ValidPositiveInteger(numberOfLives)    &&
                ValidatePercentage(difficultyPercent)  &&
                ValidatePercentage(soundVolumePercent) &&
                ValidatePercentage(musicVolumePercent))
            {
                NumberOfLives   = numberOfLives;
                DifficultyLevel = PercentToRatio(difficultyPercent);
                SoundVolume     = PercentToRatio(soundVolumePercent);
                MusicVolume     = PercentToRatio(musicVolumePercent);
            }
            else
            {
                NumberOfLives   = default;
                DifficultyLevel = default;
                SoundVolume     = default;
                MusicVolume     = default;
            }
        }


        private static bool ValidPositiveInteger(int value)
        {
            if (value < 0)
            {
                Debug.LogError($"`{nameof(value)}` must be an integer greater than 0, received {value} instead");
                return false;
            }
            return true;
        }

        private static bool ValidatePercentage(int value)
        {
            if (value < 0 || value > 100)
            {
                Debug.LogError($"`{nameof(value)}` must be given as an integer percentage between 0 and 100, received {value} instead");
                return false;
            }
            return true;
        }

        
        [Pure] private static float PercentToRatio(float percent) =>
            Mathf.Approximately(percent, 0.00f) ? 0.00f : percent / 100.00f;
    }
}
