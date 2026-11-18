# 🎮 VectorType // Cyberpunk Arcade Typing Game

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![Blazor WebAssembly](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind-CSS-38B2AC?logo=tailwind-css&logoColor=white)
![Web Audio API](https://img.shields.io/badge/Audio-Web%20Audio%20API-FF69B4)

> A polished, fast-paced 2D neon-vector arcade typing game built with client-side **Blazor WebAssembly (.NET 10)**, featuring a **60 FPS tick-based C# game loop**, dynamic difficulty scaling, boss sentence encounters, shield matrices, and zero-dependency Web Audio synthesizer sound effects.

---

## ⚡ Game Concept & Mechanics

In **VectorType**, hostile alphanumeric vectors descend through the cyber defense grid toward your core perimeter. You control the terminal defense turret at the bottom of the screen.

1. **Precision Target Lock**:
   - As you type the initial character of any incoming hostile, your targeting computer immediately locks onto the nearest threat matching that letter.
   - Letters typed illuminate in **neon green** with glowing target reticles.
   - Completing the word immediately triggers a high-velocity plasma turret beam, annihilating the enemy with particle explosions.
2. **Boss Battles**:
   - At the culmination of Normal Mode waves (or milestone score cycles in Endless Mode), the **Nexus Overlord** mothership descends.
   - Bosses require typing complete system override sentences before the core countdown reaches zero.
3. **Shield & Base Hull Integrity**:
   - If an enemy crosses the defense perimeter ($Y \ge 88\%$), it breaches the grid, dealing damage.
   - The player possesses an energy **Shield** that absorbs initial impacts and regenerates after 4 seconds of taking no damage. Once depleted, the hull absorbs direct damage.
4. **Combo & Live Telemetry**:
   - Consecutive successful word destructions increase your **Combo Multiplier** up to `4.0x`.
   - Real-time telemetry tracks **Live WPM**, **Peak WPM**, **Accuracy %**, and **Hull Integrity**.

---

## 🗂️ Architecture & Directory Breakdown

```plaintext
VectorType/
├── Components/
│   ├── BossView.razor           # Mothership dreadnought, health meter, and sentence typing banner
│   ├── DifficultySelector.razor # Mode selection (Normal/Endless) & difficulty matrix modal
│   ├── EnemyView.razor          # Vector SVG ships, target brackets, threat badges, and letter highlighting
│   ├── GameOverModal.razor      # System failure screen with comprehensive match metrics
│   ├── Hud.razor                # Top status bar: Score, Multipliers, Live WPM, Shields & Hull
│   ├── ParticleCanvas.razor     # High-performance SVG layer for laser beams and explosion debris
│   └── VictoryModal.razor       # Victory celebration banner with rank computation (S+, S, A, B, C)
├── Layout/
│   ├── MainLayout.razor         # Fullscreen cybernetic game shell
│   └── NavMenu.razor            # Navigation menu
├── Models/
│   ├── BossEnemy.cs             # Subclass of Enemy holding complete sentence and word queues
│   ├── Difficulty.cs            # Easy, Medium, Hard enum
│   ├── DifficultyConfig.cs      # Speed scaling, word length constraints, spawn rates & boss timers
│   ├── Enemy.cs                 # Enemy base entity with coordinates, typing state, and health
│   ├── EnemyType.cs             # Scout, Drone, Cruiser, Glitch, and Boss classifications
│   ├── GameMode.cs              # Normal and Endless game mode modes
│   ├── GameStats.cs             # Live WPM, Peak WPM, Accuracy, Streak multiplier, and Score
│   └── Particle.cs              # Explosion sparks, letters, and turret laser beam structures
├── Pages/
│   ├── GameView.razor           # Primary interactive game canvas, input dispatcher, and render loop
│   └── NotFound.razor           # Cyberpunk-styled 404 fallback page
├── Services/
│   ├── GameEngine.cs            # ~60 FPS PeriodicTimer game loop, collision detection, and wave logic
│   ├── SoundService.cs          # JSInterop wrapper for retro audio effects
│   └── WordBankService.cs       # Thematic cyber/tech dictionary and boss sentence catalog
├── wwwroot/
│   ├── css/app.css              # CRT scanlines, 3D vector floor, starfield, and screen shake
│   ├── js/vector-audio.js       # Pure client-side Web Audio API synthesizer
│   └── index.html               # Tailwind setup, font definitions, and audio bootstrap
├── Program.cs                   # Blazor WebAssembly service container registrations
└── VectorType.csproj            # .NET 10 project definition
```

---

## ⚙️ Technical Highlights

### 1. High-Performance C# Game Loop (`GameEngine.cs`)
- Driven by `System.Threading.PeriodicTimer` operating at approximately 60 FPS (~16ms delta time ticks).
- Decoupled state updates from UI thread rendering:
  - Movement physics with sinusoidal wave deviations for agile `Scout` and `Glitch` entities.
  - Boundary threshold checking against the defense line ($Y = 88\%$).
  - Shield regeneration cooldown timers.
  - Particle decay and turret laser dissipation.

### 2. Multi-Tier Enemy Model
- **Scout**: Swift dart vessel, high velocity, short 3–4 letter keywords.
- **Drone**: Balanced hexagonal drone, medium velocity, standard vocabulary.
- **Cruiser**: Armored warship, heavier hull, long complex words.
- **Glitch**: Erratically oscillating matrix drone with technical vocabulary.
- **Boss (`BossEnemy`)**: Flagship dreadnought containing structured word queues and sentence progression tracking.

### 3. Difficulty Tuning Matrix
| Difficulty | Velocity | Word Length | Boss Timer | Base Hull / Shield | Score Multiplier |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Easy** | 3.2 – 7.0 %/s | 3 – 5 chars | 55 sec | 120 / 60 | 1.0x |
| **Medium** | 4.8 – 11.0 %/s | 4 – 8 chars | 40 sec | 100 / 50 | 1.5x |
| **Hard** | 7.0 – 16.0 %/s | 6 – 12 chars | 28 sec | 80 / 30 | 2.2x |

### 4. Client-Side Web Audio Synthesizer (`vector-audio.js`)
Zero external MP3/WAV dependencies! Uses native browser `AudioContext` to generate:
- Custom oscillator laser sweeps ($1100\text{Hz} \to 220\text{Hz}$).
- Dual-layer filtered white noise explosion bursts and sub-bass impact thuds.
- Metallic shield deflection chimes and bass alert klaxons.
- Multi-tone 80s synth victory arpeggios ($C \to E \to G \to C$).

---

## 🚀 Running the Project

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Quick Start
1. Clone the repository:
   ```bash
   git clone https://github.com/flurgerburger/VectorType.git
   cd VectorType
   ```

2. Restore and run using the .NET CLI:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to:
   ```
   http://localhost:5018
   ```

### Hotkeys & Controls
- **Keystrokes `A`–`Z`**: Automatically locks onto and neutralizes targeted enemies.
- **`Space`**: Advances words in boss sentence sequences.
- **`Backspace`**: Releases target lock to switch to another incoming hostile.
- **`Escape`**: Pauses and resumes tactical hold.

---

## 📜 License
MIT License. Built with passion for retro arcade gaming and modern .NET Blazor WebAssembly.
