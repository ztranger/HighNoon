using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.EnhancedTouch;

namespace HighNoon
{
    /// <summary>
    /// A one-time interactive tutorial (code-built overlay UI, no world sprites so no Light2D
    /// needed). Teaches: tap your (bottom) half, wait for BANG (early = false start), and the
    /// Timing "sweet spot" bar. Auto-shown on first launch; replayable from the menu. Uses the
    /// same <see cref="HumanDuelInput"/> the real duels use, so practice matches the game 1:1.
    /// </summary>
    public class TutorialBootstrap : MonoBehaviour
    {
        static readonly Rect FullScreen = new Rect(0f, 0f, 1f, 1f);
        static readonly Rect BottomHalf = new Rect(0f, 0f, 1f, 0.5f);
        static readonly Color Gold = new Color(0.95f, 0.82f, 0.38f);
        static readonly Color Warn = new Color(1f, 0.55f, 0.25f);

        Font _font;
        Image _flash;
        Text _prompt, _hint;
        Image _playerImg, _oppImg;
        CowboyFrames _playerFrames, _oppFrames;
        DuelAudio _audio;
        HumanDuelInput _tapAny, _tapBottom;

        static double Now => Time.realtimeSinceStartupAsDouble;

        void Start()
        {
            AppInit.Apply();
            EnhancedTouchSupport.Enable();

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            SetupCamera();
            SetupEventSystem();
            BuildUI();

            _audio = new GameObject("TutorialAudio").AddComponent<DuelAudio>();
            _audio.Setup();

            _tapAny = new HumanDuelInput(FullScreen, Key.Space);
            _tapBottom = new HumanDuelInput(BottomHalf, Key.S);

            StartCoroutine(Run());
        }

        // ---- flow ----

        IEnumerator Run()
        {
            yield return StartCoroutine(StepIntro());
            yield return StartCoroutine(StepReaction());
            yield return StartCoroutine(StepTiming());
            yield return StartCoroutine(StepDone());
            Finish();
        }

        IEnumerator StepIntro()
        {
            Idle();
            Show("WELCOME, STRANGER", "You hold your end of the phone.\nThis is YOUR half — the bottom.\nTap it to draw your gun.\n\nTap anywhere to continue.");
            yield return StartCoroutine(WaitTap(_tapAny));
        }

        IEnumerator StepReaction()
        {
            Show("THE QUICK DRAW", "Wait for the word — then tap the bottom FAST.\nThe first clean tap wins.\nBut tap too EARLY and it's a false start — you lose.\n\nTap to begin.");
            yield return StartCoroutine(WaitTap(_tapAny));

            bool done = false;
            while (!done)
            {
                Idle();
                Show("STEADY…", "Don't tap yet — wait for BANG!");
                _audio.StartTension();

                _tapBottom.ResetInput(); _tapBottom.Arm();
                float tension = Random.Range(1.6f, 3.2f), e = 0f;
                bool early = false;
                while (e < tension)
                {
                    e += Time.deltaTime;
                    _tapBottom.Tick(Now);
                    if (_tapBottom.HasFired) { early = true; break; }
                    yield return null;
                }
                _audio.StopTension();

                if (early)
                {
                    Haptics.Heavy();
                    _audio.Death();
                    Show("TOO EARLY!", "That's a false start — in a real duel you'd be dead.\nBreholster and wait for the BANG.", Warn);
                    yield return new WaitForSeconds(2.0f);
                    continue; // try the draw again
                }

                double bang = Now;
                _audio.Bang(); Flash(); Haptics.Medium();
                Show("BANG!", "TAP the bottom half — NOW!", Gold);

                while (!_tapBottom.HasFired) { _tapBottom.Tick(Now); yield return null; }
                float ms = (float)(_tapBottom.FireTimeRealtime - bang) * 1000f;

                PlayerShoot(); OpponentDie(); _audio.Gunshot(); Haptics.Light();
                Show("CLEAN DRAW!", $"{Mathf.Max(0f, ms):0} ms.\nFastest valid tap always wins.\n\nTap to continue.");
                yield return StartCoroutine(WaitTap(_tapAny));
                done = true;
            }
        }

        IEnumerator StepTiming()
        {
            Idle();
            Show("THE STEADY HAND", "Some duels test your aim instead.\nA marker sweeps across a bar —\nstop it in the GREEN zone.\n\nTap to begin.");
            yield return StartCoroutine(WaitTap(_tapAny));

            bool done = false;
            while (!done)
            {
                var bar = new GameObject("TutorialBar").AddComponent<TimingBar>();
                float gc = Random.Range(0.30f, 0.70f);
                bar.Build(transform, _font, false, new Vector2(0f, -150f), 880f, 84f, gc, 0.14f, "TAP IN THE GREEN");

                Idle();
                Show("AIM…", "Tap the bottom half when the marker is on GREEN.");
                _audio.StartTension();
                _tapBottom.ResetInput(); _tapBottom.Arm();

                float t = 0f; bool tapped = false;
                while (t < 8f)
                {
                    t += Time.deltaTime;
                    float x = Mathf.PingPong(t * 0.8f, 1f);
                    bar.SetSweepX(x);
                    _tapBottom.Tick(Now);
                    if (_tapBottom.HasFired) { bar.Lock(x); tapped = true; break; }
                    yield return null;
                }
                _audio.StopTension();

                bool hit = tapped && bar.IsHit(bar.LockedX);
                _audio.Gunshot(); Haptics.Light(); PlayerShoot();

                if (hit)
                {
                    OpponentDie();
                    Show("BULLSEYE!", "A green hit is a clean shot.\nMiss it, and the other fella shoots first.\n\nTap to continue.", Gold);
                    yield return StartCoroutine(WaitTap(_tapAny));
                    Destroy(bar.gameObject);
                    done = true;
                }
                else
                {
                    Haptics.Heavy();
                    Show("MISSED THE GREEN", "Off the mark — you'd be the one to fall.\nTry again.", Warn);
                    yield return new WaitForSeconds(1.8f);
                    Destroy(bar.gameObject);
                }
            }
        }

        IEnumerator StepDone()
        {
            Idle();
            Show("YOU'RE READY", "That's all there is to it, partner.\nQuickest, steadiest hand wins.\n\nTap to ride out.", Gold);
            yield return StartCoroutine(WaitTap(_tapAny));
        }

        void Finish()
        {
            GameSettings.TutorialDone = true;
            DuelFlow.Menu();
        }

        IEnumerator WaitTap(HumanDuelInput input)
        {
            input.ResetInput(); input.Arm();
            // small guard so the tap that ended the previous step isn't read again this frame
            yield return null;
            while (!input.HasFired) { input.Tick(Now); yield return null; }
        }

        // ---- cowboy visuals (UI images) ----

        void Idle()
        {
            _playerImg.sprite = _playerFrames.Idle[0];
            _playerImg.color = Color.white;
            _playerImg.rectTransform.localRotation = Quaternion.identity;
            _oppImg.sprite = _oppFrames.Idle[0];
            _oppImg.color = Color.white;
            _oppImg.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f); // faces the player
            _oppImg.rectTransform.anchoredPosition = new Vector2(0f, 560f);
        }

        void PlayerShoot() => _playerImg.sprite = _playerFrames.Shoot[_playerFrames.Shoot.Length - 1];

        void OpponentDie()
        {
            _oppImg.sprite = _oppFrames.Death[_oppFrames.Death.Length - 1];
            _oppImg.color = new Color(1f, 1f, 1f, 0.7f);
            _oppImg.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 250f);
            _oppImg.rectTransform.anchoredPosition = new Vector2(60f, 500f);
        }

        // ---- UI build ----

        void BuildUI()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 1f;
            gameObject.AddComponent<GraphicRaycaster>();

            // Warm background.
            var bg = NewRect("BG", Vector2.zero, Vector2.one);
            bg.offsetMin = Vector2.zero; bg.offsetMax = Vector2.zero;
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0.78f, 0.62f, 0.36f);
            bgImg.raycastTarget = false;

            // A subtle divider between the two halves.
            var mid = Center("Divider", new Vector2(0f, 0f), new Vector2(1200f, 8f));
            var midImg = mid.gameObject.AddComponent<Image>();
            midImg.color = new Color(0.35f, 0.26f, 0.16f, 0.8f);
            midImg.raycastTarget = false;

            _oppFrames = CowboyArt.Build(CowboyLook.Enemy());
            _playerFrames = CowboyArt.Build(CowboyLook.Player());
            _oppImg = MakeCowboy("Opponent", new Vector2(0f, 560f), true, _oppFrames);
            _playerImg = MakeCowboy("Player", new Vector2(0f, -560f), false, _playerFrames);

            _prompt = MakeText("Prompt", 82, new Vector2(0f, 300f), 1040, 200, Gold, FontStyle.Bold);
            _hint = MakeText("Hint", 44, new Vector2(0f, 90f), 980, 320, new Color(0.98f, 0.94f, 0.86f), FontStyle.Normal);

            // Full-screen flash overlay.
            var fl = NewRect("Flash", Vector2.zero, Vector2.one);
            fl.offsetMin = Vector2.zero; fl.offsetMax = Vector2.zero;
            _flash = fl.gameObject.AddComponent<Image>();
            _flash.color = new Color(1f, 1f, 1f, 0f);
            _flash.raycastTarget = false;

            // SKIP button (top-right).
            var skip = MakeButton("Skip", "SKIP", new Vector2(1f, 1f), new Vector2(-160f, -70f), 280, 84, 40,
                new Color(0.45f, 0.4f, 0.3f));
            skip.onClick.AddListener(() => { Sfx.Click(); Haptics.Light(); Finish(); });
        }

        Image MakeCowboy(string name, Vector2 pos, bool faceDown, CowboyFrames frames)
        {
            var rt = Center(name, pos, new Vector2(240f, 320f));
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = frames.Idle[0];
            img.preserveAspect = true;
            img.raycastTarget = false;
            if (faceDown) rt.localRotation = Quaternion.Euler(0f, 0f, 180f);
            return img;
        }

        RectTransform NewRect(string name, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            return rt;
        }

        RectTransform Center(string name, Vector2 pos, Vector2 size)
        {
            var rt = NewRect(name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        Text MakeText(string name, int size, Vector2 pos, float w, float h, Color color, FontStyle style)
        {
            var rt = Center(name, pos, new Vector2(w, h));
            var txt = rt.gameObject.AddComponent<Text>();
            txt.font = _font; txt.fontSize = size; txt.fontStyle = style;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            var outline = rt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);
            return txt;
        }

        Button MakeButton(string name, string label, Vector2 anchor, Vector2 pos, float w, float h, int fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var t = new GameObject("Label");
            var trt = t.AddComponent<RectTransform>();
            trt.SetParent(rt, false);
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(w, h);
            var txt = t.AddComponent<Text>();
            txt.text = label; txt.font = _font; txt.fontSize = fontSize; txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter; txt.color = new Color(0.96f, 0.92f, 0.82f);
            txt.raycastTarget = false;
            return btn;
        }

        void Show(string prompt, string hint) => Show(prompt, hint, Gold);

        void Show(string prompt, string hint, Color promptColor)
        {
            _prompt.text = prompt; _prompt.color = promptColor;
            _hint.text = hint;
        }

        void Flash()
        {
            StopCoroutine(nameof(FlashRoutine));
            StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            float t = 0f, dur = 0.18f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _flash.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.5f, 0f, t / dur));
                yield return null;
            }
            _flash.color = new Color(1f, 1f, 1f, 0f);
        }

        void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.78f, 0.62f, 0.36f);
            AppInit.EnsureAudioListener(cam);
        }

        void SetupEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }
}
