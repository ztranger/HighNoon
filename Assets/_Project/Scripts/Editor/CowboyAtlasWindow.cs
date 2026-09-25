using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HighNoon.EditorTools
{
    /// <summary>
    /// Shows how an atlas is carved into frames and lets you nudge each cell so the
    /// cowboy plants on the same spot. Offsets save next to the PNG as a JSON sidecar
    /// (same Resources path) and <see cref="CowboySheet"/> applies them as sprite pivots.
    /// </summary>
    public class CowboyAtlasWindow : EditorWindow
    {
        [MenuItem("High Noon/Cowboy Atlas")]
        public static void Open() => GetWindow<CowboyAtlasWindow>("Cowboy Atlas");

        CowboyCharacter[] _chars;
        int _charIndex;
        List<CowboySheet.AtlasClip> _clips = new List<CowboySheet.AtlasClip>();
        int _clipIndex;
        CowboyAtlasOffsets _off;
        int _sel;
        bool _dirty;
        bool _onion = true;
        bool _playing;
        double _playStart;
        Vector2 _dragMouse;
        Vector2Int _dragOff;
        bool _dragging;

        CowboyCharacter Char =>
            _chars != null && _chars.Length > 0
                ? _chars[Mathf.Clamp(_charIndex, 0, _chars.Length - 1)]
                : null;

        bool HasClip => _clips != null && _clips.Count > 0;
        CowboySheet.AtlasClip Clip => _clips[Mathf.Clamp(_clipIndex, 0, _clips.Count - 1)];
        string ClipPath => HasClip ? Clip.Path : null;

        float PlayFps
        {
            get
            {
                if (_off != null && _off.fps > 0.01f) return _off.fps;
                return CowboySheet.DefaultFps(HasClip ? Clip.Name : "Walk");
            }
        }

        void OnEnable()
        {
            wantsMouseMove = true;
            minSize = new Vector2(980f, 620f);
            Reload();
        }

        void OnDisable()
        {
            if (_dirty) Save();
        }

        void Reload()
        {
            var list = new List<CowboyCharacter>();
            foreach (var c in CowboyCatalog.Roster)
                if (c != null && CowboySheet.Clips(c).Count > 0) list.Add(c);
            _chars = list.ToArray();
            RebuildClips();
            LoadOffsets();
        }

        void RebuildClips()
        {
            _clips = CowboySheet.Clips(Char);
            if (_clipIndex >= _clips.Count) _clipIndex = 0;
        }

        void LoadOffsets()
        {
            _off = null;
            _dirty = false;
            var c = Char;
            if (c == null || !HasClip || !CowboySheet.TryGrid(Clip.Path, Clip.Cols, Clip.Rows, out var g)) return;
            _off = CowboySheet.LoadOffsets(Clip.Path) ?? CowboyAtlasOffsets.Create(g.Cols, g.Rows);
            _off.Ensure(g.Cols, g.Rows);
            _sel = Mathf.Clamp(_sel, 0, g.Count - 1);
        }

        void OnGUI()
        {
            if (_chars == null || _chars.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No atlas characters yet. Set CowboyCharacter.Atlas on a catalog entry.",
                    MessageType.Info);
                if (GUILayout.Button("Reload catalog", GUILayout.Height(28))) Reload();
                return;
            }

            HandleKeys();

            var c = Char;
            DrawToolbar(c);
            if (!HasClip || !CowboySheet.TryGrid(Clip.Path, Clip.Cols, Clip.Rows, out var g))
            {
                EditorGUILayout.HelpBox("Could not load Resources/" + (ClipPath ?? "(none)"), MessageType.Warning);
                return;
            }
            if (_off == null) LoadOffsets();
            _off.Ensure(g.Cols, g.Rows);

            EditorGUILayout.BeginHorizontal();
            DrawAtlas(g);
            DrawSide(g);
            EditorGUILayout.EndHorizontal();

            if (_playing || Event.current.type == EventType.MouseMove)
                Repaint();
        }

        void DrawToolbar(CowboyCharacter c)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var names = new string[_chars.Length];
            for (int i = 0; i < _chars.Length; i++) names[i] = _chars[i].Id;
            int next = EditorGUILayout.Popup(_charIndex, names, EditorStyles.toolbarPopup, GUILayout.Width(180));
            if (next != _charIndex)
            {
                if (_dirty) Save();
                _charIndex = next;
                _clipIndex = 0;
                _sel = 0;
                RebuildClips();
                LoadOffsets();
            }

            if (_clips.Count > 1)
            {
                var clipNames = new string[_clips.Count];
                for (int i = 0; i < _clips.Count; i++) clipNames[i] = _clips[i].Name;
                int clip = EditorGUILayout.Popup(_clipIndex, clipNames, EditorStyles.toolbarPopup, GUILayout.Width(80));
                if (clip != _clipIndex)
                {
                    if (_dirty) Save();
                    _clipIndex = clip;
                    _sel = 0;
                    _playing = false;
                    LoadOffsets();
                }
            }

            GUILayout.Label(ClipPath ?? "", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            _onion = GUILayout.Toggle(_onion, "Onion", EditorStyles.toolbarButton, GUILayout.Width(56));
            if (GUILayout.Button(_playing ? "Stop" : "Play", EditorStyles.toolbarButton, GUILayout.Width(48)))
                TogglePlay();
            EditorGUI.BeginChangeCheck();
            float fps = EditorGUILayout.FloatField(PlayFps, GUILayout.Width(48));
            if (EditorGUI.EndChangeCheck() && _off != null)
            {
                _off.fps = Mathf.Clamp(fps, 1f, 24f);
                MarkDirty();
            }
            GUILayout.Label("fps", EditorStyles.miniLabel, GUILayout.Width(22));
            GUI.enabled = _dirty;
            if (GUILayout.Button(_dirty ? "Save *" : "Save", EditorStyles.toolbarButton, GUILayout.Width(56)))
                Save();
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(
                "Click a cell on the sheet. Arrows (or WASD) nudge that frame — Shift = 10px, Alt = every frame. Drag in the preview. The crosshair is the plant point (feet).",
                EditorStyles.miniLabel);
        }

        void DrawAtlas(CowboySheet.AtlasGrid g)
        {
            var view = GUILayoutUtility.GetRect(
                GUIContent.none, GUIStyle.none,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true),
                GUILayout.MinWidth(420), GUILayout.MinHeight(400));

            EditorGUI.DrawRect(view, new Color(0.10f, 0.10f, 0.12f));

            float pad = 10f;
            float scale = Mathf.Min(
                (view.width - pad * 2f) / g.Tex.width,
                (view.height - pad * 2f) / g.Tex.height);
            float dw = g.Tex.width * scale;
            float dh = g.Tex.height * scale;
            var img = new Rect(
                view.x + (view.width - dw) * 0.5f,
                view.y + (view.height - dh) * 0.5f,
                dw, dh);

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && img.Contains(e.mousePosition))
            {
                var local = (e.mousePosition - img.position) / scale;
                int col = Mathf.FloorToInt((local.x - g.X0) / g.CellW);
                int row = Mathf.FloorToInt((local.y - g.Top0) / g.CellH);
                if (col >= 0 && col < g.Cols && row >= 0 && row < g.Rows)
                {
                    _playing = false;
                    _sel = row * g.Cols + col;
                    e.Use();
                    Repaint();
                }
            }

            if (e.type != EventType.Repaint) return;

            GUI.DrawTexture(img, g.Tex, ScaleMode.StretchToFill, true);

            // leftover margin
            if (g.X0 > 0 || g.Top0 > 0)
            {
                var used = new Rect(img.x + g.X0 * scale, img.y + g.Top0 * scale,
                    g.Cols * g.CellW * scale, g.Rows * g.CellH * scale);
                EditorGUI.DrawRect(new Rect(img.x, img.y, img.width, used.yMin - img.y), new Color(0, 0, 0, 0.35f));
            }

            int hover = HoverCell(g, img, scale, e.mousePosition);
            int shown = ShownIndex(g);
            for (int i = 0; i < g.Count; i++)
            {
                var cell = g.GuiCell(i);
                var r = new Rect(img.x + cell.x * scale, img.y + cell.y * scale, cell.width * scale, cell.height * scale);
                bool sel = i == shown;
                bool hov = i == hover && !sel;
                var color = sel ? new Color(1f, 0.82f, 0.18f, 1f)
                    : hov ? new Color(0.45f, 0.85f, 1f, 0.95f)
                    : new Color(0.95f, 0.95f, 1f, 0.35f);
                DrawFrame(r, sel ? 3f : 1f, color);
                var off = _off.Get(i);
                string tag = i.ToString("00");
                if (off != Vector2Int.zero) tag += $"  {off.x:+#;-#;0},{off.y:+#;-#;0}";
                GUI.Label(new Rect(r.x + 4, r.y + 2, r.width - 8, 18), tag, EditorStyles.whiteLabel);
            }
        }

        int HoverCell(CowboySheet.AtlasGrid g, Rect img, float scale, Vector2 mouse)
        {
            if (!img.Contains(mouse)) return -1;
            var local = (mouse - img.position) / scale;
            int col = Mathf.FloorToInt((local.x - g.X0) / g.CellW);
            int row = Mathf.FloorToInt((local.y - g.Top0) / g.CellH);
            if (col < 0 || col >= g.Cols || row < 0 || row >= g.Rows) return -1;
            return row * g.Cols + col;
        }

        void DrawSide(CowboySheet.AtlasGrid g)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(360));

            int shown = ShownIndex(g);
            var n = _off.Get(shown);
            g.CellOf(shown, out int col, out int row);

            GUILayout.Space(6);
            EditorGUILayout.LabelField(
                $"Frame {shown:00}   col {col}  row {row}   cell {g.CellW}×{g.CellH}",
                EditorStyles.boldLabel);

            var preview = GUILayoutUtility.GetRect(340, 360, GUILayout.ExpandWidth(true));
            DrawPreview(g, preview, shown);

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("◀", GUILayout.Width(36), GUILayout.Height(28))) Select(shown - 1, g);
            if (GUILayout.Button("▶", GUILayout.Width(36), GUILayout.Height(28))) Select(shown + 1, g);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawNudgeCol(g);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Offset px");
            int nx = EditorGUILayout.IntField(n.x, GUILayout.Width(64));
            int ny = EditorGUILayout.IntField(n.y, GUILayout.Width(64));
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) SetOffset(shown, nx, ny);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset frame")) SetOffset(shown, 0, 0);
            if (GUILayout.Button("Reset all"))
            {
                for (int i = 0; i < g.Count; i++) _off.Set(i, 0, 0);
                MarkDirty();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                $"Atlas {g.Tex.width}×{g.Tex.height}  ·  {g.Cols}×{g.Rows} grid\n" +
                "Positive X = character moves right. Positive Y = character moves up.",
                MessageType.None);

            EditorGUILayout.EndVertical();
        }

        void DrawNudgeCol(CowboySheet.AtlasGrid g)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("↑", GUILayout.Width(40), GUILayout.Height(28))) Nudge(0, 1, g);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("←", GUILayout.Width(40), GUILayout.Height(28))) Nudge(-1, 0, g);
            if (GUILayout.Button("●", GUILayout.Width(40), GUILayout.Height(28))) SetOffset(ShownIndex(g), 0, 0);
            if (GUILayout.Button("→", GUILayout.Width(40), GUILayout.Height(28))) Nudge(1, 0, g);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("↓", GUILayout.Width(40), GUILayout.Height(28))) Nudge(0, -1, g);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        void DrawPreview(CowboySheet.AtlasGrid g, Rect box, int shown)
        {
            EditorGUI.DrawRect(box, new Color(0.08f, 0.08f, 0.10f));
            float pad = 16f;
            float scale = Mathf.Min((box.width - pad * 2f) / g.CellW, (box.height - pad * 2f) / g.CellH);
            var stage = new Rect(
                box.x + (box.width - g.CellW * scale) * 0.5f,
                box.y + (box.height - g.CellH * scale) * 0.5f,
                g.CellW * scale, g.CellH * scale);

            HandlePreviewDrag(g, stage, scale, shown);

            if (Event.current.type == EventType.Repaint)
            {
                DrawChecker(stage);
                GUI.BeginGroup(box);
                var localStage = new Rect(stage.x - box.x, stage.y - box.y, stage.width, stage.height);
                if (_onion)
                {
                    for (int i = 0; i < g.Count; i++)
                    {
                        if (i == shown) continue;
                        DrawCell(g, i, localStage, scale, new Color(1f, 1f, 1f, 0.22f));
                    }
                }
                DrawCell(g, shown, localStage, scale, Color.white);
                GUI.EndGroup();

                // plant crosshair — bottom centre of the cell (pivot with FeetInset=0)
                float cx = stage.x + stage.width * 0.5f;
                float fy = stage.yMax;
                Handles.BeginGUI();
                Handles.color = new Color(1f, 0.25f, 0.25f, 0.95f);
                Handles.DrawLine(new Vector3(stage.x, fy), new Vector3(stage.xMax, fy));
                Handles.DrawLine(new Vector3(cx, stage.y), new Vector3(cx, fy));
                Handles.EndGUI();
                DrawFrame(stage, 1f, new Color(1f, 1f, 1f, 0.25f));
            }
        }

        void HandlePreviewDrag(CowboySheet.AtlasGrid g, Rect stage, float scale, int shown)
        {
            var e = Event.current;
            if (e.button != 0) return;
            if (e.type == EventType.MouseDown && stage.Contains(e.mousePosition))
            {
                _playing = false;
                _dragging = true;
                _dragMouse = e.mousePosition;
                _dragOff = _off.Get(shown);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _dragging)
            {
                var d = (e.mousePosition - _dragMouse) / scale;
                // GUI Y is down; positive offset Y is up.
                SetOffset(shown, _dragOff.x + Mathf.RoundToInt(d.x), _dragOff.y - Mathf.RoundToInt(d.y));
                e.Use();
            }
            else if (e.type == EventType.MouseUp && _dragging)
            {
                _dragging = false;
                e.Use();
            }
        }

        void DrawCell(CowboySheet.AtlasGrid g, int i, Rect stage, float scale, Color tint)
        {
            var n = _off.Get(i);
            var dest = new Rect(
                stage.x + n.x * scale,
                stage.y - n.y * scale,
                stage.width, stage.height);
            var prev = GUI.color;
            GUI.color = tint;
            GUI.DrawTextureWithTexCoords(dest, g.Tex, g.Uv(i), true);
            GUI.color = prev;
        }

        static void DrawChecker(Rect r)
        {
            const int n = 12;
            float s = r.width / n;
            for (int y = 0; y < n * 2; y++)
            for (int x = 0; x < n; x++)
            {
                if (((x + y) & 1) == 0) continue;
                var c = new Rect(r.x + x * s, r.y + y * s, s, s);
                if (c.yMin > r.yMax) return;
                c.yMax = Mathf.Min(c.yMax, r.yMax);
                c.xMax = Mathf.Min(c.xMax, r.xMax);
                EditorGUI.DrawRect(c, new Color(1f, 1f, 1f, 0.04f));
            }
        }

        static void DrawFrame(Rect r, float t, Color color)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, t), color);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - t, r.width, t), color);
            EditorGUI.DrawRect(new Rect(r.x, r.y, t, r.height), color);
            EditorGUI.DrawRect(new Rect(r.xMax - t, r.y, t, r.height), color);
        }

        void HandleKeys()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;
            if (EditorGUIUtility.editingTextField) return;
            if (!HasClip || !CowboySheet.TryGrid(Clip.Path, Clip.Cols, Clip.Rows, out var g)) return;
            int step = e.shift ? 10 : 1;
            bool used = true;
            switch (e.keyCode)
            {
                case KeyCode.LeftArrow:
                case KeyCode.A: Nudge(-step, 0, g, e.alt); break;
                case KeyCode.RightArrow:
                case KeyCode.D: Nudge(step, 0, g, e.alt); break;
                case KeyCode.UpArrow:
                case KeyCode.W: Nudge(0, step, g, e.alt); break;
                case KeyCode.DownArrow:
                case KeyCode.S: Nudge(0, -step, g, e.alt); break;
                case KeyCode.LeftBracket: Select(_sel - 1, g); break;
                case KeyCode.RightBracket: Select(_sel + 1, g); break;
                case KeyCode.Space: TogglePlay(); break;
                default: used = false; break;
            }
            if (used) e.Use();
        }

        int ShownIndex(CowboySheet.AtlasGrid g)
        {
            if (!_playing) return Mathf.Clamp(_sel, 0, g.Count - 1);
            int i = (int)((EditorApplication.timeSinceStartup - _playStart) * PlayFps);
            return ((i % g.Count) + g.Count) % g.Count;
        }

        void TogglePlay()
        {
            _playing = !_playing;
            if (_playing) _playStart = EditorApplication.timeSinceStartup;
        }

        void Select(int i, CowboySheet.AtlasGrid g)
        {
            _playing = false;
            _sel = (i % g.Count + g.Count) % g.Count;
        }

        void Nudge(int dx, int dy, CowboySheet.AtlasGrid g, bool all = false)
        {
            if (all)
            {
                for (int i = 0; i < g.Count; i++)
                {
                    var n = _off.Get(i);
                    _off.Set(i, n.x + dx, n.y + dy);
                }
                MarkDirty();
                return;
            }
            int shown = ShownIndex(g);
            var cur = _off.Get(shown);
            SetOffset(shown, cur.x + dx, cur.y + dy);
        }

        void SetOffset(int i, int x, int y)
        {
            var cur = _off.Get(i);
            if (cur.x == x && cur.y == y) return;
            _off.Set(i, x, y);
            MarkDirty();
        }

        void MarkDirty()
        {
            _dirty = true;
            Repaint();
        }

        void Save()
        {
            var c = Char;
            if (c == null || _off == null) return;
            string abs = Path.GetFullPath(Path.Combine(Application.dataPath, "_Project/Resources", Clip.Path + ".json"));
            Directory.CreateDirectory(Path.GetDirectoryName(abs));
            File.WriteAllText(abs, JsonUtility.ToJson(_off, true));
            string asset = "Assets/_Project/Resources/" + Clip.Path + ".json";
            AssetDatabase.ImportAsset(asset);
            CowboySheet.Invalidate(c.Id);
            _dirty = false;
        }
    }
}
