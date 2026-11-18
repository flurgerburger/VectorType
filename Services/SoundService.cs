namespace VectorType.Services;

using Microsoft.JSInterop;
using VectorType.Models;

public interface ISoundService
{
    bool IsMuted { get; set; }
    ValueTask InitializeAsync();
    ValueTask PlayKeystrokeAsync();
    ValueTask PlayKeyErrorAsync();
    ValueTask PlayLaserFireAsync();
    ValueTask PlayEnemyDestroyedAsync(EnemyType type);
    ValueTask PlayShieldHitAsync();
    ValueTask PlayBaseDamageAsync();
    ValueTask PlayComboUpAsync(int combo);
    ValueTask PlayBossAlarmAsync();
    ValueTask PlayBossDefeatedAsync();
    ValueTask PlayVictoryAsync();
    ValueTask PlayGameOverAsync();
    ValueTask ToggleMuteAsync();
}

public class SoundService(IJSRuntime jsRuntime) : ISoundService
{
    private readonly IJSRuntime _js = jsRuntime;
    private bool _initialized = false;

    public bool IsMuted { get; set; } = false;

    public async ValueTask InitializeAsync()
    {
        if (_initialized) return;
        try
        {
            await _js.InvokeVoidAsync("vectorAudio.init");
            _initialized = true;
        }
        catch
        {
            // Sound gracefully ignored if audio context is blocked prior to user interaction
        }
    }

    private async ValueTask SafeInvokeAsync(string identifier, params object?[]? args)
    {
        if (IsMuted) return;
        try
        {
            await _js.InvokeVoidAsync(identifier, args);
        }
        catch
        {
            // Silently swallow audio exceptions (e.g. unmuted tab restrictions)
        }
    }

    public ValueTask PlayKeystrokeAsync() => SafeInvokeAsync("vectorAudio.playKeystroke");

    public ValueTask PlayKeyErrorAsync() => SafeInvokeAsync("vectorAudio.playKeyError");

    public ValueTask PlayLaserFireAsync() => SafeInvokeAsync("vectorAudio.playLaser");

    public ValueTask PlayEnemyDestroyedAsync(EnemyType type) => SafeInvokeAsync("vectorAudio.playExplosion", type.ToString().ToLowerInvariant());

    public ValueTask PlayShieldHitAsync() => SafeInvokeAsync("vectorAudio.playShieldHit");

    public ValueTask PlayBaseDamageAsync() => SafeInvokeAsync("vectorAudio.playBaseDamage");

    public ValueTask PlayComboUpAsync(int combo) => SafeInvokeAsync("vectorAudio.playComboUp", combo);

    public ValueTask PlayBossAlarmAsync() => SafeInvokeAsync("vectorAudio.playBossAlarm");

    public ValueTask PlayBossDefeatedAsync() => SafeInvokeAsync("vectorAudio.playBossDefeated");

    public ValueTask PlayVictoryAsync() => SafeInvokeAsync("vectorAudio.playVictory");

    public ValueTask PlayGameOverAsync() => SafeInvokeAsync("vectorAudio.playGameOver");

    public async ValueTask ToggleMuteAsync()
    {
        IsMuted = !IsMuted;
        await SafeInvokeAsync("vectorAudio.setMuted", IsMuted);
    }
}
