using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    /// <summary>Cycles a uGUI <see cref="Image"/> through a set of sprites (unscaled time), for
    /// simple looping animations on menus — e.g. the idle cowboy on the home screen.</summary>
    public class UiSpriteAnim : MonoBehaviour
    {
        Image _img;
        Sprite[] _frames;
        float _frameTime;
        float _t;
        int _i;

        public void Play(Image img, Sprite[] frames, float fps)
        {
            _img = img;
            _frames = frames;
            _frameTime = fps > 0f ? 1f / fps : 0.3f;
            _i = 0; _t = 0f;
            if (frames != null && frames.Length > 0) _img.sprite = frames[0];
        }

        void Update()
        {
            if (_frames == null || _frames.Length < 2 || _img == null) return;
            _t += Time.unscaledDeltaTime;
            if (_t < _frameTime) return;
            _t = 0f;
            _i = (_i + 1) % _frames.Length;
            _img.sprite = _frames[_i];
        }
    }
}
