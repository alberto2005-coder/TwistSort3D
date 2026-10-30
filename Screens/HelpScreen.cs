using System;
using Microsoft.Xna.Framework;
using TwistSort3D.Core;
using TwistSort3D.Render;

namespace TwistSort3D.Screens
{
    public class HelpScreen : Screen
    {
        public HelpScreen(TwistSortGame g) : base(g) { }

        public override void Update(float dt)
        {
            if (G.Input.EscapePressed) G.GoTo(new MenuScreen(G));
        }

        public override void Draw(UiKit ui)
        {
            ui.Begin(G.UiTransform);
            ui.Gradient(new Rectangle(0, 0, Config.VirtualW, Config.VirtualH),
                        new Color(250, 248, 240), new Color(220, 230, 245));

            ui.TextCentered(50, Loc.T("HELP_TITLE"), new Rectangle(0, 40, Config.VirtualW, 80), Config.Navy, true);
            if (ui.Button(new Rectangle(30, 36, 130, 84), "<", false, true, 44)) GoBack();

            int y = 160;
            for (int i = 1; i <= 4; i++)
            {
                ui.Text(31, Loc.T($"HELP_STEP{i}_T"), new Vector2(70, y), Config.Brand);
                string desc = ui.Wrap(24, Loc.T($"HELP_STEP{i}_D"), 900);
                ui.Text(24, desc, new Vector2(70, y + 42), Config.TextGrey);
                y += 118;
            }

            y += 10;
            ui.Text(32, Loc.T("HELP_TOOLS"), new Vector2(70, y), Config.Brand);
            y += 48;

            string[] toolKeys = { "TOOL_HAMMER", "TOOL_SWAP", "TOOL_EXPAND", "TOOL_HINT", "TOOL_FREEZE" };
            for (int i = 0; i < toolKeys.Length; i++)
            {
                ui.Text(25, $"• {Loc.T(toolKeys[i])}", new Vector2(80, y), Config.Navy);
                ui.Text(23, Loc.T(toolKeys[i] + "_D"), new Vector2(360, y + 2), Config.TextGrey);
                y += 46;
            }

            y += 14;
            ui.Text(32, Loc.T("HELP_OBSTACLES"), new Vector2(70, y), Config.Brand);
            y += 48;

            string[] obsKeys = { "OB_DOUBLE", "OB_WOOD", "OB_ICE", "OB_LINKED", "OB_LOCKED", "OB_SWIVEL", "OB_PAINT", "OB_BOMB" };
            foreach (var key in obsKeys)
            {
                ui.Text(24, $"• {Loc.T(key)}", new Vector2(80, y), Config.Navy);
                ui.Text(22, Loc.T(key + "_D"), new Vector2(360, y + 2), Config.TextGrey);
                y += 44;
            }

            ui.TextCentered(22, "Sin anuncios · sin compras · sin conexión", new Rectangle(0, 1800, Config.VirtualW, 30),
                            Config.TextGrey * 0.8f);

            ui.End();
        }

        private void GoBack() => G.GoTo(new MenuScreen(G));
    }

    public class DailyScreen : Screen
    {
        public DailyScreen(TwistSortGame g) : base(g) { }

        public override void Update(float dt)
        {
            if (G.Input.EscapePressed) G.GoTo(new MenuScreen(G));
        }

        public override void Draw(UiKit ui)
        {
            bool done = SaveManager.Data.LastDaily == DateTime.Today.ToString("yyyy-MM-dd");

            ui.Begin(G.UiTransform);
            ui.Gradient(new Rectangle(0, 0, Config.VirtualW, Config.VirtualH),
                        new Color(255, 236, 210), new Color(255, 171, 145));

            var panel = new Rectangle(100, 400, 880, 1000);
            ui.Shadow(panel, 44);
            ui.RoundRect(panel, 44, Config.Cream);

            ui.TextCentered(52, Loc.T("DAILY_TITLE"), new Rectangle(panel.X, panel.Y + 50, panel.Width, 80),
                            Config.Navy, true);
            ui.TextCentered(30, DateTime.Today.ToString("dd / MM / yyyy"),
                            new Rectangle(panel.X, panel.Y + 150, panel.Width, 40), Config.Coral, true);

            string desc = ui.Wrap(28, Loc.T("DAILY_DESC"), panel.Width - 120);
            ui.TextCentered(28, desc, new Rectangle(panel.X + 60, panel.Y + 230, panel.Width - 120, 130),
                            Config.TextGrey);

            if (done)
            {
                string msg = ui.Wrap(28, Loc.T("DAILY_DONE"), panel.Width - 120);
                ui.TextCentered(28, msg, new Rectangle(panel.X + 60, panel.Y + 420, panel.Width - 120, 120),
                                Config.Success);
            }

            var play = new Rectangle(panel.X + 90, panel.Y + 620, panel.Width - 180, 140);
            if (ui.Button(play, Loc.T("DAILY_PLAY"), true, !done, 38)) G.StartDaily();

            var back = new Rectangle(panel.X + 90, panel.Y + 800, panel.Width - 180, 110);
            if (ui.Button(back, Loc.T("BACK"), false, true, 30)) G.GoTo(new MenuScreen(G));

            ui.End();
        }
    }

    public class MarathonScreen : Screen
    {
        public MarathonScreen(TwistSortGame g) : base(g) { }

        public override void Update(float dt)
        {
            if (G.Input.EscapePressed) G.GoTo(new MenuScreen(G));
        }

        public override void Draw(UiKit ui)
        {
            ui.Begin(G.UiTransform);
            ui.Gradient(new Rectangle(0, 0, Config.VirtualW, Config.VirtualH),
                        new Color(210, 230, 255), new Color(120, 160, 235));

            var panel = new Rectangle(100, 400, 880, 1000);
            ui.Shadow(panel, 44);
            ui.RoundRect(panel, 44, Config.Cream);

            ui.TextCentered(52, Loc.T("MARATHON_TITLE"), new Rectangle(panel.X, panel.Y + 50, panel.Width, 80),
                            Config.Navy, true);

            string desc = ui.Wrap(28, Loc.T("MARATHON_DESC"), panel.Width - 120);
            ui.TextCentered(28, desc, new Rectangle(panel.X + 60, panel.Y + 200, panel.Width - 120, 160),
                            Config.TextGrey);

            ui.TextCentered(34, Loc.T("MARATHON_LEVEL", SaveManager.Data.MarathonLevel),
                            new Rectangle(panel.X, panel.Y + 400, panel.Width, 60), Config.Brand, true);

            var play = new Rectangle(panel.X + 90, panel.Y + 540, panel.Width - 180, 140);
            if (ui.Button(play, Loc.T("MARATHON_PLAY"), true, true, 38)) G.StartMarathon();

            var back = new Rectangle(panel.X + 90, panel.Y + 720, panel.Width - 180, 110);
            if (ui.Button(back, Loc.T("BACK"), false, true, 30)) G.GoTo(new MenuScreen(G));

            ui.End();
        }
    }
}
