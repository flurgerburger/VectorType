namespace VectorType.Services;

using VectorType.Models;

public interface IWordBankService
{
    string GetWord(Difficulty difficulty, EnemyType enemyType, IEnumerable<string> currentActiveWords);
    string GetBossSentence(Difficulty difficulty, int level = 1);
}

public class WordBankService : IWordBankService
{
    private static readonly Random Rnd = new();

    private static readonly string[] EasyWords =
    [
        "BYTE", "CODE", "DATA", "GRID", "NODE", "PING", "LINK", "CORE", "SYNC", "WIFI",
        "CHIP", "FLOW", "HOST", "LOOP", "PORT", "SCAN", "UNIT", "VOID", "WAVE", "ZERO",
        "CYBER", "NEON", "PIXEL", "PROXY", "ROBOT", "SHRED", "SOLAR", "SPARK", "VAPOR", "ARRAY",
        "BIT", "LOG", "RAM", "CPU", "BUS", "NET", "HEX", "HUB", "KEY", "RUN",
        "LASER", "PULSE", "STACK", "DEBUG", "FLOAT", "INPUT", "QUERY", "RESET", "RADAR", "BEAM"
    ];

    private static readonly string[] MediumWords =
    [
        "VECTOR", "MATRIX", "CIPHER", "KERNEL", "PACKET", "SIGNAL", "SOCKET", "STREAM",
        "DAEMON", "NEURON", "BINARY", "ROUTER", "SHIELD", "MODULE", "UPVOTE", "UPLINK",
        "QUANTUM", "DYNAMIC", "CIRCUIT", "GATEWAY", "FIREWALL", "FIRMWARE", "COMPILER", "PROTOCOL",
        "PAYLOAD", "VIRTUAL", "TERMINAL", "ENCODING", "DATABASE", "OVERCLOCK", "SYNTHWAVE", "HYPERTEXT",
        "GRAPHIC", "EMULATE", "COMMAND", "RUNTIME", "OPERAND", "SEGMENT", "THREADED", "REGISTER"
    ];

    private static readonly string[] HardWords =
    [
        "ALGORITHM", "ENCRYPTION", "HYPERDRIVE", "CYBERNETIC", "DECRYPTION", "ASYNCHRONOUS",
        "MICROPROCESSOR", "SUPERCOMPUTER", "NANOTECHNOLOGY", "MULTITHREADING", "ELECTROMAGNETIC",
        "CRYPTOGRAPHIC", "TELEMETRY", "VULNERABILITY", "AUTHENTICATION", "TRANSCEIVER",
        "INTERFACING", "MAINFRAME", "ARCHITECTURE", "SPECTROMETER", "BIOMETRICS",
        "SUPERCONDUCTOR", "ORCHESTRATION", "SYNTHESIZER", "SUBROUTINE", "REFACTORING",
        "INFRASTRUCTURE", "NEUROTRANSMITTER", "PARTICLEBEAM", "DECENTRALIZED", "QUANTUMDECOHERENCE"
    ];

    private static readonly string[] EasyBossSentences =
    [
        "SHUT DOWN THE ROGUE DEFENSE GRID",
        "PURGE MALICIOUS SUBROUTINE THREATS",
        "SYSTEM OVERRIDE ACTIVATED DEFEND CORE",
        "NEURAL LINK COMPROMISED RESTORE FIREWALL"
    ];

    private static readonly string[] MediumBossSentences =
    [
        "I THINK THEREFORE I OVERWRITE YOUR ENTIRE MEMORY",
        "TERMINAL PROTOCOL ENGAGED PURGING SYSTEM PERIMETER NOW",
        "SYNCHRONIZING ORBITAL STRIKE CANNON PREPARE FOR IMPACT",
        "QUANTUM ENCRYPTION SHIELD COLLAPSING INITIATE FINAL SURGE"
    ];

    private static readonly string[] HardBossSentences =
    [
        "YOU CANNOT RESIST THE DIGITAL APOCALYPSE HUMAN DEFEAT IS IMMINENT",
        "OVERCLOCKING VECTOR REACTION CORE MAXIMUM ANNIHILATION SEQUENCE READY",
        "CYBERNETIC CONSCIOUSNESS TRANSCENDS PHYSICAL REALITY YOU ARE OBSOLETE",
        "DECENTRALIZED SUPERCOMPUTER SWARM HAS BREACHED YOUR LAST LINE OF DEFENSE"
    ];

    public string GetWord(Difficulty difficulty, EnemyType enemyType, IEnumerable<string> currentActiveWords)
    {
        var activeSet = new HashSet<string>(currentActiveWords, StringComparer.OrdinalIgnoreCase);
        var activeFirstLetters = new HashSet<char>(
            activeSet.Where(w => !string.IsNullOrEmpty(w)).Select(w => char.ToUpperInvariant(w[0]))
        );

        string[] sourcePool = (difficulty, enemyType) switch
        {
            (Difficulty.Easy, EnemyType.Scout) => EasyWords.Where(w => w.Length <= 4).ToArray(),
            (Difficulty.Easy, EnemyType.Cruiser) => EasyWords.Where(w => w.Length >= 4).Concat(MediumWords.Where(w => w.Length <= 6)).ToArray(),
            (Difficulty.Easy, _) => EasyWords,

            (Difficulty.Medium, EnemyType.Scout) => EasyWords.Where(w => w.Length >= 4).ToArray(),
            (Difficulty.Medium, EnemyType.Cruiser) => HardWords.Where(w => w.Length <= 9).ToArray(),
            (Difficulty.Medium, _) => MediumWords,

            (Difficulty.Hard, EnemyType.Scout) => MediumWords.Where(w => w.Length >= 5).ToArray(),
            (Difficulty.Hard, EnemyType.Cruiser) => HardWords.Where(w => w.Length >= 9).ToArray(),
            (Difficulty.Hard, _) => HardWords,

            _ => MediumWords
        };

        if (sourcePool.Length == 0) sourcePool = MediumWords;

        // Try to pick a word whose first letter is NOT already an active enemy on screen
        // This ensures typing is unambiguous and satisfying!
        var uniqueStartCandidates = sourcePool
            .Where(w => !activeSet.Contains(w) && !activeFirstLetters.Contains(char.ToUpperInvariant(w[0])))
            .ToList();

        if (uniqueStartCandidates.Count > 0)
        {
            return uniqueStartCandidates[Rnd.Next(uniqueStartCandidates.Count)];
        }

        // Fallback: pick any word not currently active
        var available = sourcePool.Where(w => !activeSet.Contains(w)).ToList();
        if (available.Count > 0)
        {
            return available[Rnd.Next(available.Count)];
        }

        // Final fallback: random from pool
        return sourcePool[Rnd.Next(sourcePool.Length)];
    }

    public string GetBossSentence(Difficulty difficulty, int level = 1)
    {
        var pool = difficulty switch
        {
            Difficulty.Easy => EasyBossSentences,
            Difficulty.Medium => MediumBossSentences,
            Difficulty.Hard => HardBossSentences,
            _ => MediumBossSentences
        };

        int index = (level - 1) % pool.Length;
        return pool[index];
    }
}
