using System.Collections;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Visual representation of a duelist using frame-by-frame pixel animation.
    /// The top duelist is rotated 180° so each player, sitting at opposite ends of
    /// one phone, sees their own cowboy upright.
    /// </summary>
    public class DuelistView : MonoBehaviour
    {
        SpriteRenderer _sr;
        FrameAnimator _anim;
        CowboyFrames _frames;
        Vector3 _homePos;
        Vector3 _offscreenPos;
        bool _faceDown;

        public void Setup(SpriteRenderer sr, Color teamColor, bool faceDown)
        {
            _sr = sr;
            _faceDown = faceDown;
            _frames = CowboyArt.Build(teamColor);

            _anim = gameObject.AddComponent<FrameAnimator>();
            _anim.Init(sr);
            _sr.sprite = _frames.Idle[0];

            _homePos = transform.position;
            _offscreenPos = _homePos + (faceDown ? Vector3.up : Vector3.down) * 7f;
            if (faceDown) transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }

        public void SetIdleOffscreen()
        {
            StopAllCoroutines();
            transform.position = _offscreenPos;
            _anim.Play(_frames.Idle, 3f, loop: true);
        }

        public IEnumerator WalkIn(float duration)
        {
            _anim.Play(_frames.Idle, 6f, loop: true); // brisker "walking" bob
            float t = 0f;
            Vector3 start = _offscreenPos;
            while (t < duration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, _homePos, Mathf.Clamp01(t / duration));
                yield return null;
            }
            transform.position = _homePos;
        }

        public void Stance() => _anim.Play(_frames.Ready, 4f, loop: true);

        public void PlayShoot() => _anim.Play(_frames.Shoot, 12f, loop: false);

        public void PlayDeath() => _anim.Play(_frames.Death, 6f, loop: false);
    }
}
