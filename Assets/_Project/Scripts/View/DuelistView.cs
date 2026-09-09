using System.Collections;
using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// Visual representation of a duelist using frame-by-frame pixel animation.
    /// Landscape showdown: the two gunslingers stand at opposite ends of the street and walk in
    /// from their own edge; the right-hand one is mirrored (flipX) so the pair faces the centre.
    /// </summary>
    public class DuelistView : MonoBehaviour
    {
        SpriteRenderer _sr;
        FrameAnimator _anim;
        CowboyFrames _frames;
        Vector3 _homePos;
        Vector3 _offscreenPos;
        bool _rightSide;

        /// <summary>Idle frame for UI portraits — same cached sprites as the cowboy on the field.</summary>
        public Sprite IdlePortrait => _frames != null && _frames.Idle != null && _frames.Idle.Length > 0
            ? _frames.Idle[0] : null;

        /// <summary><paramref name="rightSide"/> = this duelist stands on the right of the street
        /// (walks in from the right, sprite mirrored to face left).</summary>
        public void Setup(SpriteRenderer sr, CowboyLook look, bool rightSide)
        {
            _sr = sr;
            _rightSide = rightSide;
            _frames = CowboyArt.Build(look);

            _anim = gameObject.AddComponent<FrameAnimator>();
            _anim.Init(sr);
            _sr.sprite = _frames.Idle[0];
            _sr.flipX = rightSide; // right duelist faces left toward the centre

            _homePos = transform.position;
            _offscreenPos = _homePos + (rightSide ? Vector3.right : Vector3.left) * 9f;
        }

        public void SetIdleOffscreen()
        {
            StopAllCoroutines();
            transform.position = _offscreenPos;
            _anim.Play(_frames.Idle, 3f, loop: true);
        }

        /// <summary>Walk-in runs on this view so <see cref="SetIdleOffscreen"/> can stop it.</summary>
        public void BeginWalkIn(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(WalkIn(duration));
        }

        IEnumerator WalkIn(float duration)
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
