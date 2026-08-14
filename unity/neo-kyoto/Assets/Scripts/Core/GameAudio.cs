using System.Collections.Generic;
using UnityEngine;

namespace NeoKyoto.Core
{
    /// <summary>Short interface sounds, synthesised rather than sourced.</summary>
    public enum Sfx { Click, Back, Run, Complete, Error, Tick, Crackle }

    /// <summary>
    /// The whole mix, in one serialisable block so it can live on Bootstrap and be
    /// edited in the inspector. Bus levels are absolute; per-effect values are
    /// multipliers around 1, so an effect can be nudged without disturbing the rest.
    ///
    /// GameAudio re-reads this every frame, so dragging a slider during play is
    /// audible immediately. Play-mode edits still revert on stop, as they always do —
    /// find a value live, then set it in edit mode to keep it.
    /// </summary>
    [System.Serializable]
    public class AudioMix
    {
        [Header("Buses")]
        [Range(0f, 1f)] public float master = 1f;
        [Range(0f, 1f)] public float music = 0.35f;
        [Range(0f, 1f)] public float effects = 0.5f;
        [Range(0f, 1f)] public float rain = 0.18f;
        [Range(0f, 1f)] public float hum = 0.10f;
        public bool muted;

        [Header("Per effect (1 = default)")]
        [Range(0f, 4f)] public float click = 1f;
        [Range(0f, 4f)] public float back = 1f;
        [Range(0f, 4f)] public float run = 1f;
        [Range(0f, 4f)] public float complete = 1f;
        [Range(0f, 4f)] public float error = 1f;
        [Range(0f, 4f)] public float tick = 1f;
        [Range(0f, 4f)] public float crackle = 2.2f;

        public float GainFor(Sfx sfx)
        {
            switch (sfx)
            {
                case Sfx.Click: return click;
                case Sfx.Back: return back;
                case Sfx.Run: return run;
                case Sfx.Complete: return complete;
                case Sfx.Error: return error;
                case Sfx.Tick: return tick;
                case Sfx.Crackle: return crackle;
                default: return 1f;
            }
        }
    }

    /// <summary>
    /// Music and interface sound. One music source that crossfades, one pooled set of
    /// one-shot sources so overlapping effects do not cut each other off.
    ///
    /// The effects are generated at startup instead of loaded. A terminal game wants
    /// short synthetic tones anyway, so a few hundred bytes of maths gives immediate
    /// feedback on every button without waiting on an audio pass — and the shapes stay
    /// easy to retune. Swapping in recorded clips later only means changing Play.
    ///
    /// WebGL note: browsers refuse to start audio before the player interacts with the
    /// page, so on the web build the splash music begins at the first click or keypress
    /// rather than at load. Nothing to fix in code — it is the autoplay policy.
    /// </summary>
    public class GameAudio : MonoBehaviour
    {
        public static GameAudio Instance { get; private set; }

        /// <summary>Set by Bootstrap so the sliders live on a scene object.</summary>
        public AudioMix mix = new AudioMix();

        private bool _ambienceOn;

        private AudioSource _music;
        private AudioSource _rain, _hum;
        private AudioSource[] _oneShots;
        private int _nextShot;
        private readonly Dictionary<Sfx, AudioClip> _clips = new Dictionary<Sfx, AudioClip>();
        private const int VoiceCount = 6;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.playOnAwake = false;
            _music.spatialBlend = 0f;

            _rain = AddLoop(Rain());
            _hum = AddLoop(Hum());

            _oneShots = new AudioSource[VoiceCount];
            for (int i = 0; i < VoiceCount; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                _oneShots[i] = src;
            }

            BuildSfx();
        }

        private AudioSource AddLoop(AudioClip clip)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.clip = clip;
            src.loop = true;
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.volume = 0f;
            return src;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        // ─── Music ───

        /// <summary>Continuous levels are re-applied every frame so inspector edits are live.</summary>
        private void Update()
        {
            float m = mix.muted ? 0f : mix.master;
            _music.volume = mix.music * m;
            if (_ambienceOn)
            {
                _rain.volume = mix.rain * m;
                _hum.volume = mix.hum * m;
            }
        }

        /// <summary>Starts a track from Resources/Audio, ignoring a repeat request.</summary>
        public void PlayMusic(string resourceName)
        {
            var clip = Resources.Load<AudioClip>("Audio/" + resourceName);
            if (clip == null) return;
            if (_music.clip == clip && _music.isPlaying) return;

            _music.clip = clip;
            _music.Play();
        }

        public void StopMusic() { _music.Stop(); }

        public void SetMuted(bool value) { mix.muted = value; }

        // ─── Ambience ───

        /// <summary>Rain and the electrical hum of a city running on tired infrastructure.</summary>
        public void PlayAmbience()
        {
            _ambienceOn = true;
            if (!_rain.isPlaying) _rain.Play();
            if (!_hum.isPlaying) _hum.Play();
        }

        public void StopAmbience()
        {
            _ambienceOn = false;
            if (_rain != null) _rain.Stop();
            if (_hum != null) _hum.Stop();
        }

        // ─── Effects ───

        public void Play(Sfx sfx, float volumeScale = 1f, float pitch = 1f)
        {
            if (mix.muted) return;
            AudioClip clip;
            if (!_clips.TryGetValue(sfx, out clip) || clip == null) return;

            float gain = mix.master * mix.effects * mix.GainFor(sfx) * volumeScale;
            if (gain <= 0f) return;

            var src = _oneShots[_nextShot];
            _nextShot = (_nextShot + 1) % VoiceCount;
            src.pitch = pitch;
            src.PlayOneShot(clip, Mathf.Clamp(gain, 0f, 4f));
        }

        // ─── Synthesis ───

        private void BuildSfx()
        {
            _clips[Sfx.Click] = Tone("sfxClick", 0.055f, 880f, 1320f, 0.004f, Wave.Square, 0.30f);
            _clips[Sfx.Back] = Tone("sfxBack", 0.070f, 660f, 440f, 0.004f, Wave.Square, 0.26f);
            _clips[Sfx.Run] = Tone("sfxRun", 0.180f, 320f, 720f, 0.010f, Wave.Saw, 0.30f);
            _clips[Sfx.Error] = Tone("sfxError", 0.220f, 300f, 190f, 0.010f, Wave.Saw, 0.26f);
            _clips[Sfx.Tick] = Tone("sfxTick", 0.035f, 1400f, 1400f, 0.002f, Wave.Sine, 0.18f);
            _clips[Sfx.Complete] = Arpeggio("sfxComplete", new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.085f);
            _clips[Sfx.Crackle] = Crackle();
        }

        private const int Rate = 44100;

        /// <summary>
        /// Rain: broadband hiss, band-limited so it sits behind everything, with a slow
        /// wander so it does not read as a flat noise floor.
        /// </summary>
        private static AudioClip Rain()
        {
            const float seconds = 6f;
            int fade = Rate / 4;
            int count = (int)(seconds * Rate);
            var data = new float[count + fade];

            var rnd = new System.Random(7);
            float lo = 0f, hi = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float white = (float)(rnd.NextDouble() * 2.0 - 1.0);
                lo += (white - lo) * 0.35f;      // one-pole low pass, takes the edge off
                hi += (lo - hi) * 0.010f;        // slow follower, subtracted for high pass
                float band = lo - hi;

                // Gentle swell so the loop does not sound mechanical.
                float wander = 0.82f + 0.18f * Mathf.Sin(i / (float)Rate * 0.7f * Mathf.PI * 2f);
                data[i] = band * wander * 0.5f;
            }

            return Looped("ambRain", data, count, fade);
        }

        /// <summary>
        /// Mains hum: 50 Hz and its harmonics. The loop length is an exact multiple of
        /// every period, so it repeats without a discontinuity.
        /// </summary>
        private static AudioClip Hum()
        {
            const int count = Rate * 2;          // 100 whole cycles of 50 Hz
            var data = new float[count];
            float[] harmonics = { 50f, 100f, 150f, 250f };
            float[] gains = { 1f, 0.45f, 0.22f, 0.08f };

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Rate;
                float s = 0f;
                for (int hIdx = 0; hIdx < harmonics.Length; hIdx++)
                    s += Mathf.Sin(t * harmonics[hIdx] * Mathf.PI * 2f) * gains[hIdx];

                // One full tremolo cycle across the loop, so the ends still meet.
                float trem = 0.9f + 0.1f * Mathf.Sin(t / 2f * Mathf.PI * 2f);
                data[i] = s * trem * 0.16f;
            }

            var clip = AudioClip.Create("ambHum", count, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>A tube arcing: a few noise spits with hard decay.</summary>
        private static AudioClip Crackle()
        {
            int count = (int)(0.13f * Rate);
            var data = new float[count];
            var rnd = new System.Random(19);
            float lo = 0f;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float white = (float)(rnd.NextDouble() * 2.0 - 1.0);
                lo += (white - lo) * 0.6f;                       // keep it bright, not hissy

                // Three spits inside the window rather than one flat burst.
                float spit = Mathf.Max(0f, Mathf.Sin(t * Mathf.PI * 7f));
                float env = Mathf.Exp(-7f * t) * spit;
                data[i] = lo * env * 0.55f;
            }

            var clip = AudioClip.Create("sfxCrackle", count, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Folds an extra tail back over the head so a noise loop has no seam. Without
        /// this the wrap point is an audible click every time round.
        /// </summary>
        private static AudioClip Looped(string name, float[] data, int count, int fade)
        {
            for (int i = 0; i < fade; i++)
            {
                float t = (float)i / fade;
                data[i] = data[i] * t + data[count + i] * (1f - t);
            }

            var body = new float[count];
            System.Array.Copy(data, body, count);
            var clip = AudioClip.Create(name, count, 1, Rate, false);
            clip.SetData(body, 0);
            return clip;
        }

        private enum Wave { Sine, Square, Saw }

        /// <summary>A short tone that glides between two pitches, with a soft envelope.</summary>
        private static AudioClip Tone(string name, float seconds, float fromHz, float toHz,
                                      float attack, Wave wave, float gain)
        {
            const int rate = 44100;
            int count = Mathf.Max(1, (int)(seconds * rate));
            var data = new float[count];
            float phase = 0f;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float hz = Mathf.Lerp(fromHz, toHz, t);
                phase += hz / rate;
                if (phase > 1f) phase -= 1f;

                float s;
                switch (wave)
                {
                    case Wave.Square: s = phase < 0.5f ? 1f : -1f; break;
                    case Wave.Saw: s = phase * 2f - 1f; break;
                    default: s = Mathf.Sin(phase * Mathf.PI * 2f); break;
                }

                // Quick attack, exponential decay — clicks, not beeps that outstay.
                float env = t < attack / seconds
                    ? t / (attack / seconds)
                    : Mathf.Exp(-5f * (t - attack / seconds));
                data[i] = s * env * gain;
            }

            var clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Rising notes for a success payoff.</summary>
        private static AudioClip Arpeggio(string name, float[] notes, float noteSeconds)
        {
            const int rate = 44100;
            int per = (int)(noteSeconds * rate);
            var data = new float[per * notes.Length];

            for (int n = 0; n < notes.Length; n++)
            {
                float phase = 0f;
                for (int i = 0; i < per; i++)
                {
                    phase += notes[n] / rate;
                    if (phase > 1f) phase -= 1f;
                    float t = (float)i / per;
                    float env = Mathf.Min(1f, t / 0.05f) * Mathf.Exp(-3.2f * t);
                    float s = Mathf.Sin(phase * Mathf.PI * 2f) * 0.7f
                            + (phase < 0.5f ? 1f : -1f) * 0.15f;
                    data[n * per + i] = s * env * 0.26f;
                }
            }

            var clip = AudioClip.Create(name, data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
