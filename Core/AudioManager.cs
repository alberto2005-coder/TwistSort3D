using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace TwistSort3D.Core
{
    /// <summary>
    /// Audio 100% procedural: todos los sonidos y la música se sintetizan en
    /// memoria al arrancar. No se necesita ningún archivo de audio externo.
    /// </summary>
    public static class AudioManager
    {
        private const int SampleRate = 44100;

        private static readonly Dictionary<string, SoundEffect> Sfx = new();
        private static SoundEffect _music;
        private static SoundEffectInstance _musicInstance;
        private static float _musicVolume = 0.65f;
        private static float _sfxVolume = 0.85f;

        public static void Initialize()
        {
            if (Sfx.Count > 0) return;

            Sfx["click"] = Build(Click());
            Sfx["screw"] = Build(Screw());
            Sfx["match"] = Build(Match());
            Sfx["drop"] = Build(Drop());
            Sfx["win"] = Build(Win());
            Sfx["lose"] = Build(Lose());
            Sfx["coin"] = Build(Coin());
            Sfx["error"] = Build(Error());
            Sfx["ice"] = Build(Ice());
            Sfx["bomb"] = Build(Bomb());
            Sfx["expand"] = Build(Expand());
            Sfx["hint"] = Build(Hint());

            _music = Build(MusicLoop());
            _musicInstance = _music.CreateInstance();
            _musicInstance.IsLooped = true;
            _musicInstance.Volume = _musicVolume;
            _musicInstance.Play();
        }

        public static void ApplySettings(SettingsData s)
        {
            _musicVolume = Math.Clamp(s.Music, 0f, 1f);
            _sfxVolume = Math.Clamp(s.Sfx, 0f, 1f);
            if (_musicInstance != null) _musicInstance.Volume = _musicVolume;
        }

        public static void Play(string id, float pitch = 0f, float volumeScale = 1f)
        {
            if (!Sfx.TryGetValue(id, out var s) || _sfxVolume <= 0f) return;
            s.Play(Math.Clamp(_sfxVolume * volumeScale, 0f, 1f), pitch, 0f);
        }

        public static void PlayMusic()
        {
            if (_musicInstance == null) return;
            _musicInstance.Volume = _musicVolume;
            _musicInstance.Play();
        }

        // ============================================================ síntesis
        private static SoundEffect Build(float[] samples)
        {
            var pcm = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short v = (short)(Math.Clamp(samples[i], -1f, 1f) * 32000f);
                pcm[i * 2] = (byte)(v & 0xFF);
                pcm[i * 2 + 1] = (byte)((v >> 8) & 0xFF);
            }
            return new SoundEffect(pcm, SampleRate, AudioChannels.Mono);
        }

        private static float[] Buffer(float seconds) => new float[(int)(SampleRate * seconds)];

        private static void AddTone(float[] buf, float start, float dur, float freq, float vol, Waveform wave, float attack = 0.005f, float release = 0.04f)
        {
            int i0 = (int)(start * SampleRate);
            int n = (int)(dur * SampleRate);
            for (int i = 0; i < n && i0 + i < buf.Length; i++)
            {
                float t = i / (float)SampleRate;
                float env = Envelope(i / (float)n, attack / dur, release / dur);
                float ph = freq * t;
                float s = wave switch
                {
                    Waveform.Sine => MathF.Sin(2 * MathF.PI * ph),
                    Waveform.Square => MathF.Sign(MathF.Sin(2 * MathF.PI * ph)) * 0.55f,
                    Waveform.Triangle => 2f * MathF.Abs(2f * (ph - MathF.Floor(ph + 0.5f))) - 1f,
                    _ => 0f
                };
                buf[i0 + i] += s * env * vol;
            }
        }

        private static void AddSweep(float[] buf, float start, float dur, float f0, float f1, float vol)
        {
            int i0 = (int)(start * SampleRate);
            int n = (int)(dur * SampleRate);
            float phase = 0;
            for (int i = 0; i < n && i0 + i < buf.Length; i++)
            {
                float k = i / (float)n;
                float f = f0 + (f1 - f0) * k;
                phase += 2 * MathF.PI * f / SampleRate;
                float env = Envelope(k, 0.02f, 0.25f);
                buf[i0 + i] += MathF.Sin(phase) * env * vol;
            }
        }

        private static void AddNoise(float[] buf, float start, float dur, float vol, Random rng)
        {
            int i0 = (int)(start * SampleRate);
            int n = (int)(dur * SampleRate);
            float last = 0;
            for (int i = 0; i < n && i0 + i < buf.Length; i++)
            {
                float k = i / (float)n;
                float white = (float)(rng.NextDouble() * 2 - 1);
                last = last * 0.7f + white * 0.3f;
                buf[i0 + i] += last * Envelope(k, 0.01f, 0.5f) * vol;
            }
        }

        private static float Envelope(float k, float attack, float release)
        {
            if (attack > 0 && k < attack) return k / attack;
            if (release > 0 && k > 1f - release) return (1f - k) / release;
            return 1f;
        }

        private static float[] Click()
        {
            var b = Buffer(0.08f);
            AddTone(b, 0, 0.05f, 950, 0.35f, Waveform.Sine);
            AddNoise(b, 0, 0.02f, 0.15f, new Random(1));
            return b;
        }

        private static float[] Screw()
        {
            var b = Buffer(0.35f);
            AddSweep(b, 0, 0.22f, 520, 210, 0.4f);
            AddNoise(b, 0.02f, 0.14f, 0.12f, new Random(2));
            AddTone(b, 0.22f, 0.1f, 880, 0.18f, Waveform.Sine);
            return b;
        }

        private static float[] Match()
        {
            var b = Buffer(0.5f);
            AddTone(b, 0.00f, 0.12f, 659.25f, 0.3f, Waveform.Triangle);
            AddTone(b, 0.09f, 0.12f, 830.61f, 0.3f, Waveform.Triangle);
            AddTone(b, 0.18f, 0.2f, 987.77f, 0.32f, Waveform.Triangle);
            return b;
        }

        private static float[] Drop()
        {
            var b = Buffer(0.3f);
            AddSweep(b, 0, 0.22f, 180, 70, 0.35f);
            AddNoise(b, 0, 0.08f, 0.18f, new Random(3));
            return b;
        }

        private static float[] Win()
        {
            var b = Buffer(1.2f);
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
            for (int i = 0; i < notes.Length; i++)
                AddTone(b, i * 0.13f, 0.3f, notes[i], 0.28f, Waveform.Triangle);
            return b;
        }

        private static float[] Lose()
        {
            var b = Buffer(0.9f);
            float[] notes = { 392f, 329.63f, 261.63f };
            for (int i = 0; i < notes.Length; i++)
                AddTone(b, i * 0.16f, 0.38f, notes[i], 0.26f, Waveform.Sine);
            return b;
        }

        private static float[] Coin()
        {
            var b = Buffer(0.25f);
            AddTone(b, 0, 0.08f, 1200, 0.25f, Waveform.Sine);
            AddTone(b, 0.07f, 0.12f, 1600, 0.22f, Waveform.Sine);
            return b;
        }

        private static float[] Error()
        {
            var b = Buffer(0.18f);
            AddTone(b, 0, 0.14f, 190, 0.3f, Waveform.Square);
            return b;
        }

        private static float[] Ice()
        {
            var b = Buffer(0.3f);
            AddNoise(b, 0, 0.22f, 0.25f, new Random(4));
            AddTone(b, 0.02f, 0.18f, 2200, 0.12f, Waveform.Sine);
            return b;
        }

        private static float[] Bomb()
        {
            var b = Buffer(0.4f);
            AddTone(b, 0, 0.08f, 80, 0.4f, Waveform.Square);
            AddTone(b, 0.15f, 0.08f, 80, 0.4f, Waveform.Square);
            AddTone(b, 0.3f, 0.08f, 80, 0.4f, Waveform.Square);
            return b;
        }

        private static float[] Expand()
        {
            var b = Buffer(0.35f);
            AddSweep(b, 0, 0.3f, 300, 900, 0.3f);
            return b;
        }

        private static float[] Hint()
        {
            var b = Buffer(0.3f);
            AddTone(b, 0, 0.1f, 1000, 0.2f, Waveform.Sine);
            AddTone(b, 0.1f, 0.15f, 1500, 0.18f, Waveform.Sine);
            return b;
        }

        private static float[] MusicLoop()
        {
            const float chordDur = 2.0f;
            var b = Buffer(chordDur * 4);
            float[][] chords =
            {
                new[] { 130.81f, 164.81f, 196.00f }, // C
                new[] { 110.00f, 130.81f, 164.81f }, // Am
                new[] { 87.31f, 130.81f, 174.61f },  // F
                new[] { 98.00f, 146.83f, 196.00f }   // G
            };
            for (int c = 0; c < 4; c++)
            {
                float start = c * chordDur;
                foreach (var f in chords[c])
                {
                    AddTone(b, start, chordDur * 0.95f, f, 0.045f, Waveform.Sine, 0.35f, 0.4f);
                    AddTone(b, start, chordDur * 0.95f, f * 2f, 0.02f, Waveform.Sine, 0.4f, 0.4f);
                }
            }
            return b;
        }

        private enum Waveform { Sine, Square, Triangle }
    }
}
