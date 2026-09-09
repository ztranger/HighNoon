using System;
using UnityEngine;

namespace HighNoon
{
    public sealed class StatsScreen
    {
        public RectTransform Root { get; private set; }

        public void Build(RectTransform root, UiBuild ui, Action back)
        {
            Root = root;
            ui.Back(root, back);
            ui.Text(root, "Head", "RECORDS", 64, new Vector2(0.5f, 0.88f), Vector2.zero, 900, 90, UiBuild.Gold, FontStyle.Bold);

            Cell(ui, root, "FASTEST DRAW", Records.HasReaction ? $"{Records.BestReactionMs:0} ms" : "—", new Vector2(-420f, 80f));
            Cell(ui, root, "BEST AIM", Records.HasAccuracy ? $"{Records.BestAccuracy * 100f:0}%" : "—", new Vector2(420f, 80f));
            Cell(ui, root, "CAMPAIGNS WON", $"{Records.Completions}", new Vector2(-420f, -180f));
            Cell(ui, root, "FURTHEST", (Records.FurthestChapter > 0 || Records.FurthestStage > 0)
                ? $"CH {Records.FurthestChapter + 1}  ·  {Records.FurthestStage + 1}" : "—", new Vector2(420f, -180f));

            if (!Records.HasReaction && !Records.HasAccuracy && Records.Completions == 0)
                ui.Text(root, "Empty", "No records yet — go make history.", 32, new Vector2(0.5f, 0.12f), Vector2.zero, 900, 56, new Color(0.7f, 0.64f, 0.5f), FontStyle.Italic);
        }

        static void Cell(UiBuild ui, RectTransform p, string label, string value, Vector2 pos)
        {
            ui.Text(p, label + "L", label, 28, new Vector2(0.5f, 0.5f), pos + new Vector2(0f, 70f), 560, 48, new Color(0.72f, 0.66f, 0.52f), FontStyle.Normal);
            ui.Text(p, label + "V", value, 52, new Vector2(0.5f, 0.5f), pos, 560, 80, UiBuild.Gold, FontStyle.Bold);
        }
    }
}
