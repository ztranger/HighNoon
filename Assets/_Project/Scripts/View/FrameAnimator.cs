using System;
using UnityEngine;

namespace HighNoon
{
    /// <summary>Plays a sequence of sprite frames on a SpriteRenderer at a fixed fps.</summary>
    public class FrameAnimator : MonoBehaviour
    {
        SpriteRenderer _sr;
        Sprite[] _frames;
        float _frameDur;
        bool _loop;
        int _i;
        float _t;
        bool _playing;
        Action _onDone;

        public void Init(SpriteRenderer sr) => _sr = sr;

        public void Play(Sprite[] frames, float fps, bool loop, Action onDone = null)
        {
            if (frames == null || frames.Length == 0) return;
            _frames = frames;
            _frameDur = 1f / Mathf.Max(1f, fps);
            _loop = loop;
            _onDone = onDone;
            _i = 0;
            _t = 0f;
            _playing = true;
            _sr.sprite = _frames[0];
        }

        public void ShowStatic(Sprite s)
        {
            _playing = false;
            _frames = null;
            if (s != null) _sr.sprite = s;
        }

        void Update()
        {
            if (!_playing || _frames == null) return;
            _t += Time.deltaTime;
            while (_t >= _frameDur)
            {
                _t -= _frameDur;
                _i++;
                if (_i >= _frames.Length)
                {
                    if (_loop)
                    {
                        _i = 0;
                    }
                    else
                    {
                        _i = _frames.Length - 1;
                        _sr.sprite = _frames[_i];
                        _playing = false;
                        var cb = _onDone;
                        _onDone = null;
                        cb?.Invoke();
                        return;
                    }
                }
                _sr.sprite = _frames[_i];
            }
        }
    }
}
