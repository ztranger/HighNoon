using UnityEngine;
using UnityEngine.UI;

namespace HighNoon
{
    /// <summary>
    /// Shared overlay canvas: landscape 1920×1080, same reference as the duel HUD.
    /// Every menu / map / story / tutorial screen should use this so layouts stay in one space.
    /// </summary>
    public static class UiCanvas
    {
        public static readonly Vector2 Ref = new Vector2(1920f, 1080f);

        public static void Overlay(GameObject host, float matchWidthOrHeight = 0.5f)
        {
            // `??` skips AddComponent on Unity's fake-null GetComponent result.
            if (!host.TryGetComponent<Canvas>(out var canvas))
                canvas = host.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (!host.TryGetComponent<CanvasScaler>(out var scaler))
                scaler = host.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Ref;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
            if (!host.TryGetComponent<GraphicRaycaster>(out _))
                host.AddComponent<GraphicRaycaster>();
        }
    }
}
