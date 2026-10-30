using System;
using Microsoft.Xna.Framework;
using TwistSort3D.Core;
using TwistSort3D.Render;

namespace TwistSort3D.Screens
{
    public class SkinsScreen : Screen
    {
        private static readonly (string id, string nameKey, int price, int level)[] ScrewItems =
        {
            ("standard", "SK_STANDARD", 0, 0),
            ("hex", "SK_HEX", 300, 8),
            ("round", "SK_ROUND", 450, 15)
        };

        private static readonly (string id, string nameKey, int price, int level)[] TrayItems =
        {
            ("wood", "SK_WOOD", 0, 0),
            ("metal", "SK_METAL", 250, 6),
            ("candy", "SK_CANDY", 400, 12)
        };

        private bool _trayTab;

        public SkinsScreen(TwistSortGame g) : base(g) { }

        public override void Update(float dt)
        {
            if (G.Input.EscapePressed) G.GoTo(new MenuScreen(G));
        }

        public override void Draw(UiKit ui)
        {
            ui.Begin(G.UiTransform);
            ui.Gradient(new Rectangle(0, 0, Config.VirtualW, Config.VirtualH),
                        new Color(250, 246, 255), new Color(214, 225, 245));

            ui.TextCentered(52, Loc.T("SK_TITLE"), new Rectangle(0, 45, Config.VirtualW, 80), Config.Navy, true);
            if (ui.Button(new Rectangle(30, 40, 130, 86), "<", false, true, 44)) GoBack();

            var coins = new Rectangle(860, 42, 190, 64);
            ui.RoundRect(coins, 32, new Color(0, 0, 0, 40));
            ui.Circle(new Vector2(coins.X + 32, coins.Y + 32), 18, Config.Honey);
            ui.TextRight(26, SaveManager.Data.Coins.ToString(),
                         new Rectangle(coins.X + 56, coins.Y, 116, coins.Height), Config.Navy, true);

            var screwsTab = new Rectangle(250, 160, 270, 84);
            var traysTab = new Rectangle(560, 160, 270, 84);
            if (ui.Button(screwsTab, Loc.T("SK_SCREWS"), !_trayTab, true, 26)) _trayTab = false;
            if (ui.Button(traysTab, Loc.T("SK_TRAYS"), _trayTab, true, 26)) _trayTab = true;

            var items = _trayTab ? TrayItems : ScrewItems;
            int best = Math.Max(1, SaveManager.Data.CurrentLevel - 1);

            for (int i = 0; i < items.Length; i++)
            {
                var it = items[i];
                int x = 40 + i * 350;
                var card = new Rectangle(x, 300, 300, 470);

                ui.Shadow(card, 32);
                ui.RoundRect(card, 32, Config.Cream);

                bool owned = _trayTab
                    ? SaveManager.Data.TraySkins.Contains(it.id)
                    : SaveManager.Data.ScrewSkins.Contains(it.id);
                bool active = _trayTab
                    ? SaveManager.Data.ActiveTraySkin == it.id
                    : SaveManager.Data.ActiveScrewSkin == it.id;

                // vista previa
                var preview = new Rectangle(card.X + 22, card.Y + 22, 256, 190);
                ui.RoundRect(preview, 24, new Color(236, 240, 246));
                DrawPreview(ui, preview, it.id, _trayTab);

                ui.TextCentered(30, Loc.T(it.nameKey), new Rectangle(card.X, card.Y + 230, card.Width, 44),
                                Config.Navy, true);

                var action = new Rectangle(card.X + 24, card.Y + 366, card.Width - 48, 76);

                if (active)
                {
                    ui.RoundRect(action, 26, Config.Teal);
                    ui.TextCentered(24, Loc.T("SK_EQUIPPED"), action, Color.White, true);
                }
                else if (owned)
                {
                    if (ui.Button(action, Loc.T("SK_EQUIP"), false, true, 24)) Equip(it.id);
                }
                else if (best < it.level)
                {
                    ui.Button(action, Loc.T("SK_LOCKED", it.level), false, false, 22);
                }
                else
                {
                    bool canBuy = SaveManager.Data.Coins >= it.price;
                    if (ui.Button(action, Loc.T("SK_BUY", it.price), true, canBuy, 22))
                        Buy(it.id, it.price);
                }

                if (active)
                {
                    int th = 7;
                    ui.RoundRect(new Rectangle(card.X - 4, card.Y - 4, card.Width + 8, th), 3, Config.Teal);
                    ui.RoundRect(new Rectangle(card.X - 4, card.Bottom - 3, card.Width + 8, th), 3, Config.Teal);
                    ui.RoundRect(new Rectangle(card.X - 4, card.Y - 4, th, card.Height + 8), 3, Config.Teal);
                    ui.RoundRect(new Rectangle(card.Right - 3, card.Y - 4, th, card.Height + 8), 3, Config.Teal);
                }
            }

            ui.TextCentered(22, "100% cosmético · nunca cambia la dificultad", new Rectangle(0, 880, Config.VirtualW, 30),
                            Config.TextGrey * 0.8f);

            if (G.ToastT > 0) G.DrawToast(ui);

            ui.End();
        }

        private void DrawPreview(UiKit ui, Rectangle preview, string id, bool tray)
        {
            if (!tray)
            {
                var c = Config.ScrewColors[0];
                var head = new Vector2(preview.Center.X, preview.Y + 84);
                float r = id == "hex" ? 64 : 58;

                // cuerpo metálico
                ui.RoundRect(new Rectangle((int)head.X - 26, (int)head.Y + 22, 52, 74), 16,
                             new Color(70, 76, 88));

                // cabeza
                if (id == "round") ui.Circle(head, r, c);
                else ui.Hex(head, r, c);

                // brillo
                ui.Circle(new Vector2(head.X - 20, head.Y - 22), 15, Color.White * 0.22f);

                if (id == "standard")
                    ui.RoundRect(new Rectangle((int)head.X - 30, (int)head.Y - 6, 60, 12), 6,
                                 Color.Black * 0.28f);
                else if (id == "hex")
                    ui.Hex(head, r * 0.45f, Color.Lerp(c, Color.White, 0.22f));
                else
                    ui.Circle(head, r * 0.38f, Color.Lerp(c, Color.White, 0.22f));
            }
            else
            {
                Color frame = id switch
                {
                    "metal" => new Color(150, 158, 170),
                    "candy" => new Color(255, 170, 200),
                    _ => new Color(150, 105, 65)
                };
                ui.RoundRect(new Rectangle(preview.X + 26, preview.Y + 44, preview.Width - 52, 102), 24, frame);
                for (int i = 0; i < 3; i++)
                {
                    var slot = new Rectangle(preview.X + 40 + i * 60, preview.Y + 60, 46, 70);
                    ui.RoundRect(slot, 14, new Color(255, 255, 255, 80));
                    ui.Circle(new Vector2(slot.Center.X, slot.Center.Y), 16, Config.ScrewColors[i]);
                }
            }
        }

        private void Equip(string id)
        {
            if (_trayTab) SaveManager.Data.ActiveTraySkin = id;
            else SaveManager.Data.ActiveScrewSkin = id;
            SaveManager.Save();
            AudioManager.Play("click");
        }

        private void Buy(string id, int price)
        {
            if (!SaveManager.SpendCoins(price)) return;
            if (_trayTab)
            {
                if (!SaveManager.Data.TraySkins.Contains(id)) SaveManager.Data.TraySkins.Add(id);
                SaveManager.Data.ActiveTraySkin = id;
            }
            else
            {
                if (!SaveManager.Data.ScrewSkins.Contains(id)) SaveManager.Data.ScrewSkins.Add(id);
                SaveManager.Data.ActiveScrewSkin = id;
            }
            SaveManager.Save();
            AudioManager.Play("coin");
            G.ShowToast(Loc.T("SK_EQUIPPED"));
        }

        private void GoBack() => G.GoTo(new MenuScreen(G));
    }
}
