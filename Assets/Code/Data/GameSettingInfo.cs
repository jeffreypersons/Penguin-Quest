using UnityEngine;


public class GameSettingsInfo
{
    public int   NumberOfLives   { get; private set; }
    public float DifficultyLevel { get; private set; }
    public float SoundVolume     { get; private set; }
    public float MusicVolume     { get; private set; }
    public override string ToString()
    {
        return $"NumberOfLives is {NumberOfLives}, and difficulty is {DifficultyLevel}%, " +
               $"SoundVolume is {SoundVolume}%, and MusicVolume is {MusicVolume}%";
    }

    public GameSettingsInfo(int numberOfLives, int difficultyPercent, int soundVolumePercent, int musicVolumePercent)
    {
        if (ValidPositiveInteger(numberOfLives)    &&
            ValidatePercentage(difficultyPercent)  &&
            ValidatePercentage(soundVolumePercent) &&
            ValidatePercentage(musicVolumePercent))
        {
            NumberOfLives   = numberOfLives;
            DifficultyLevel = MathUtils.PercentToRatio(difficultyPercent);
            SoundVolume     = MathUtils.PercentToRatio(soundVolumePercent);
            MusicVolume     = MathUtils.PercentToRatio(musicVolumePercent);
        }
    }


    private bool ValidPositiveInteger(int value)
    {
        if (value < 0)
        {
            Debug.LogError($"`{nameof(value)}` must be an integer greater than 0, received {value} instead");
            return false;
        }
        return true;
    }

    private bool ValidatePercentage(int value)
    {
        if (!MathUtils.IsWithinRange(value, 0, 100))
        {
            Debug.LogError($"`{nameof(value)}` must be given as an integer percentage between 0 and 100, received {value} instead");
            return false;
        }
        return true;
    }
}
