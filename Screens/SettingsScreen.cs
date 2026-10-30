using System;
using Microsoft.Xna.Framework;
using TwistSort3D.Core;
using TwistSort3D.Render;

namespace TwistSort3D.Screens
{
    public class SettingsScreen : Screen
    {
        private bool _confirmReset;
        private float _resetTimer;

        public SettingsScreen(TwistSortGame g) : base(g) { }

        public override void Update(float dt)
        {
            if (_confirmReset)
            {
                _resetTimer -= dt;
                if (_resetTimer <= 0f) _confirmReset = false;
            }
            if (G.Input.EscapePressed) G.GoTo(new MenuScreen(G));
        }

        public override void Draw(UiKit ui)
        {
            var s = SaveManager.Data.Settings;

            ui.Begin(G.UiTransform);
            ui.Gradient(new Rectangle(0, 0, Config.VirtualW, Config.VirtualH),
                        new Color(247, 249, 251), new Color(214, 225, 245));

            ui.TextCentered(56, Loc.T("SET_TITLE"), new Rectangle(0, 50, Config.VirtualW, 90), Config.Navy, true);
            if (ui.Button(new Rectangle(30, 45, 130, 90), "<", false, true, 44)) GoBack();

            int y = 200;
            ui.Text(34, Loc.T("SET_LANGUAGE"), new Vector2(80, y + 18), Config.Navy);
            if (ui.Button(new Rectangle(600, y, 190, 80), "ESPAÑOL", Loc.Language == "es", true, 26)) SetLang("es");
            if (ui.Button(new Rectangle(810, y, 190, 80), "ENGLISH", Loc.Language == "en", true, 26)) SetLang("en");

            y = 330;
            ui.Text(34, Loc.T("SET_MUSIC"), new Vector2(80, y + 8), Config.Navy);
            float mv = s.Music;
            if (ui.Slider(new Rectangle(470, y + 16, 520, 24), ref mv))
            {
                s.Music = mv;
                AudioManager.ApplySettings(s);
            }
            ui.Text(26, $"{(int)(s.Music * 100)}%", new Vector2(1005, y), Config.TextGrey);

            y = 450;
            ui.Text(34, Loc.T("SET_SFX"), new Vector2(80, y + 8), Config.Navy);
            float sv = s.Sfx;
            if (ui.Slider(new Rectangle(470, y + 16, 520, 24), ref sv))
            {
                s.Sfx = sv;
                AudioManager.ApplySettings(s);
                AudioManager.Play("click");
            }
            ui.Text(26, $"{(int)(s.Sfx * 100)}%", new Vector2(1005, y), Config.TextGrey);

            y = 580;
            ui.Text(34, Loc.T("SET_VIBRATION"), new Vector2(80, y + 8), Config.Navy);
            bool nv = ui.Toggle(new Rectangle(860, y, 130, 64), s.Vibration, out bool ch);
            if (ch) { s.Vibration = nv; SaveManager.Save(); }

            y = 700;
            ui.Text(34, Loc.T("SET_SYMBOLS"), new Vector2(80, y + 8), Config.Navy);
            nv = ui.Toggle(new Rectangle(860, y, 130, 64), s.Symbols, out ch);
            if (ch) { s.Symbols = nv; SaveManager.Save(); }

            y = 820;
            ui.Text(34, Loc.T("SET_MOTION"), new Vector2(80, y + 8), Config.Navy);
            nv = ui.Toggle(new Rectangle(860, y, 130, 64), s.ReduceMotion, out ch);
            if (ch) { s.ReduceMotion = nv; SaveManager.Save(); }

            y = 940;
            ui.Text(34, Loc.T("SET_QUALITY"), new Vector2(80, y + 8), Config.Navy);
            if (ui.Button(new Rectangle(470, y, 170, 78), Loc.T("SET_Q_LOW"), s.Quality == "baja", true, 24))
                SetQuality("baja");
            if (ui.Button(new Rectangle(655, y, 170, 78), Loc.T("SET_Q_MED"), s.Quality == "media", true, 24))
                SetQuality("media");
            if (ui.Button(new Rectangle(840, y, 170, 78), Loc.T("SET_Q_HIGH"), s.Quality == "alta", true, 24))
                SetQuality("alta");

            ui.TextCentered(26, Loc.T("SET_STATS", SaveManager.Data.LevelsWon, SaveManager.Data.LevelsLost,
                                      SaveManager.Data.ScrewsRemoved),
                            new Rectangle(60, 1080, Config.VirtualW - 120, 40), Config.TextGrey);
            ui.TextCentered(26, Loc.T("COINS", SaveManager.Data.Coins),
                            new Rectangle(60, 1130, Config.VirtualW - 120, 40), Config.TextGrey);

            var reset = new Rectangle(340, 1220, 400, 84);
            if (ui.Button(reset, _confirmReset ? Loc.T("SET_RESET_Q") : Loc.T("SET_RESET"),
                          false, true, _confirmReset ? 18 : 26))
            {
                if (!_confirmReset)
                {
                    _confirmReset = true;
                    _resetTimer = 4f;
                    AudioManager.Play("error");
                }
                else
                {
                    ResetAll();
                    _confirmReset = false;
                }
            }

            string credits = ui.Wrap(22, Loc.T("SET_CREDITS"), 900);
            ui.TextCentered(22, credits, new Rectangle(90, 1360, 900, 120), Config.TextGrey);
            ui.TextCentered(20, "OpenGL / MonoGame · .NET 8 · 100% Visual Studio",
                            new Rectangle(0, 1520, Config.VirtualW, 30), Config.TextGrey * 0.8f);

            if (G.ToastT > 0) G.DrawToast(ui);

            ui.End();
        }

        private void SetLang(string lang)
        {
            Loc.SetLanguage(lang);
            SaveManager.Data.Language = lang;
            SaveManager.Save();
        }

        private void SetQuality(string q)
        {
            SaveManager.Data.Settings.Quality = q;
            SaveManager.Save();
        }

        private void ResetAll()
        {
            string lang = Loc.Language;
            SaveManager.Reset();
            SaveManager.Data.Language = lang;
            Loc.SetLanguage(lang);
            SaveManager.Save();
            AudioManager.ApplySettings(SaveManager.Data.Settings);
            G.ShowToast(Loc.T("SET_RESET_DONE"));
        }

        private void GoBack() => G.GoTo(new MenuScreen(G));
    }
}
