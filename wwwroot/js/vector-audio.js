// VectorType - Web Audio API Synthesizer
// Pure client-side zero-dependency arcade synthesizer
(function () {
    let audioCtx = null;
    let masterGain = null;
    let isMuted = false;

    function getAudioContext() {
        if (!audioCtx) {
            const AudioContextClass = window.AudioContext || window.webkitAudioContext;
            if (AudioContextClass) {
                audioCtx = new AudioContextClass();
                masterGain = audioCtx.createGain();
                masterGain.gain.setValueAtTime(0.3, audioCtx.currentTime);
                masterGain.connect(audioCtx.destination);
            }
        }
        if (audioCtx && audioCtx.state === 'suspended') {
            audioCtx.resume();
        }
        return audioCtx;
    }

    window.vectorAudio = {
        init: function () {
            getAudioContext();
        },

        setMuted: function (muted) {
            isMuted = muted;
            if (masterGain && audioCtx) {
                masterGain.gain.setValueAtTime(muted ? 0.0 : 0.3, audioCtx.currentTime);
            }
        },

        playKeystroke: function () {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = 'triangle';
            osc.frequency.setValueAtTime(800 + Math.random() * 200, now);
            osc.frequency.exponentialRampToValueAtTime(1400, now + 0.04);

            gain.gain.setValueAtTime(0.12, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.04);

            osc.connect(gain);
            gain.connect(masterGain);

            osc.start(now);
            osc.stop(now + 0.05);
        },

        playKeyError: function () {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(160, now);
            osc.frequency.linearRampToValueAtTime(110, now + 0.12);

            gain.gain.setValueAtTime(0.25, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.12);

            osc.connect(gain);
            gain.connect(masterGain);

            osc.start(now);
            osc.stop(now + 0.13);
        },

        playLaser: function () {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(1100, now);
            osc.frequency.exponentialRampToValueAtTime(220, now + 0.1);

            gain.gain.setValueAtTime(0.18, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.1);

            osc.connect(gain);
            gain.connect(masterGain);

            osc.start(now);
            osc.stop(now + 0.11);
        },

        playExplosion: function (enemyType) {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;

            // Noise buffer for blast
            const bufferSize = ctx.sampleRate * 0.25;
            const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
            const data = buffer.getChannelData(0);
            for (let i = 0; i < bufferSize; i++) {
                data[i] = Math.random() * 2 - 1;
            }

            const noise = ctx.createBufferSource();
            noise.buffer = buffer;

            // Filter for punchy impact
            const filter = ctx.createBiquadFilter();
            filter.type = 'lowpass';
            filter.frequency.setValueAtTime(enemyType === 'cruiser' ? 400 : 900, now);
            filter.frequency.exponentialRampToValueAtTime(100, now + 0.25);

            const gain = ctx.createGain();
            gain.gain.setValueAtTime(0.35, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.25);

            noise.connect(filter);
            filter.connect(gain);
            gain.connect(masterGain);

            noise.start(now);
            noise.stop(now + 0.26);

            // Sub-bass thud
            const osc = ctx.createOscillator();
            const oscGain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(180, now);
            osc.frequency.exponentialRampToValueAtTime(45, now + 0.2);

            oscGain.gain.setValueAtTime(0.4, now);
            oscGain.gain.exponentialRampToValueAtTime(0.001, now + 0.2);

            osc.connect(oscGain);
            oscGain.connect(masterGain);

            osc.start(now);
            osc.stop(now + 0.21);
        },

        playShieldHit: function () {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = 'sine';
            osc.frequency.setValueAtTime(600, now);
            osc.frequency.exponentialRampToValueAtTime(200, now + 0.18);

            gain.gain.setValueAtTime(0.3, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.18);

            osc.connect(gain);
            gain.connect(masterGain);

            osc.start(now);
            osc.stop(now + 0.19);
        },

        playBaseDamage: function () {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(130, now);
            osc.frequency.exponentialRampToValueAtTime(35, now + 0.35);

            gain.gain.setValueAtTime(0.5, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.35);

            osc.connect(gain);
            gain.connect(masterGain);

            osc.start(now);
            osc.stop(now + 0.36);
        },

        playComboUp: function (combo) {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            const baseFreq = 440;
            const pitchStep = Math.min(combo, 24);
            const freq = baseFreq * Math.pow(1.05946, pitchStep);

            osc.type = 'square';
            osc.frequency.setValueAtTime(freq, now);
            osc.frequency.exponentialRampToValueAtTime(freq * 1.3, now + 0.1);

            gain.gain.setValueAtTime(0.15, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.15);

            osc.connect(gain);
            gain.connect(masterGain);

            osc.start(now);
            osc.stop(now + 0.16);
        },

        playBossAlarm: function () {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            for (let i = 0; i < 3; i++) {
                const startTime = now + (i * 0.28);
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();

                osc.type = 'sawtooth';
                osc.frequency.setValueAtTime(740, startTime);
                osc.frequency.linearRampToValueAtTime(520, startTime + 0.2);

                gain.gain.setValueAtTime(0.35, startTime);
                gain.gain.exponentialRampToValueAtTime(0.001, startTime + 0.25);

                osc.connect(gain);
                gain.connect(masterGain);

                osc.start(startTime);
                osc.stop(startTime + 0.26);
            }
        },

        playBossDefeated: function () {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            const notes = [330, 440, 554, 660, 880];
            notes.forEach((freq, idx) => {
                const noteTime = now + (idx * 0.12);
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();

                osc.type = 'sawtooth';
                osc.frequency.setValueAtTime(freq, noteTime);

                gain.gain.setValueAtTime(0.25, noteTime);
                gain.gain.exponentialRampToValueAtTime(0.001, noteTime + 0.35);

                osc.connect(gain);
                gain.connect(masterGain);

                osc.start(noteTime);
                osc.stop(noteTime + 0.36);
            });
        },

        playVictory: function () {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            const chords = [523.25, 659.25, 783.99, 1046.50]; // C E G C
            chords.forEach((freq, idx) => {
                const noteTime = now + (idx * 0.15);
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();

                osc.type = 'triangle';
                osc.frequency.setValueAtTime(freq, noteTime);

                gain.gain.setValueAtTime(0.3, noteTime);
                gain.gain.exponentialRampToValueAtTime(0.001, noteTime + 0.6);

                osc.connect(gain);
                gain.connect(masterGain);

                osc.start(noteTime);
                osc.stop(noteTime + 0.65);
            });
        },

        playGameOver: function () {
            if (isMuted) return;
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            const notes = [392.00, 370.00, 349.23, 311.13, 261.63]; // Descending minor
            notes.forEach((freq, idx) => {
                const noteTime = now + (idx * 0.2);
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();

                osc.type = 'sawtooth';
                osc.frequency.setValueAtTime(freq, noteTime);
                osc.frequency.exponentialRampToValueAtTime(freq * 0.9, noteTime + 0.3);

                gain.gain.setValueAtTime(0.28, noteTime);
                gain.gain.exponentialRampToValueAtTime(0.001, noteTime + 0.3);

                osc.connect(gain);
                gain.connect(masterGain);

                osc.start(noteTime);
                osc.stop(noteTime + 0.32);
            });
        }
    };
})();
