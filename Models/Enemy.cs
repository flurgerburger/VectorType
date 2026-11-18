namespace VectorType.Models;

public class Enemy
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Word { get; set; } = string.Empty;
    public double X { get; set; } // Horizontal percentage (5% to 90%)
    public double Y { get; set; } // Vertical percentage (0% = top, 88% = defense line)
    public double BaseX { get; set; }
    public double Speed { get; set; } // Percent units per second
    public int MaxHealth { get; set; } = 1;
    public int CurrentHealth { get; set; } = 1;
    public EnemyType Type { get; set; } = EnemyType.Drone;
    public int PointsValue { get; set; } = 100;
    public int TypedProgress { get; set; } = 0; // Number of letters correctly typed
    public bool IsTargeted { get; set; } = false;
    public bool IsDestroyed { get; set; } = false;
    public double SineOffset { get; set; } = 0.0;
    public double SineFrequency { get; set; } = 1.8;
    public double SineAmplitude { get; set; } = 4.0;
    public int LaneIndex { get; set; } = 0;

    public string TypedSubstring => TypedProgress <= Word.Length 
        ? Word[..TypedProgress] 
        : Word;

    public string RemainingSubstring => TypedProgress < Word.Length 
        ? Word[TypedProgress..] 
        : string.Empty;

    public char? NextChar => TypedProgress < Word.Length 
        ? Word[TypedProgress] 
        : null;

    public bool IsFullyTyped => TypedProgress >= Word.Length;

    public bool MatchesNextChar(char inputChar)
    {
        if (NextChar == null) return false;
        return char.ToLowerInvariant(NextChar.Value) == char.ToLowerInvariant(inputChar);
    }

    public virtual bool RegisterKeystroke(char inputChar)
    {
        if (MatchesNextChar(inputChar))
        {
            TypedProgress++;
            if (IsFullyTyped)
            {
                CurrentHealth = 0;
                IsDestroyed = true;
            }
            return true;
        }
        return false;
    }

    public virtual void ResetProgress()
    {
        TypedProgress = 0;
        IsTargeted = false;
    }
}
