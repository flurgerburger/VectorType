namespace VectorType.Models;

public class DifficultyConfig
{
    public Difficulty Difficulty { get; init; }
    public double BaseSpeed { get; init; } = 4.5; // percentage units per second
    public double MaxSpeed { get; init; } = 12.0;
    public double SpeedScalingPerWave { get; init; } = 0.4;
    public double SpawnIntervalSeconds { get; init; } = 2.8;
    public double MinSpawnIntervalSeconds { get; init; } = 1.1;
    public int MaxConcurrentEnemies { get; init; } = 5;
    public int MinWordLength { get; init; } = 3;
    public int MaxWordLength { get; init; } = 6;
    public double BossTimeLimitSeconds { get; init; } = 45.0;
    public int PlayerBaseHealth { get; init; } = 100;
    public int PlayerBaseShield { get; init; } = 50;
    public double ScoreMultiplier { get; init; } = 1.0;

    public static DifficultyConfig GetConfig(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => new DifficultyConfig
        {
            Difficulty = Difficulty.Easy,
            BaseSpeed = 3.2,
            MaxSpeed = 7.0,
            SpeedScalingPerWave = 0.25,
            SpawnIntervalSeconds = 3.4,
            MinSpawnIntervalSeconds = 1.8,
            MaxConcurrentEnemies = 4,
            MinWordLength = 3,
            MaxWordLength = 5,
            BossTimeLimitSeconds = 55.0,
            PlayerBaseHealth = 120,
            PlayerBaseShield = 60,
            ScoreMultiplier = 1.0
        },
        Difficulty.Medium => new DifficultyConfig
        {
            Difficulty = Difficulty.Medium,
            BaseSpeed = 4.8,
            MaxSpeed = 11.0,
            SpeedScalingPerWave = 0.45,
            SpawnIntervalSeconds = 2.4,
            MinSpawnIntervalSeconds = 1.2,
            MaxConcurrentEnemies = 6,
            MinWordLength = 4,
            MaxWordLength = 8,
            BossTimeLimitSeconds = 40.0,
            PlayerBaseHealth = 100,
            PlayerBaseShield = 50,
            ScoreMultiplier = 1.5
        },
        Difficulty.Hard => new DifficultyConfig
        {
            Difficulty = Difficulty.Hard,
            BaseSpeed = 7.0,
            MaxSpeed = 16.0,
            SpeedScalingPerWave = 0.7,
            SpawnIntervalSeconds = 1.7,
            MinSpawnIntervalSeconds = 0.8,
            MaxConcurrentEnemies = 8,
            MinWordLength = 6,
            MaxWordLength = 12,
            BossTimeLimitSeconds = 28.0,
            PlayerBaseHealth = 80,
            PlayerBaseShield = 30,
            ScoreMultiplier = 2.2
        },
        _ => GetConfig(Difficulty.Medium)
    };
}
