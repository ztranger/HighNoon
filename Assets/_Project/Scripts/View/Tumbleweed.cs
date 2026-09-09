using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// A tumbleweed that rolls across the field for Wild-West atmosphere. Spawned by
    /// the DuelManager during the tension phase. Self-destructs once off-screen.
    /// </summary>
    public class Tumbleweed : MonoBehaviour
    {
        float _speedX;
        float _spin;
        float _endX;
        float _baseY;
        float _bobAmp;
        float _phase;

        public static void Spawn()
        {
            var cam = Camera.main;
            if (cam == null) return;

            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            const float margin = 1.5f;

            var go = new GameObject("Tumbleweed");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderArt.Tumbleweed(); // cached — Destroy(GO) must not Destroy the texture
            sr.sortingOrder = 20;

            bool leftToRight = Random.value < 0.5f;
            float y = Random.Range(-halfH * 0.6f, halfH * 0.6f);
            float startX = leftToRight ? -halfW - margin : halfW + margin;

            go.transform.position = new Vector3(startX, y, 0f);
            float s = Random.Range(0.5f, 0.8f);
            go.transform.localScale = new Vector3(s, s, 1f);

            var tw = go.AddComponent<Tumbleweed>();
            tw._endX = leftToRight ? halfW + margin : -halfW - margin;
            tw._speedX = (leftToRight ? 1f : -1f) * Random.Range(3.5f, 5.5f);
            tw._spin = -(leftToRight ? 1f : -1f) * Random.Range(180f, 320f);
            tw._baseY = y;
            tw._bobAmp = Random.Range(0.1f, 0.3f);
            tw._phase = Random.value * 6.283f;
        }

        void Update()
        {
            transform.position += new Vector3(_speedX * Time.deltaTime, 0f, 0f);
            _phase += Time.deltaTime * 6f;
            var p = transform.position;
            p.y = _baseY + Mathf.Sin(_phase) * _bobAmp;
            transform.position = p;
            transform.Rotate(0f, 0f, _spin * Time.deltaTime);

            if ((_speedX > 0f && transform.position.x > _endX) ||
                (_speedX < 0f && transform.position.x < _endX))
            {
                Destroy(gameObject);
            }
        }
    }
}
