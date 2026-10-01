// Implements: approved UI item U5 (2026-10-01) — the Three-Worlds upgrade menu: 3 kit tabs × 3 tier cards (name, effect, cost,
// bought/locked), points at the top. Opens from the pause menu and at huacas. Kuntur/Amaru tabs unlock at M3.6 (02-mission-list.md).
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class UpgradeMenuUI : UIElement
    {
        public static UpgradeMenuUI Instance { get; private set; }
        public bool IsOpen { get; private set; }

        static readonly KitId[] Kits = { KitId.Puma, KitId.Kuntur, KitId.Amaru };
        RectTransform panel;
        Text pointsText;
        readonly Image[] tabs = new Image[3];
        readonly Text[] tabLabels = new Text[3];
        readonly Image[] cards = new Image[3];
        readonly Text[] cardTitles = new Text[3];
        readonly Text[] cardBodies = new Text[3];
        readonly Button[] buyButtons = new Button[3];
        readonly Text[] buyLabels = new Text[3];
        KitId selected = KitId.Puma;

        protected override void OnBuild()
        {
            Instance = this;
            panel = UIFactory.Panel(Root, "Panel", new Color(0.03f, 0.03f, 0.05f, 0.94f)).rectTransform;
            panel.GetComponent<Image>().raycastTarget = true;
            UIFactory.Stretch(panel);

            var title = UIFactory.Label(panel, "Title", "THE THREE WORLDS", 34, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.5f));
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(900f, 50f));
            pointsText = UIFactory.Label(panel, "Points", "", 22, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Place(pointsText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(900f, 34f));

            for (int i = 0; i < 3; i++)
            {
                var kit = Kits[i];
                var tab = UIFactory.Button(panel, kit.ToString(), () => selected = kit, new Vector2(260f, 48f));
                UIFactory.Place((RectTransform)tab.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2((i - 1) * 280f, -175f), new Vector2(260f, 48f));
                tabs[i] = tab.GetComponent<Image>();
                tabLabels[i] = tab.GetComponentInChildren<Text>();

                int tier = i + 1;
                var card = UIFactory.Panel(panel, "Card" + tier, new Color(0.12f, 0.12f, 0.15f, 1f));
                UIFactory.Place(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 380f, -20f), new Vector2(360f, 380f));
                cards[i] = card;
                cardTitles[i] = UIFactory.Label(card.transform, "Title", "", 24, TextAnchor.UpperCenter, Color.white);
                UIFactory.Place(cardTitles[i].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(330f, 64f));
                cardBodies[i] = UIFactory.Label(card.transform, "Body", "", 17, TextAnchor.UpperLeft, new Color(0.85f, 0.85f, 0.85f));
                UIFactory.Place(cardBodies[i].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(320f, 190f));
                buyButtons[i] = UIFactory.Button(card.transform, "Buy", () => Buy(tier), new Vector2(300f, 50f));
                UIFactory.Place((RectTransform)buyButtons[i].transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(300f, 50f));
                buyLabels[i] = buyButtons[i].GetComponentInChildren<Text>();
            }

            var close = UIFactory.Button(panel, "Close (Esc)", () => Open(false), new Vector2(260f, 48f));
            UIFactory.Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(260f, 48f));
            panel.gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Open(bool value)
        {
            if (IsOpen == value) return;
            IsOpen = value;
            panel.gameObject.SetActive(value);
            Time.timeScale = value ? 0f : 1f;
            if (value) { GameInput.Instance?.PushMenu(); SelectControlledKit(); }
            else GameInput.Instance?.PopMenu();
        }

        void SelectControlledKit()
        {
            var c = Controlled;
            if (c != null && c.Kit != null && UpgradeEconomy.KitMenuUnlocked(c.Kit.Kit)) selected = c.Kit.Kit;
        }

        void Buy(int tier)
        {
            if (UpgradeEconomy.TryBuy(selected, tier))
            {
                AudioPool.Instance?.PlayCue(PlaceholderCue.Pickup, Vector3.zero, 0.4f);
                var mm = MissionManager.Instance;
                if (SaveSystem.Instance != null && mm != null && mm.Current != null) SaveSystem.Instance.Save(mm.Current.Id);
            }
        }

        void Update()
        {
            if (!IsOpen) return;
            var input = GameInput.Instance;
            if (UIEscape.TryConsume(input)) { Open(false); return; }

            pointsText.text = $"Upgrade points: {UpgradeEconomy.AvailablePoints()}   (earned {UpgradeEconomy.EarnedPoints()} · 1 per combat mission + 1 per relic)";
            for (int i = 0; i < 3; i++)
            {
                bool open = UpgradeEconomy.KitMenuUnlocked(Kits[i]);
                bool sel = Kits[i] == selected;
                tabs[i].color = sel ? new Color(0.45f, 0.35f, 0.15f, 1f) : new Color(0.15f, 0.15f, 0.18f, 0.95f);
                tabLabels[i].text = UpgradeCatalog.KitTitle(Kits[i]) + (open ? $"  ({Progression.GetTier(Kits[i])}/3)" : "  — from M3.6");
                tabLabels[i].color = open ? Color.white : new Color(0.55f, 0.55f, 0.55f);
            }

            for (int i = 0; i < 3; i++)
            {
                int tier = i + 1;
                var status = UpgradeEconomy.StatusOf(selected, tier);
                cardTitles[i].text = $"Tier {tier}\n{UpgradeCatalog.TierName(selected, tier)}";
                cardBodies[i].text = UpgradeCatalog.TierEffect(selected, tier).Replace(" · ", "\n• ").Insert(0, "• ");
                buyButtons[i].interactable = status == UpgradeStatus.Available;
                buyLabels[i].text = status == UpgradeStatus.Bought ? "Bought"
                    : status == UpgradeStatus.Available ? $"Buy ({UpgradeEconomy.CostPerTier} pt)"
                    : status == UpgradeStatus.NeedsPreviousTier ? "Needs previous tier"
                    : status == UpgradeStatus.NotEnoughPoints ? "Not enough points"
                    : "Unlocks at M3.6";
                cards[i].color = status == UpgradeStatus.Bought ? new Color(0.2f, 0.17f, 0.08f, 1f) : new Color(0.12f, 0.12f, 0.15f, 1f);
            }
        }
    }
}
