namespace VectorType.Models;

public class BossEnemy : Enemy
{
    public string Sentence { get; set; } = string.Empty;
    public List<string> Words { get; set; } = [];
    public int CurrentWordIndex { get; set; } = 0;
    public double TimeLimitSeconds { get; set; } = 40.0;
    public double TimeRemainingSeconds { get; set; } = 40.0;
    public int BossMaxHealth { get; set; } = 100;
    public int BossCurrentHealth { get; set; } = 100;
    public int BossShield { get; set; } = 50;
    public int BossMaxShield { get; set; } = 50;
    public string BossName { get; set; } = "NEXUS OVERLORD";

    public BossEnemy()
    {
        Type = EnemyType.Boss;
        X = 50.0;
        Y = 12.0; // Floats at the upper quadrant
        Speed = 0.5;
        PointsValue = 2500;
    }

    public void InitializeSentence(string sentence, double timeLimit = 40.0)
    {
        Sentence = sentence.Trim().ToUpperInvariant();
        Word = Sentence;
        Words = Sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        CurrentWordIndex = 0;
        TypedProgress = 0;
        TimeLimitSeconds = timeLimit;
        TimeRemainingSeconds = timeLimit;
        MaxHealth = Sentence.Length;
        CurrentHealth = Sentence.Length;
        BossMaxHealth = Sentence.Length;
        BossCurrentHealth = Sentence.Length;
    }

    public string CurrentTargetWord => CurrentWordIndex < Words.Count ? Words[CurrentWordIndex] : string.Empty;

    public int TotalCharsInSentence => Sentence.Length;

    public double HealthPercentage => BossMaxHealth > 0 
        ? Math.Clamp((double)BossCurrentHealth / BossMaxHealth * 100.0, 0, 100) 
        : 0;

    public double TimerPercentage => TimeLimitSeconds > 0 
        ? Math.Clamp(TimeRemainingSeconds / TimeLimitSeconds * 100.0, 0, 100) 
        : 0;

    public override bool RegisterKeystroke(char inputChar)
    {
        if (TypedProgress >= Sentence.Length)
            return false;

        char expected = Sentence[TypedProgress];

        // Handle spaces: if expected char is space, and player typed space, or if player typed next letter directly
        if (expected == ' ' && (inputChar == ' ' || char.ToUpperInvariant(inputChar) == ' '))
        {
            TypedProgress++;
            CurrentWordIndex = Math.Min(Words.Count - 1, CurrentWordIndex + 1);
            return true;
        }

        if (expected == ' ' && char.ToUpperInvariant(inputChar) != ' ')
        {
            // Auto skip space if user typed the next word's character
            if (TypedProgress + 1 < Sentence.Length && 
                char.ToUpperInvariant(Sentence[TypedProgress + 1]) == char.ToUpperInvariant(inputChar))
            {
                TypedProgress += 2;
                CurrentWordIndex = Math.Min(Words.Count - 1, CurrentWordIndex + 1);
                BossCurrentHealth = Math.Max(0, BossMaxHealth - TypedProgress);
                CheckBossDefeat();
                return true;
            }
        }

        if (char.ToUpperInvariant(expected) == char.ToUpperInvariant(inputChar))
        {
            TypedProgress++;
            BossCurrentHealth = Math.Max(0, BossMaxHealth - TypedProgress);

            // If the next character right after this is a space, advance word index
            if (TypedProgress < Sentence.Length && Sentence[TypedProgress] == ' ')
            {
                // Note: user can press space or continue typing next letter
            }

            CheckBossDefeat();
            return true;
        }

        return false;
    }

    private void CheckBossDefeat()
    {
        if (TypedProgress >= Sentence.Length)
        {
            BossCurrentHealth = 0;
            CurrentHealth = 0;
            IsDestroyed = true;
        }
    }

    public void AdvanceTime(double dt)
    {
        TimeRemainingSeconds = Math.Max(0, TimeRemainingSeconds - dt);
    }

    public bool IsTimeExpired => TimeRemainingSeconds <= 0 && !IsDestroyed;
}
