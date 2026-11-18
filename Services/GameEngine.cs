namespace VectorType.Services;

using System.Diagnostics;
using VectorType.Models;

public enum GameState
{
    Ready,
    Playing,
    Paused,
    BossFight,
    Victory,
    GameOver
}

public class GameEngine : IAsyncDisposable
{
    private readonly IWordBankService _wordBank;
    private readonly ISoundService _sound;
    private readonly Random _rnd = new();

    private CancellationTokenSource? _gameCts;
    private Task? _gameLoopTask;
    private readonly Stopwatch _frameStopwatch = new();

    public GameState State { get; private set; } = GameState.Ready;
    public GameMode Mode { get; private set; } = GameMode.Normal;
    public Difficulty Difficulty { get; private set; } = Difficulty.Medium;
    public DifficultyConfig Config { get; private set; } = DifficultyConfig.GetConfig(Difficulty.Medium);

    // Player Stats & Health Pools
    public int PlayerHealth { get; private set; } = 100;
    public int PlayerMaxHealth { get; private set; } = 100;
    public int PlayerShield { get; private set; } = 50;
    public int PlayerMaxShield { get; private set; } = 50;
    public double ShieldRegenTimer { get; private set; } = 0.0;
    private const double ShieldRegenDelay = 4.0; // Seconds without damage before regen

    public GameStats Stats { get; } = new();

    // Wave Management
    public int CurrentWave { get; private set; } = 1;
    public int TotalNormalWaves { get; private set; } = 3;
    public int EnemiesSpawnedInWave { get; private set; } = 0;
    public int TotalEnemiesInWave { get; private set; } = 8;
    public double SpawnTimer { get; private set; } = 0.0;

    // Entities
    private readonly List<Enemy> _enemies = [];
    public IReadOnlyList<Enemy> Enemies => _enemies;

    public BossEnemy? CurrentBoss { get; private set; }
    public Enemy? TargetEnemy { get; private set; }

    // Visual FX
    private readonly List<Particle> _particles = [];
    public IReadOnlyList<Particle> Particles => _particles;

    private readonly List<LaserBeam> _lasers = [];
    public IReadOnlyList<LaserBeam> Lasers => _lasers;

    public bool IsScreenShaking => ScreenShakeTimer > 0;
    public double ScreenShakeTimer { get; private set; } = 0.0;
    public double DangerWarningIntensity { get; private set; } = 0.0; // 0 to 1

    // Events for reactive Blazor rendering
    public event Action? StateChanged;
    public event Action? ScreenShaken;

    public GameEngine(IWordBankService wordBank, ISoundService sound)
    {
        _wordBank = wordBank;
        _sound = sound;
    }

    public async Task StartGameAsync(GameMode mode, Difficulty difficulty)
    {
        await StopGameLoopAsync();

        Mode = mode;
        Difficulty = difficulty;
        Config = DifficultyConfig.GetConfig(difficulty);

        PlayerMaxHealth = Config.PlayerBaseHealth;
        PlayerHealth = PlayerMaxHealth;
        PlayerMaxShield = Config.PlayerBaseShield;
        PlayerShield = PlayerMaxShield;
        ShieldRegenTimer = 0;

        CurrentWave = 1;
        EnemiesSpawnedInWave = 0;
        TotalEnemiesInWave = CalculateEnemiesForWave(CurrentWave);
        SpawnTimer = 0.5; // Quick initial spawn

        Stats.Reset();
        _enemies.Clear();
        _particles.Clear();
        _lasers.Clear();
        TargetEnemy = null;
        CurrentBoss = null;
        ScreenShakeTimer = 0;

        State = GameState.Playing;
        await _sound.InitializeAsync();

        _gameCts = new CancellationTokenSource();
        _frameStopwatch.Restart();
        _gameLoopTask = RunGameLoopAsync(_gameCts.Token);

        NotifyStateChanged();
    }

    public void PauseGame()
    {
        if (State is GameState.Playing or GameState.BossFight)
        {
            State = GameState.Paused;
            NotifyStateChanged();
        }
    }

    public void ResumeGame()
    {
        if (State is GameState.Paused)
        {
            State = CurrentBoss != null ? GameState.BossFight : GameState.Playing;
            _frameStopwatch.Restart();
            NotifyStateChanged();
        }
    }

    private int CalculateEnemiesForWave(int wave) => Mode switch
    {
        GameMode.Normal => wave switch
        {
            1 => 6,
            2 => 10,
            _ => 14
        },
        GameMode.Endless => 6 + (wave * 3),
        _ => 8
    };

    private double GetCurrentSpawnInterval()
    {
        double reduction = (CurrentWave - 1) * 0.15;
        return Math.Max(Config.MinSpawnIntervalSeconds, Config.SpawnIntervalSeconds - reduction);
    }

    private double GetCurrentSpeed()
    {
        double speed = Config.BaseSpeed + ((CurrentWave - 1) * Config.SpeedScalingPerWave);
        return Math.Min(Config.MaxSpeed, speed);
    }

    private async Task RunGameLoopAsync(CancellationToken ct)
    {
        // 60 FPS target loop (~16.6ms)
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(16));

        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (!await timer.WaitForNextTickAsync(ct))
                    break;

                double dt = _frameStopwatch.Elapsed.TotalSeconds;
                _frameStopwatch.Restart();

                // Clamp delta time to avoid large jumps if browser tab paused or tabbed away
                if (dt > 0.1) dt = 0.05;

                if (State is GameState.Playing or GameState.BossFight)
                {
                    await UpdateAsync(dt);
                    NotifyStateChanged();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameEngine Loop Error]: {ex.Message}");
            }
        }
    }

    private async Task UpdateAsync(double dt)
    {
        Stats.ElapsedSeconds += dt;

        // Screen Shake decay
        if (ScreenShakeTimer > 0)
        {
            ScreenShakeTimer = Math.Max(0, ScreenShakeTimer - dt);
        }

        // Shield regeneration logic
        ShieldRegenTimer += dt;
        if (ShieldRegenTimer >= ShieldRegenDelay && PlayerShield < PlayerMaxShield)
        {
            PlayerShield = Math.Min(PlayerMaxShield, PlayerShield + (int)Math.Ceiling(10 * dt));
        }

        // Update visual FX (Lasers and Particles)
        UpdateFx(dt);

        // Danger distance warning calculation
        UpdateDangerIntensity();

        if (State == GameState.Playing)
        {
            await UpdatePlayingStateAsync(dt);
        }
        else if (State == GameState.BossFight)
        {
            await UpdateBossFightStateAsync(dt);
        }
    }

    private async Task UpdatePlayingStateAsync(double dt)
    {
        // 1. Enemy Spawning Logic
        SpawnTimer += dt;
        double interval = GetCurrentSpawnInterval();

        if (SpawnTimer >= interval && 
            _enemies.Count < Config.MaxConcurrentEnemies && 
            (Mode == GameMode.Endless || EnemiesSpawnedInWave < TotalEnemiesInWave))
        {
            SpawnTimer = 0;
            SpawnEnemy();
        }

        // 2. Enemy Movement & Collision with Player Defense Perimeter
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            
            // Move downwards
            enemy.Y += enemy.Speed * dt;

            // Sinusoidal/Glitch movement for scouts and glitch drones
            if (enemy.Type is EnemyType.Glitch or EnemyType.Scout)
            {
                enemy.SineOffset += dt * enemy.SineFrequency;
                enemy.X = Math.Clamp(enemy.BaseX + Math.Sin(enemy.SineOffset) * enemy.SineAmplitude, 8.0, 92.0);
            }

            // Check if enemy crossed the player's defense line (Y >= 88.0%)
            if (enemy.Y >= 88.0)
            {
                await HandleEnemyBreachedAsync(enemy);
                _enemies.RemoveAt(i);
                if (TargetEnemy == enemy)
                {
                    TargetEnemy = null;
                }
            }
        }

        // 3. Wave completion check
        if (Mode == GameMode.Normal)
        {
            if (EnemiesSpawnedInWave >= TotalEnemiesInWave && _enemies.Count == 0)
            {
                if (CurrentWave < TotalNormalWaves)
                {
                    CurrentWave++;
                    EnemiesSpawnedInWave = 0;
                    TotalEnemiesInWave = CalculateEnemiesForWave(CurrentWave);
                    SpawnTimer = 0;
                }
                else
                {
                    // All standard waves cleared -> Trigger Boss Fight!
                    await TriggerBossFightAsync();
                }
            }
        }
        else if (Mode == GameMode.Endless)
        {
            // In endless mode, trigger a boss at milestone waves or every 5 waves
            if (EnemiesSpawnedInWave >= TotalEnemiesInWave && _enemies.Count == 0)
            {
                if (CurrentWave % 4 == 0)
                {
                    await TriggerBossFightAsync();
                }
                else
                {
                    CurrentWave++;
                    EnemiesSpawnedInWave = 0;
                    TotalEnemiesInWave = CalculateEnemiesForWave(CurrentWave);
                    SpawnTimer = 0;
                }
            }
        }
    }

    private async Task UpdateBossFightStateAsync(double dt)
    {
        if (CurrentBoss == null) return;

        CurrentBoss.AdvanceTime(dt);

        // Check if boss time expired
        if (CurrentBoss.IsTimeExpired)
        {
            // Boss unleashes core blast!
            await TriggerBaseDamageAsync(35);
            if (PlayerHealth <= 0)
            {
                await TriggerGameOverAsync();
                return;
            }
            // Reset boss timer with penalty
            CurrentBoss.TimeRemainingSeconds = Config.BossTimeLimitSeconds * 0.5;
            TriggerScreenShake(0.6);
            await _sound.PlayBossAlarmAsync();
        }
    }

    private void UpdateDangerIntensity()
    {
        if (_enemies.Count == 0)
        {
            DangerWarningIntensity = 0.0;
            return;
        }

        double maxY = _enemies.Max(e => e.Y);
        // If an enemy is past 70% of the screen, ramp warning intensity from 0 to 1
        if (maxY > 70.0)
        {
            DangerWarningIntensity = Math.Clamp((maxY - 70.0) / 18.0, 0.0, 1.0);
        }
        else
        {
            DangerWarningIntensity = 0.0;
        }
    }

    private void UpdateFx(double dt)
    {
        // Lasers
        for (int i = _lasers.Count - 1; i >= 0; i--)
        {
            var laser = _lasers[i];
            laser.Life -= dt;
            if (laser.Life <= 0)
            {
                _lasers.RemoveAt(i);
            }
        }

        // Particles
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.Life -= dt;
            p.X += p.Vx * dt;
            p.Y += p.Vy * dt;

            if (p.Life <= 0 || p.X < 0 || p.X > 100 || p.Y < 0 || p.Y > 100)
            {
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnEnemy()
    {
        EnemiesSpawnedInWave++;

        // Determine enemy type based on wave & difficulty
        EnemyType type = EnemyType.Drone;
        int roll = _rnd.Next(100);

        if (roll < 30)
            type = EnemyType.Scout;
        else if (roll < 70)
            type = EnemyType.Drone;
        else if (roll < 90)
            type = EnemyType.Glitch;
        else
            type = EnemyType.Cruiser;

        var activeWords = _enemies.Select(e => e.Word).ToList();
        string word = _wordBank.GetWord(Difficulty, type, activeWords);

        // Distribute horizontally across 6 lanes (12%, 26%, 40%, 54%, 68%, 82%)
        int lane = _rnd.Next(6);
        double posX = 12.0 + (lane * 14.0) + (_rnd.NextDouble() * 4.0 - 2.0);

        double speed = GetCurrentSpeed();
        int basePoints = 100;

        if (type == EnemyType.Scout)
        {
            speed *= 1.35;
            basePoints = 150;
        }
        else if (type == EnemyType.Cruiser)
        {
            speed *= 0.75;
            basePoints = 250;
        }
        else if (type == EnemyType.Glitch)
        {
            speed *= 1.1;
            basePoints = 200;
        }

        var enemy = new Enemy
        {
            Word = word,
            X = posX,
            BaseX = posX,
            Y = 4.0 + (_rnd.NextDouble() * 3.0),
            Speed = speed,
            Type = type,
            PointsValue = basePoints,
            SineOffset = _rnd.NextDouble() * Math.PI * 2,
            LaneIndex = lane
        };

        _enemies.Add(enemy);
    }

    private async Task TriggerBossFightAsync()
    {
        State = GameState.BossFight;
        TargetEnemy = null;
        _enemies.Clear();

        string sentence = _wordBank.GetBossSentence(Difficulty, CurrentWave);
        var boss = new BossEnemy();
        boss.InitializeSentence(sentence, Config.BossTimeLimitSeconds);

        CurrentBoss = boss;
        TargetEnemy = boss;

        TriggerScreenShake(0.7);
        await _sound.PlayBossAlarmAsync();
    }

    // Input Keystroke Dispatcher
    public async Task HandleKeystrokeAsync(char inputChar)
    {
        if (State is not (GameState.Playing or GameState.BossFight))
            return;

        if (char.IsControl(inputChar))
            return;

        // If in Boss Fight, boss is always active target
        if (State == GameState.BossFight && CurrentBoss != null)
        {
            await ProcessBossKeystrokeAsync(inputChar);
            return;
        }

        // Standard Playing Mode:
        // 1. If we already have a locked target enemy:
        if (TargetEnemy != null && _enemies.Contains(TargetEnemy))
        {
            if (TargetEnemy.MatchesNextChar(inputChar))
            {
                await RegisterSuccessfulKeystrokeAsync(TargetEnemy, inputChar);
            }
            else
            {
                await RegisterFailedKeystrokeAsync();
            }
            return;
        }

        // 2. No target locked: find candidate enemies that start with this letter
        var candidates = _enemies
            .Where(e => e.TypedProgress == 0 && e.MatchesNextChar(inputChar))
            .OrderByDescending(e => e.Y) // Prioritize the one nearest to defense line!
            .ToList();

        if (candidates.Count > 0)
        {
            TargetEnemy = candidates[0];
            TargetEnemy.IsTargeted = true;
            await RegisterSuccessfulKeystrokeAsync(TargetEnemy, inputChar);
        }
        else
        {
            await RegisterFailedKeystrokeAsync();
        }
    }

    private async Task ProcessBossKeystrokeAsync(char inputChar)
    {
        if (CurrentBoss == null) return;

        bool matched = CurrentBoss.RegisterKeystroke(inputChar);
        if (matched)
        {
            Stats.RegisterKeypress(correct: true);
            SpawnLaserBeam(CurrentBoss.X, CurrentBoss.Y, "#bd00ff");
            SpawnWordParticles(CurrentBoss.X, CurrentBoss.Y, inputChar.ToString(), "#bd00ff", 3);
            await _sound.PlayKeystrokeAsync();

            if (CurrentBoss.IsDestroyed)
            {
                await HandleBossDefeatedAsync();
            }
        }
        else
        {
            await RegisterFailedKeystrokeAsync();
        }
    }

    private async Task RegisterSuccessfulKeystrokeAsync(Enemy enemy, char inputChar)
    {
        bool hit = enemy.RegisterKeystroke(inputChar);
        if (!hit) return;

        Stats.RegisterKeypress(correct: true);

        // Fire laser beam from turret (50, 92) to enemy (X, Y)
        string laserColor = enemy.Type switch
        {
            EnemyType.Scout => "#00f0ff",
            EnemyType.Drone => "#ffb703",
            EnemyType.Cruiser => "#ff007f",
            EnemyType.Glitch => "#00ff66",
            _ => "#00f0ff"
        };

        SpawnLaserBeam(enemy.X, enemy.Y, laserColor);
        SpawnWordParticles(enemy.X, enemy.Y, inputChar.ToString(), laserColor, 3);
        await _sound.PlayKeystrokeAsync();

        if (enemy.IsFullyTyped)
        {
            await HandleEnemyDestroyedAsync(enemy);
        }
    }

    private async Task RegisterFailedKeystrokeAsync()
    {
        Stats.RegisterKeypress(correct: false);
        Stats.BreakCombo();
        await _sound.PlayKeyErrorAsync();
    }

    private async Task HandleEnemyDestroyedAsync(Enemy enemy)
    {
        _enemies.Remove(enemy);
        if (TargetEnemy == enemy)
        {
            TargetEnemy = null;
        }

        Stats.RegisterWordDefeated(enemy.PointsValue);

        // Burst explosion particles
        string color = enemy.Type switch
        {
            EnemyType.Scout => "#00f0ff",
            EnemyType.Drone => "#ffb703",
            EnemyType.Cruiser => "#ff007f",
            EnemyType.Glitch => "#00ff66",
            _ => "#00f0ff"
        };

        SpawnExplosion(enemy.X, enemy.Y, color, 18);
        await _sound.PlayEnemyDestroyedAsync(enemy.Type);

        if (Stats.Combo >= 5 && Stats.Combo % 5 == 0)
        {
            await _sound.PlayComboUpAsync(Stats.Combo);
        }
    }

    private async Task HandleBossDefeatedAsync()
    {
        if (CurrentBoss == null) return;

        Stats.BossesDefeated++;
        Stats.RegisterWordDefeated(CurrentBoss.PointsValue);
        TriggerScreenShake(1.0);
        SpawnExplosion(CurrentBoss.X, CurrentBoss.Y, "#bd00ff", 40);
        await _sound.PlayBossDefeatedAsync();

        if (Mode == GameMode.Normal)
        {
            State = GameState.Victory;
            await _sound.PlayVictoryAsync();
        }
        else
        {
            // In endless mode, celebrate boss defeat and ramp up next wave cycle!
            CurrentBoss = null;
            TargetEnemy = null;
            CurrentWave++;
            EnemiesSpawnedInWave = 0;
            TotalEnemiesInWave = CalculateEnemiesForWave(CurrentWave);
            SpawnTimer = 0;
            State = GameState.Playing;
        }
    }

    private async Task HandleEnemyBreachedAsync(Enemy enemy)
    {
        Stats.EnemiesBreached++;
        Stats.BreakCombo();

        int damage = enemy.Type switch
        {
            EnemyType.Cruiser => 25,
            EnemyType.Drone => 15,
            EnemyType.Scout => 10,
            EnemyType.Glitch => 15,
            _ => 15
        };

        TriggerScreenShake(0.5);
        SpawnExplosion(enemy.X, 90.0, "#ff0055", 15);
        await TriggerBaseDamageAsync(damage);

        if (PlayerHealth <= 0)
        {
            await TriggerGameOverAsync();
        }
    }

    private async Task TriggerBaseDamageAsync(int damage)
    {
        ShieldRegenTimer = 0.0; // Reset regen timer on hit
        Stats.DamageTaken += damage;

        if (PlayerShield > 0)
        {
            if (PlayerShield >= damage)
            {
                PlayerShield -= damage;
                await _sound.PlayShieldHitAsync();
                return;
            }
            else
            {
                int remaining = damage - PlayerShield;
                PlayerShield = 0;
                PlayerHealth = Math.Max(0, PlayerHealth - remaining);
                await _sound.PlayBaseDamageAsync();
            }
        }
        else
        {
            PlayerHealth = Math.Max(0, PlayerHealth - damage);
            await _sound.PlayBaseDamageAsync();
        }
    }

    private async Task TriggerGameOverAsync()
    {
        State = GameState.GameOver;
        TriggerScreenShake(1.2);
        await _sound.PlayGameOverAsync();
        NotifyStateChanged();
    }

    public void TriggerScreenShake(double duration)
    {
        ScreenShakeTimer = Math.Max(ScreenShakeTimer, duration);
        ScreenShaken?.Invoke();
    }

    private void SpawnLaserBeam(double targetX, double targetY, string color)
    {
        _lasers.Add(new LaserBeam
        {
            StartX = 50.0,
            StartY = 92.0,
            TargetX = targetX,
            TargetY = targetY,
            Color = color
        });
    }

    private void SpawnWordParticles(double x, double y, string text, string color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            double angle = _rnd.NextDouble() * Math.PI * 2;
            double speed = 8.0 + (_rnd.NextDouble() * 14.0);
            _particles.Add(new Particle
            {
                X = x,
                Y = y,
                Vx = Math.Cos(angle) * speed,
                Vy = Math.Sin(angle) * speed,
                Color = color,
                Life = 0.45,
                MaxLife = 0.45,
                Size = 3.0,
                TextChar = text
            });
        }
    }

    private void SpawnExplosion(double x, double y, string color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            double angle = _rnd.NextDouble() * Math.PI * 2;
            double speed = 12.0 + (_rnd.NextDouble() * 26.0);
            _particles.Add(new Particle
            {
                X = x,
                Y = y,
                Vx = Math.Cos(angle) * speed,
                Vy = Math.Sin(angle) * speed,
                Color = color,
                Life = 0.6 + (_rnd.NextDouble() * 0.4),
                MaxLife = 1.0,
                Size = 2.0 + (_rnd.NextDouble() * 4.0)
            });
        }
    }

    public void UntargetCurrentEnemy()
    {
        if (TargetEnemy != null && State != GameState.BossFight)
        {
            TargetEnemy.ResetProgress();
            TargetEnemy = null;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    private async Task StopGameLoopAsync()
    {
        if (_gameCts != null)
        {
            _gameCts.Cancel();
            if (_gameLoopTask != null)
            {
                try
                {
                    await _gameLoopTask;
                }
                catch (OperationCanceledException) { }
            }
            _gameCts.Dispose();
            _gameCts = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopGameLoopAsync();
        GC.SuppressFinalize(this);
    }
}
