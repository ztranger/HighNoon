using System;
using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    public sealed class GunsScreen
    {
        public RectTransform Root { get; private set; }

        Text _name;
        Image _icon;
        Action _onChanged;

        public void Build(RectTransform root, UiBuild ui, Action back, Action onChanged)
        {
            Root = root;
            _onChanged = onChanged;
            ui.Back(root, back);
            ui.Text(root, "Head", "WEAPON", 64, new Vector2(0.5f, 0.88f), Vector2.zero, 900, 90, UiBuild.Gold, FontStyle.Bold);

            var iconGo = new GameObject("WeaponIcon");
            var irt = iconGo.AddComponent<RectTransform>();
            irt.SetParent(root, false);
            irt.anchorMin = irt.anchorMax = new Vector2(0.32f, 0.48f);
            irt.pivot = new Vector2(0.5f, 0.5f);
            irt.sizeDelta = new Vector2(720f, 420f);
            _icon = iconGo.AddComponent<Image>();
            _icon.raycastTarget = false;
            _icon.preserveAspect = true;

            _name = ui.Text(root, "WName", "REVOLVER", 56, new Vector2(0.72f, 0.58f), Vector2.zero, 640, 90, UiBuild.Gold, FontStyle.Bold);

            var prev = ui.Button(root, "GunPrev", "<", new Vector2(0.72f, 0.38f), new Vector2(-180f, 0f), 120, 120, 56, UiBuild.Normal, out _);
            var next = ui.Button(root, "GunNext", ">", new Vector2(0.72f, 0.38f), new Vector2(180f, 0f), 120, 120, 56, UiBuild.Normal, out _);
            prev.onClick.AddListener(() => Cycle(-1));
            next.onClick.AddListener(() => Cycle(1));

            ui.Text(root, "Hint", "tap  ◄  ►  to choose your iron", 28, new Vector2(0.72f, 0.18f), Vector2.zero, 700, 50, new Color(0.72f, 0.66f, 0.52f), FontStyle.Normal);
            Refresh();
        }

        public void Refresh()
        {
            var w = Weapons.Selected;
            if (_name != null) _name.text = w.Name;
            if (_icon != null) _icon.sprite = WeaponArt.For(GameSettings.SelectedWeapon);
        }

        void Cycle(int dir)
        {
            GameSettings.SelectedWeapon = (GameSettings.SelectedWeapon + dir + Weapons.Count) % Weapons.Count;
            Refresh();
            _onChanged?.Invoke();
            Sfx.PlayClip(AudioBank.GunshotFor(Weapons.Selected, ProcAudio.Gunshot));
        }
    }
}
