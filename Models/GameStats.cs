namespace VectorType.Models;

public class GameStats
{
    public long Score { get; set; } = 0;
    public int Combo { get; set; } = 0;
    public int MaxCombo { get; set; } = 0;
    public int TotalKeysTyped { get; set; } = 0;
    public int CorrectKeysTyped { get; set; } = 0;
    public int WordsCompleted { get; set; } = 0;
    public int EnemiesBreached { get; set; } = 0;
    public int BossesDefeated { get; set; } = 0;
    public int DamageTaken { get; set; } = 0;
    public double ElapsedSeconds { get; set; } = 0;
    public double PeakWpm { get; set; } = 0;

    public double Accuracy => TotalKeysTyped > 0 
        ? Math.Clamp((double)CorrectKeysTyped / TotalKeysTyped * 100.0, 0, 100) 
        : 100.0;

    public double CurrentWpm
    {
        get
        {
            if (ElapsedSeconds < 1.0) return 0;
            double minutes = ElapsedSeconds / 60.0;
            // Standard typing WPM: (correct chars / 5) / minutes
            double wpm = (CorrectKeysTyped / 5.0) / minutes;
            return Math.Round(Math.Clamp(wpm, 0, 300), 1);
        }
    }

    public double ComboMultiplier => Combo switch
    {
        >= 30 => 4.0,
        >= 20 => 3.0,
        >= 10 => 2.0,
        >= 5 => 1.5,
        _ => 1.0
    };

    public void RegisterKeypress(bool correct)
    {
        TotalKeysTyped++;
        if (correct)
        {
            CorrectKeysTyped++;
            double currentWpm = CurrentWpm;
            if (currentWpm > PeakWpm)
            {
                PeakWpm = currentWpm;
            }
        }
    }

    public void RegisterWordDefeated(int basePoints)
    {
        Combo++;
        if (Combo > MaxCombo) MaxCombo = Combo;
        WordsCompleted++;
        Score += (long)(basePoints * ComboMultiplier);
    }

    public void BreakCombo()
    {
        Combo = 0;
    }

    public void Reset()
    {
        Score = 0;
        Combo = 0;
        MaxCombo = 0;
        TotalKeysTyped = 0;
        CorrectKeysTyped = 0;
        WordsCompleted = 0;
        EnemiesBreached = 0;
        BossesDefeated = 0;
        DamageTaken = 0;
        ElapsedSeconds = 0;
        PeakWpm = 0;
    }
}
