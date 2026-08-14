using System.Collections.Generic;
using UnityEngine;

namespace NeoKyoto.Core
{
    /// <summary>Short interface sounds, synthesised rather than sourced.</summary>
    public enum Sfx { Click, Back, Run, Complete, Error, Tick }

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

        [Range(0f, 1f)] public float musicVolume = 0.35f;
        [Range(0f, 1f)] public float sfxVolume = 0.5f;
        public bool muted;

        private AudioSource _music;
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

        private void OnDestroy() { if (Instance == this) Instance = null; }

        // ─── Music ───

        /// <summary>Starts a track from Resources/Audio, ignoring a repeat request.</summary>
        public void PlayMusic(string resourceName, float volumeScale = 1f)
        {
            var clip = Resources.Load<AudioClip>("Audio/" + resourceName);
            if (clip == null) return;
            if (_music.clip == clip && _music.isPlaying) return;

            _music.clip = clip;
            _music.volume = muted ? 0f : musicVolume * volumeScale;
            _music.Play();
        }

        public void StopMusic() { _music.Stop(); }

        public void SetMuted(bool value)
        {
            muted = value;
            _music.volume = muted ? 0f : musicVolume;
        }

        // ─── Effects ───

        public void Play(Sfx sfx, float volumeScale = 1f, float pitch = 1f)
        {
            if (muted) return;
            AudioClip clip;
            if (!_clips.TryGetValue(sfx, out clip) || clip == null) return;

            var src = _oneShots[_nextShot];
            _nextShot = (_nextShot + 1) % VoiceCount;
            src.pitch = pitch;
            src.PlayOneShot(clip, Mathf.Clamp01(sfxVolume * volumeScale));
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
