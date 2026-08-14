using NeoKyoto.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NeoKyoto.UI
{
    /// <summary>
    /// The splash mark: it wipes on like a signal locking onto a frequency, then
    /// carries a faint instability that fades out as the player stabilises the city.
    ///
    /// The wipe runs left to right, which puts the "://" — the rightmost glyph and
    /// the part of the mark that means "connected" — last, for free.
    ///
    /// Flicker is deliberately a slow train of single dips rather than a strobe.
    /// Repetitive flashing between roughly 3 and 30 Hz is a photosensitivity risk,
    /// so dips are brief, isolated, never fully dark, and average well under 1 Hz
    /// even at worst. <see cref="flicker"/> turns it off outright.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SplashLogo : MonoBehaviour
    {
        [Tooltip("Seconds for the mark to wipe on. Any input skips to the end.")]
        public float revealSeconds = 1.6f;

        [Tooltip("Signal instability early game. Turn off for photosensitivity.")]
        public bool flicker = true;

        // Starting values. Gap is divided by instability, so a player at full rank
        // effectively never sees a dip.
        private const float MinGap = 1.2f, MaxGap = 3.5f;
        private const float MinDip = 0.04f, MaxDip = 0.09f;
        private const float DeepestAlpha = 0.45f, ShallowestAlpha = 0.78f;

        private Image _image;
        private float _elapsed;
        private bool _revealed;

        private float _instability = 1f;
        private float _nextDipAt, _dipEndsAt, _dipAlpha = 1f;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.type = Image.Type.Filled;
            _image.fillMethod = Image.FillMethod.Horizontal;
            _image.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        private void OnEnable()
        {
            _elapsed = 0f;
            _revealed = false;
            _image.fillAmount = 0f;
            SetAlpha(1f);

            // The ONCALL network runs on the same infrastructure the player is
            // repairing, so the mark steadies as the city does.
            _instability = 1f;
            var gm = GameManager.Instance;
            if (gm != null && gm.State != null)
            {
                int max = ContractRegistry.MaxTotalStars;
                if (max > 0) _instability = 1f - Mathf.Clamp01((float)gm.State.TotalStars / max);
            }
            _nextDipAt = Time.unscaledTime + Random.Range(MinGap, MaxGap);
        }

        private void Update()
        {
            if (!_revealed) { Reveal(); return; }
            if (flicker) Flicker();
        }

        private void Reveal()
        {
            _elapsed += Time.unscaledDeltaTime;

            // Never trap someone who has already decided to play.
            if (Skipped() || _elapsed >= revealSeconds)
            {
                _image.fillAmount = 1f;
                _revealed = true;
                return;
            }

            float t = _elapsed / revealSeconds;
            _image.fillAmount = 1f - (1f - t) * (1f - t);   // ease out
        }

        private static bool Skipped()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.anyKey.wasPressedThisFrame) return true;
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        private void Flicker()
        {
            float now = Time.unscaledTime;

            if (now < _dipEndsAt) { SetAlpha(_dipAlpha); return; }

            SetAlpha(1f);
            if (_instability <= 0.02f || now < _nextDipAt) return;

            _dipEndsAt = now + Random.Range(MinDip, MaxDip);
            _dipAlpha = Mathf.Lerp(1f, Random.Range(DeepestAlpha, ShallowestAlpha), _instability);
            _nextDipAt = _dipEndsAt + Random.Range(MinGap, MaxGap) / Mathf.Max(0.05f, _instability);
        }

        private void SetAlpha(float a)
        {
            var c = _image.color;
            if (!Mathf.Approximately(c.a, a)) _image.color = new Color(c.r, c.g, c.b, a);
        }
    }
}
