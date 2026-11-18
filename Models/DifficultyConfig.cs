namespace VectorType.Models;

public class DifficultyConfig
{
    public Difficulty Difficulty { get; init; }
    public double BaseSpeed { get; init; } = 3.4; // percentage units per second (moderate, readable velocity)
    public double MaxSpeed { get; init; } = 5.2;
    public double SpeedScalingPerWave { get; init; } = 0.12;
    public double SpawnIntervalSeconds { get; init; } = 1.3; // High-frequency spawning
    public double MinSpawnIntervalSeconds { get; init; } = 0.7;
    public int MaxConcurrentEnemies { get; init; } = 8;
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
            BaseSpeed = 2.6,
            MaxSpeed = 4.2,
            SpeedScalingPerWave = 0.1,
            SpawnIntervalSeconds = 1.9,
            MinSpawnIntervalSeconds = 1.1,
            MaxConcurrentEnemies = 6,
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
            BaseSpeed = 3.4,
            MaxSpeed = 5.2,
            SpeedScalingPerWave = 0.12,
            SpawnIntervalSeconds = 1.3,
            MinSpawnIntervalSeconds = 0.7,
            MaxConcurrentEnemies = 8,
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
            BaseSpeed = 4.2,
            MaxSpeed = 6.4,
            SpeedScalingPerWave = 0.15,
            SpawnIntervalSeconds = 0.95,
            MinSpawnIntervalSeconds = 0.45,
            MaxConcurrentEnemies = 11,
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
