using System;
using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    public sealed class SettingsScreen
    {
        public RectTransform Root { get; private set; }

        Image _sfxImg, _musicImg, _vibImg;
        Text _sfxLabel, _musicLabel, _vibLabel;

        public void Build(RectTransform root, UiBuild ui, Action back)
        {
            Root = root;
            ui.Back(root, back);
            ui.Text(root, "Head", "SETTINGS", 64, new Vector2(0.5f, 0.88f), Vector2.zero, 900, 90, UiBuild.Gold, FontStyle.Bold);

            var sfx = ui.Button(root, "SfxToggle", "SFX", new Vector2(0.5f, 0.58f), new Vector2(-480f, 0f), 420, 110, 36, UiBuild.Normal, out _sfxImg);
            var mus = ui.Button(root, "MusicToggle", "MUSIC / WIND", new Vector2(0.5f, 0.58f), Vector2.zero, 420, 110, 32, UiBuild.Normal, out _musicImg);
            var vib = ui.Button(root, "VibToggle", "VIBRATION", new Vector2(0.5f, 0.58f), new Vector2(480f, 0f), 420, 110, 36, UiBuild.Normal, out _vibImg);
            _sfxLabel = sfx.GetComponentInChildren<Text>();
            _musicLabel = mus.GetComponentInChildren<Text>();
            _vibLabel = vib.GetComponentInChildren<Text>();
            sfx.onClick.AddListener(() => { GameSettings.SfxEnabled = !GameSettings.SfxEnabled; Refresh(); });
            mus.onClick.AddListener(() => { GameSettings.MusicEnabled = !GameSettings.MusicEnabled; Refresh(); });
            vib.onClick.AddListener(() => { GameSettings.HapticsEnabled = !GameSettings.HapticsEnabled; Refresh(); Haptics.Medium(); });

            var howto = ui.Button(root, "HowTo", "HOW TO PLAY", new Vector2(0.5f, 0.32f), Vector2.zero, 520, 110, 40, new Color(0.60f, 0.50f, 0.34f), out _);
            howto.onClick.AddListener(() => DuelFlow.Tutorial());
            Refresh();
        }

        public void Refresh()
        {
            if (_sfxImg != null)
            {
                _sfxImg.color = GameSettings.SfxEnabled ? UiBuild.Selected : UiBuild.Disabled;
                if (_sfxLabel != null) _sfxLabel.text = GameSettings.SfxEnabled ? "SFX: ON" : "SFX: OFF";
            }
            if (_musicImg != null)
            {
                _musicImg.color = GameSettings.MusicEnabled ? UiBuild.Selected : UiBuild.Disabled;
                if (_musicLabel != null) _musicLabel.text = GameSettings.MusicEnabled ? "MUSIC / WIND: ON" : "MUSIC / WIND: OFF";
            }
            if (_vibImg != null)
            {
                _vibImg.color = GameSettings.HapticsEnabled ? UiBuild.Selected : UiBuild.Disabled;
                if (_vibLabel != null) _vibLabel.text = GameSettings.HapticsEnabled ? "VIBRATION: ON" : "VIBRATION: OFF";
            }
        }
    }
}
