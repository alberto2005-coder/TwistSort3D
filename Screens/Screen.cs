using System;
using Microsoft.Xna.Framework;
using TwistSort3D.Core;
using TwistSort3D.Render;

namespace TwistSort3D.Screens
{
    public abstract class Screen
    {
        protected readonly TwistSortGame G;
        protected Screen(TwistSortGame g) => G = g;
        public virtual void OnEnter() { }
        public abstract void Update(float dt);
        public abstract void Draw(UiKit ui);
    }

    /// <summary>Pantalla de carga con el logo.</summary>
    public class SplashScreen : Screen
    {
        private float _t;

        public SplashScreen(TwistSortGame g) : base(g) { }

        public override void Update(float dt)
        {
            _t += dt;
            if (_t > 1.6f) G.GoTo(new MenuScreen(G));
        }

        public override void Draw(UiKit ui)
        {
            ui.Begin(G.UiTransform);
            ui.Gradient(new Rectangle(0, 0, Config.VirtualW, Config.VirtualH), Config.Brand, Config.Teal);

            var logo = new Rectangle(390, 620, 300, 300);
            ui.Shadow(logo, 72);
            ui.RoundRect(logo, 72, Color.White);
            ui.Circle(new Vector2(540, 770), 96, Config.Honey);
            ui.TextCentered(84, "3D", new Rectangle(440, 720, 200, 100), Config.Navy, true);

            ui.TextCentered(78, "TWIST & SORT", new Rectangle(0, 980, Config.VirtualW, 100), Color.White, true);
            ui.TextCentered(40, Loc.T("TAGLINE"), new Rectangle(0, 1080, Config.VirtualW, 60), Color.White * 0.85f);

            float a = (float)(0.5 + 0.5 * Math.Sin(_t * 4));
            ui.TextCentered(30, "cargando…  ·  loading…", new Rectangle(0, 1720, Config.VirtualW, 40), Color.White * a);

            ui.End();
        }
    }

    /// <summary>Menú principal.</summary>
    public class MenuScreen : Screen
    {
        public MenuScreen(TwistSortGame g) : base(g) { }

        public override void Update(float dt)
        {
            if (G.Input.EscapePressed) { /* nada: ya estamos en el menú */ }

            if (G.Input.EnterPressed) { G.StartAdventure(); return; }

            var k = G.Input.Keys;
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S)) G.GoTo(new SettingsScreen(G));
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.C)) G.GoTo(new SkinsScreen(G));
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.H)) G.GoTo(new HelpScreen(G));
        }

        public override void Draw(UiKit ui)
        {
            var save = SaveManager.Data;
            int biome = Config.Biome(save.CurrentLevel);

            ui.Begin(G.UiTransform);
            ui.Gradient(new Rectangle(0, 0, Config.VirtualW, Config.VirtualH),
                        Config.BiomeTop[biome], Config.BiomeBottom[biome]);

            // decoración
            ui.Circle(new Vector2(120, 240), 220, Color.White * 0.06f);
            ui.Circle(new Vector2(980, 500), 280, Color.White * 0.05f);
            ui.Circle(new Vector2(180, 1500), 320, Color.White * 0.04f);

            // logo
            var logo = new Rectangle(420, 130, 240, 240);
            ui.Shadow(logo, 60);
            ui.RoundRect(logo, 60, Color.White);
            ui.Circle(new Vector2(540, 250), 76, Config.Honey);
            ui.TextCentered(66, "3D", new Rectangle(460, 205, 160, 90), Config.Navy, true);

            // título
            ui.TextCentered(76, "TWIST & SORT 3D", new Rectangle(0, 400, Config.VirtualW, 90), Color.White, true);
            ui.TextCentered(36, Loc.T("TAGLINE"), new Rectangle(0, 485, Config.VirtualW, 50), Color.White * 0.85f);

            // monedas
            var coins = new Rectangle(745, 38, 305, 72);
            ui.RoundRect(coins, 36, new Color(0, 0, 0, 75));
            ui.Circle(new Vector2(coins.X + 38, coins.Y + 36), 21, Config.Honey);
            ui.TextRight(26, Loc.T("COINS", save.Coins), new Rectangle(coins.X + 34, coins.Y, 250, coins.Height), Color.White, true);

            // jugar
            var play = new Rectangle(190, 620, 700, 150);
            if (ui.Button(play, $"{Loc.T("MENU_PLAY")}   ·   {Loc.T("MENU_LEVEL", save.CurrentLevel)}", true, true, 44))
                G.StartAdventure();

            // diario / maratón
            var daily = new Rectangle(60, 820, 470, 130);
            var marathon = new Rectangle(550, 820, 470, 130);
            if (ui.Button(daily, Loc.T("MENU_DAILY"), false, true, 30)) G.GoTo(new DailyScreen(G));
            if (ui.Button(marathon, Loc.T("MENU_MARATHON"), false, true, 30)) G.GoTo(new MarathonScreen(G));

            // ajustes / skins / ayuda
            int w = 300;
            var settings = new Rectangle(60, 1000, w, 120);
            var skins = new Rectangle(390, 1000, w, 120);
            var help = new Rectangle(720, 1000, w, 120);
            if (ui.Button(settings, Loc.T("MENU_SETTINGS"), false, true, 27)) G.GoTo(new SettingsScreen(G));
            if (ui.Button(skins, Loc.T("MENU_SKINS"), false, true, 27)) G.GoTo(new SkinsScreen(G));
            if (ui.Button(help, Loc.T("MENU_HELP"), false, true, 27)) G.GoTo(new HelpScreen(G));

            // pie
            var best = new Rectangle(240, 1160, 600, 70);
            ui.RoundRect(best, 35, new Color(0, 0, 0, 85));
            ui.TextCentered(24, Loc.T("MENU_BEST", Math.Max(0, save.CurrentLevel - 1)),
                            best, Color.White);

            ui.TextCentered(17, $"{Config.GameName} v{Config.Version}  ·  PixelBolt Games",
                            new Rectangle(0, 1246, Config.VirtualW, 40), Color.White * 0.92f);

            // siluetas decorativas de tornillos
            Silhouette(ui, new Vector2(250, 1520), 105);
            Silhouette(ui, new Vector2(540, 1655), 78);
            Silhouette(ui, new Vector2(845, 1500), 95);

            if (G.ToastT > 0)
                G.DrawToast(ui);

            ui.End();
        }

        private static void Silhouette(UiKit ui, Vector2 c, float r)
        {
            ui.RoundRect(new Rectangle((int)(c.X - r * 0.26f), (int)(c.Y + r * 0.60f),
                                       (int)(r * 0.52f), (int)(r * 1.0f)),
                         (int)(r * 0.22f), Color.White * 0.05f);
            ui.Circle(c, r, Color.White * 0.055f);
            ui.Circle(c, r * 0.70f, new Color(255, 255, 255, 16));
            ui.RoundRect(new Rectangle((int)(c.X - r * 0.46f), (int)(c.Y - r * 0.085f),
                                       (int)(r * 0.92f), (int)(r * 0.17f)),
                         (int)(r * 0.085f), new Color(0, 0, 0, 16));
        }
    }
}
