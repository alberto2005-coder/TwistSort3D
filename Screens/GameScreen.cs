using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TwistSort3D.Core;
using TwistSort3D.Game;
using TwistSort3D.Render;

namespace TwistSort3D.Screens
{
    /// <summary>Pantalla de juego: tablero 3D, HUD, bandeja, herramientas y overlays.</summary>
    public class GameScreen : Screen
    {
        private readonly GameSession _s;
        private readonly float _targetY;

        private bool _paused;
        private bool _dragging;
        private Vector2 _dragStart;
        private float _yawStart, _pitchStart;
        private bool _moved;

        private bool _tutorial;
        private int _tutStep;
        private float _tutTime;

        private const float TrayTop = 1330f;
        private const float ToolsTop = 1580f;

        public GameScreen(TwistSortGame g, GameSession session) : base(g)
        {
            _s = session;
            int maxLayer = _s.Level.Plates.Count == 0 ? 0 : _s.Level.Plates.Max(p => p.Layer);
            _targetY = 0.75f + maxLayer * Board.LayerHeight * 0.55f;

            _tutorial = _s.LevelNumber == 1 && !_s.Daily && !_s.Marathon &&
                        !SaveManager.Data.TutorialDone;
        }

        // ------------------------------------------------------------------ update
        public override void Update(float dt)
        {
            _s.Update(dt);

            if (_tutorial && _s.Won) CloseTutorial();

            if (_tutorial && !_s.Won && !_s.Lost) UpdateTutorial(dt);

            if (G.Input.EscapePressed)
            {
                _paused = !_paused;
                return;
            }
            if (_paused) return;

            if (_s.Won || _s.Lost) return;

            var p = G.Input.Pointer;
            bool in3D = p.Y < 1240f;

            if (G.Input.Pressed && in3D)
            {
                _dragging = true;
                _dragStart = p;
                _yawStart = G.Renderer.Yaw;
                _pitchStart = G.Renderer.Pitch;
                _moved = false;
            }

            if (_dragging && G.Input.Down)
            {
                var d = p - _dragStart;
                if (d.Length() > 10f) _moved = true;
                G.Renderer.Yaw = _yawStart - d.X * 0.006f;
                G.Renderer.Pitch = MathHelper.Clamp(_pitchStart + d.Y * 0.004f, 0.20f, 1.10f);
            }

            if (G.Input.Released && _dragging)
            {
                _dragging = false;
                if (!_moved && in3D) TryPick(p);
            }

            // teclas de herramienta
            var k = G.Input.Keys;
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D1)) _s.UseTool("hammer");
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D2)) _s.UseTool("swap");
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D3)) _s.UseTool("expand");
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D4)) _s.UseTool("hint");
            if (k.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D5)) _s.UseTool("freeze");
        }

        private void TryPick(Vector2 virtualPos)
        {
            if (_tutorial && TutorialCard().Contains(new Point((int)virtualPos.X, (int)virtualPos.Y)))
                return;

            var vp = G.View3D;
            if (vp.Width <= 0 || vp.Height <= 0) return;

            float lx = virtualPos.X / Config.VirtualW * vp.Width;
            float ly = virtualPos.Y / 1250f * vp.Height;

            var ray = G.Renderer.GetRay(new Vector2(lx, ly), vp);
            ScrewState best = null;
            float bestD = float.MaxValue;

            foreach (var s in _s.Board.Screws)
            {
                if (s.Removed) continue;
                var sphere = new BoundingSphere(s.Position + new Vector3(0, 0.18f, 0), 0.40f);
                var d = ray.Intersects(sphere);
                if (d.HasValue && d.Value < bestD)
                {
                    bestD = d.Value;
                    best = s;
                }
            }

            if (best != null) _s.TryTake(best);
        }

        // ------------------------------------------------------------------ draw
        public override void Draw(UiKit ui)
        {
            int biome = _s.Level.Biome;

            // fondo
            ui.Begin(G.UiTransform);
            ui.Gradient(new Rectangle(0, 0, Config.VirtualW, Config.VirtualH),
                        Config.BiomeTop[biome], Config.BiomeBottom[biome]);
            ui.End();

            // 3D
            G.Renderer.UpdateCamera(G.View3D, _targetY);
            G.Renderer.Draw(_s.Board, SaveManager.Data.Settings.ReduceMotion, _s.Shake, G.View3D);

            // capa UI
            ui.Begin(G.UiTransform);

            DrawHud(ui, biome);
            DrawBadges(ui);
            DrawTray(ui);
            DrawTools(ui);
            DrawMessage(ui);

            if (_paused) DrawPause(ui);
            else if (_s.Won) DrawWin(ui);
            else if (_s.Lost) DrawLose(ui);
            else if (_tutorial) DrawTutorial(ui);

            if (G.ToastT > 0 && !_s.Won && !_s.Lost) G.DrawToast(ui);

            ui.End();
        }

        // ------------------------------------------------------------------ HUD
        private void DrawHud(UiKit ui, int biome)
        {
            // barra superior
            var bar = new Rectangle(0, 0, Config.VirtualW, 150);
            ui.Rect(bar, new Color(0, 0, 0, 55));

            var pause = new Rectangle(24, 28, 92, 92);
            if (ui.Button(pause, "II", true, true, 30)) _paused = true;

            string label = _s.Daily ? Loc.T("DAILY_TITLE")
                         : _s.Marathon ? Loc.T("MARATHON_LEVEL", _s.LevelNumber)
                         : Loc.T("HUD_LEVEL", _s.LevelNumber);
            ui.TextCentered(40, label, new Rectangle(130, 12, 640, 92), Color.White, true);

            ui.TextCentered(22, Loc.T("HUD_MOVES", _s.Moves),
                            new Rectangle(130, 98, 640, 40), Color.White * 0.92f);

            // monedas
            var coins = new Rectangle(894, 40, 162, 64);
            ui.RoundRect(coins, 32, new Color(0, 0, 0, 70));
            ui.Circle(new Vector2(coins.X + 32, coins.Y + 32), 18, Config.Honey);
            ui.TextCentered(26, SaveManager.Data.Coins.ToString(),
                            new Rectangle(coins.X + 54, coins.Y, 98, coins.Height), Color.White, true);
        }

        // ------------------------------------------------------------------ badges 3D
        private void DrawBadges(UiKit ui)
        {
            foreach (var s in _s.Board.Screws)
            {
                if (s.Removed) continue;

                string tag = s.Data.Obstacle switch
                {
                    ObstacleType.Double => "2",
                    ObstacleType.Ice => "I",
                    ObstacleType.Linked => "C",
                    ObstacleType.Locked => "L",
                    ObstacleType.Swivel => "S",
                    ObstacleType.Paint => "P",
                    ObstacleType.Bomb => "B",
                    _ => null
                };

                var world = s.Position + new Vector3(0, 0.55f, 0);
                var p = Project(world);
                if (p.X < -50 || p.X > Config.VirtualW + 50 || p.Y < -50 || p.Y > 1300) continue;

                if (tag != null)
                {
                    var color = s.Data.Obstacle == ObstacleType.Bomb ? Config.Danger
                              : s.Data.Obstacle == ObstacleType.Ice ? new Color(77, 171, 247)
                              : new Color(60, 60, 70);
                    var rect = new Rectangle((int)p.X - 18, (int)p.Y - 18, 36, 36);
                    ui.Circle(new Vector2(p.X, p.Y), 19, color);
                    ui.TextCentered(24, tag, rect, Color.White, true);
                }

                if (s.Data.Obstacle == ObstacleType.Bomb && s.Data.BombMoves <= 3)
                {
                    ui.TextCentered(26, s.Data.BombMoves.ToString(),
                                    new Rectangle((int)p.X - 30, (int)p.Y + 16, 60, 30), Config.Danger, true);
                }

                if (s.Data.Obstacle == ObstacleType.Ice && s.Data.IceRemaining > 0)
                {
                    ui.TextCentered(22, s.Data.IceRemaining.ToString(),
                                    new Rectangle((int)p.X - 30, (int)p.Y + 16, 60, 26), Color.White, true);
                }
            }

            if (_s.HintTimer > 0 && _s.HintScrew != null && !_s.HintScrew.Removed)
            {
                var p = Project(_s.HintScrew.Position + new Vector3(0, 0.3f, 0));
                float a = (float)(0.55 + 0.45 * Math.Sin(_s.HintTimer * 8));
                ui.Circle(new Vector2(p.X, p.Y), 46, Config.Honey * a);
                ui.Circle(new Vector2(p.X, p.Y), 34, Config.Honey * (a * 0.45f));
            }
        }

        private Vector2 Project(Vector3 world)
        {
            var vp = G.View3D;
            var px = G.Renderer.WorldToScreen(world, vp);
            return new Vector2(px.X / Math.Max(1, vp.Width) * Config.VirtualW,
                               px.Y / Math.Max(1, vp.Height) * 1250f);
        }

        // ------------------------------------------------------------------ bandeja
        private void DrawTray(UiKit ui)
        {
            var tray = _s.Tray;
            int cap = tray.Capacity;
            int slot = 112, gap = 10;
            int total = cap * slot + (cap - 1) * gap;
            int x0 = (Config.VirtualW - total) / 2;
            int y = (int)TrayTop;

            ui.TextCentered(26, Loc.T("HUD_TRAY"), new Rectangle(0, y - 46, Config.VirtualW, 34),
                            Color.White * 0.85f, true);

            // marco (según la skin de bandeja elegida)
            Color frameColor = SaveManager.Data.ActiveTraySkin switch
            {
                "metal" => new Color(126, 134, 148),
                "candy" => new Color(232, 126, 170),
                _ => new Color(126, 86, 54)
            };

            var frame = new Rectangle(x0 - 18, y - 12, total + 36, slot + 24);
            ui.RoundRect(frame, 30, Color.Lerp(frameColor, Color.Black, 0.18f));
            ui.RoundRect(new Rectangle(frame.X + 6, frame.Y + 6, frame.Width - 12, frame.Height - 12), 24,
                         Color.Lerp(frameColor, Color.White, 0.15f + 0.45f * _s.TrayFlash));

            for (int i = 0; i < cap; i++)
            {
                var r = new Rectangle(x0 + i * (slot + gap), y, slot, slot);
                ui.RoundRect(r, 20, new Color(255, 255, 255, 70));
                ui.RoundRect(new Rectangle(r.X + 6, r.Y + 6, slot - 12, slot - 12), 16,
                             new Color(0, 0, 0, 28));

                int c = tray.Slots[i];
                if (c >= 0)
                {
                    var col = Config.ScrewColors[c % Config.ScrewColors.Length];
                    ui.Circle(new Vector2(r.Center.X, r.Center.Y), 42, col);
                    if (SaveManager.Data.Settings.Symbols)
                        ui.TextCentered(30, (c + 1).ToString(),
                                       new Rectangle(r.X, r.Y, slot, slot), Color.White, true);
                }
            }

            // resaltar parejas a punto de trío
            var g2 = tray.Slots.Where(c => c >= 0).GroupBy(c => c).Where(g => g.Count() == 2).Select(g => g.Key).ToList();
            if (g2.Count > 0)
            {
                for (int i = 0; i < cap; i++)
                {
                    if (tray.Slots[i] >= 0 && g2.Contains(tray.Slots[i]))
                    {
                        var r = new Rectangle(x0 + i * (slot + gap), y, slot, slot);
                        ui.RoundRect(r, 20, Color.White * 0.12f);
                    }
                }
            }
        }

        // ------------------------------------------------------------------ herramientas
        private void DrawTools(UiKit ui)
        {
            string[] ids = GameSession.ToolIds;
            string[] labels =
            {
                Loc.T("TOOL_HAMMER"), Loc.T("TOOL_SWAP"), Loc.T("TOOL_EXPAND"),
                Loc.T("TOOL_HINT"), Loc.T("TOOL_FREEZE")
            };
            int w = 190, h = 150, gap = 12;
            int total = ids.Length * w + (ids.Length - 1) * gap;
            int x0 = (Config.VirtualW - total) / 2;

            for (int i = 0; i < ids.Length; i++)
            {
                var r = new Rectangle(x0 + i * (w + gap), (int)ToolsTop, w, h);
                int count = SaveManager.GetTool(ids[i]);
                bool enabled = count > 0 && !_s.Won && !_s.Lost;

                if (ui.Button(r, "", false, enabled, 24))
                    _s.UseTool(ids[i]);

                ui.TextCentered(17, labels[i], new Rectangle(r.X, r.Y + 62, r.Width, 54),
                                enabled ? Config.Navy : new Color(120, 128, 134), true);

                ui.Circle(new Vector2(r.Right - 32, r.Y + 30), 21,
                          count > 0 ? Config.Brand : Config.Disabled);
                ui.TextCentered(20, count.ToString(),
                                new Rectangle(r.Right - 54, r.Y + 9, 44, 42), Color.White, true);
            }
        }

        // ------------------------------------------------------------------ mensajes
        private void DrawMessage(UiKit ui)
        {
            if (_s.MessageTimer <= 0 || string.IsNullOrEmpty(_s.Message)) return;
            float a = Math.Min(1f, _s.MessageTimer);
            var r = new Rectangle(140, 1240, 800, 70);
            ui.RoundRect(r, 28, new Color(0, 0, 0, (int)(170 * a)));
            ui.TextCentered(30, _s.Message, r, Color.White * a, true);
        }

        // ------------------------------------------------------------------ overlays
        private void DrawPause(UiKit ui)
        {
            ui.Dim(0.6f);
            var panel = new Rectangle(120, 430, 840, 1050);
            ui.Shadow(panel, 44);
            ui.RoundRect(panel, 44, Config.Cream);

            ui.TextCentered(64, Loc.T("HUD_PAUSE"), new Rectangle(panel.X, panel.Y + 40, panel.Width, 90),
                            Config.Navy, true);

            var resume = new Rectangle(panel.X + 90, panel.Y + 170, panel.Width - 180, 130);
            if (ui.Button(resume, Loc.T("HUD_RESUME"), true, true, 38)) _paused = false;

            var restart = new Rectangle(panel.X + 90, panel.Y + 330, panel.Width - 180, 130);
            if (ui.Button(restart, Loc.T("HUD_RESTART"), false, true, 32))
                G.RestartLevel(_s);

            var menu = new Rectangle(panel.X + 90, panel.Y + 490, panel.Width - 180, 130);
            if (ui.Button(menu, Loc.T("HUD_MENU"), false, true, 32)) G.GoTo(new MenuScreen(G));

            // sliders
            var s1 = SaveManager.Data.Settings;
            ui.Text(30, Loc.T("SET_MUSIC"), new Vector2(panel.X + 90, panel.Y + 670), Config.TextGrey);
            float mv = s1.Music;
            if (ui.Slider(new Rectangle(panel.X + 90, panel.Y + 720, panel.Width - 180, 22), ref mv))
            {
                s1.Music = mv;
                AudioManager.ApplySettings(s1);
            }

            ui.Text(30, Loc.T("SET_SFX"), new Vector2(panel.X + 90, panel.Y + 790), Config.TextGrey);
            float sv = s1.Sfx;
            if (ui.Slider(new Rectangle(panel.X + 90, panel.Y + 840, panel.Width - 180, 22), ref sv))
            {
                s1.Sfx = sv;
                AudioManager.ApplySettings(s1);
            }

            if (G.Input.Released) SaveManager.Save();
        }

        private void DrawWin(UiKit ui)
        {
            ui.Dim(0.55f);
            var panel = new Rectangle(90, 480, 900, 960);
            ui.Shadow(panel, 44);
            ui.RoundRect(panel, 44, Config.Cream);

            ui.TextCentered(58, Loc.T("WIN_TITLE", _s.LevelNumber),
                            new Rectangle(panel.X, panel.Y + 40, panel.Width, 80), Config.Navy, true);

            for (int i = 0; i < 3; i++)
            {
                float cx = 540 + (i - 1) * 150;
                bool filled = i < _s.Stars;
                float pop = Math.Clamp((_s.EndTimer - i * 0.22f) * 4f, 0f, 1f);
                ui.Star(new Vector2(cx, panel.Y + 200), 55 * (0.7f + 0.3f * pop),
                        filled ? Config.Honey : new Color(205, 210, 216));
            }

            ui.TextCentered(32, Loc.T("WIN_MOVES", _s.Moves, _s.Level.OptimalMoves),
                            new Rectangle(panel.X, panel.Y + 290, panel.Width, 50), Config.TextGrey);
            ui.TextCentered(32, Loc.T("WIN_POINTS", _s.Score),
                            new Rectangle(panel.X, panel.Y + 350, panel.Width, 50), Config.TextGrey);
            ui.TextCentered(32, Loc.T("WIN_REWARD", _s.CoinsEarned),
                            new Rectangle(panel.X, panel.Y + 410, panel.Width, 50), Config.Success, true);

            var next = new Rectangle(panel.X + 90, panel.Y + 540, panel.Width - 180, 140);
            if (ui.Button(next, Loc.T("WIN_NEXT"), true, true, 40))
                G.NextLevel(_s);

            var menu = new Rectangle(panel.X + 90, panel.Y + 710, panel.Width - 180, 130);
            if (ui.Button(menu, Loc.T("WIN_MENU"), false, true, 32))
                G.GoTo(new MenuScreen(G));
        }

        private void DrawLose(UiKit ui)
        {
            ui.Dim(0.55f);
            var panel = new Rectangle(90, 560, 900, 800);
            ui.Shadow(panel, 44);
            ui.RoundRect(panel, 44, Config.Cream);

            ui.TextCentered(50, Loc.T("LOSE_TITLE"), new Rectangle(panel.X, panel.Y + 50, panel.Width, 80),
                            Config.Danger, true);
            ui.TextCentered(30, Loc.T(_s.LoseReasonKey), new Rectangle(panel.X + 60, panel.Y + 150, panel.Width - 120, 50),
                            Config.TextGrey);

            string tip = TipForLevel();
            ui.TextCentered(26, Loc.T("LOSE_TIP", tip),
                            new Rectangle(panel.X + 60, panel.Y + 240, panel.Width - 120, 120), Config.TextGrey);

            var retry = new Rectangle(panel.X + 90, panel.Y + 400, panel.Width - 180, 140);
            if (ui.Button(retry, Loc.T("LOSE_RETRY"), true, true, 40))
                G.RestartLevel(_s);

            var menu = new Rectangle(panel.X + 90, panel.Y + 570, panel.Width - 180, 130);
            if (ui.Button(menu, Loc.T("WIN_MENU"), false, true, 32))
                G.GoTo(new MenuScreen(G));
        }

        private string TipForLevel()
        {
            return _s.Level.Obstacle switch
            {
                ObstacleType.Double => Loc.T("OB_DOUBLE_D"),
                ObstacleType.Ice => Loc.T("OB_ICE_D"),
                ObstacleType.Linked => Loc.T("OB_LINKED_D"),
                ObstacleType.Locked => Loc.T("OB_LOCKED_D"),
                ObstacleType.Bomb => Loc.T("OB_BOMB_D"),
                _ => Loc.T("HELP_STEP2_D")
            };
        }

        // ------------------------------------------------------------------ tutorial
        private void UpdateTutorial(float dt)
        {
            _tutTime += dt;

            // alternativa al botón: Intro/Espacio avanza el tutorial
            if (G.Input.EnterPressed)
            {
                if (_tutStep >= 4) { CloseTutorial(); return; }
                _tutStep++;
                return;
            }

            switch (_tutStep)
            {
                case 0:
                    if (_s.Moves >= 1) _tutStep = 1;
                    break;
                case 1:
                    if (_s.TriplesPopped >= 1) _tutStep = 2;
                    break;
                case 2:
                    if (_s.Board.Plates.Any(p => p.Falling || p.Fallen)) _tutStep = 3;
                    break;
            }
        }

        private Rectangle TutorialCard() => new Rectangle(60, 930, 960, 310);

        private void DrawTutorial(UiKit ui)
        {
            Vector2? target = null;
            switch (_tutStep)
            {
                case 0:
                    var screw = _s.Board.Playable.FirstOrDefault();
                    if (screw != null)
                        target = Project(screw.Position + new Vector3(0, 0.45f, 0));
                    break;

                case 1:
                    target = new Vector2(540, TrayTop + 55);
                    break;

                case 2:
                    var plate = _s.Board.Plates.FirstOrDefault(p => p.Fallen || p.Falling)
                                ?? _s.Board.Plates.FirstOrDefault();
                    if (plate != null)
                        target = Project(plate.Origin + new Vector3(0, 0.28f, 0));
                    break;

                case 3:
                    target = new Vector2(540, ToolsTop + 70);
                    break;
            }

            if (target.HasValue) DrawTutorialTarget(ui, target.Value);

            string title, body;
            switch (_tutStep)
            {
                case 0: title = Loc.T("TUT_WELCOME_T"); body = Loc.T("TUT_WELCOME"); break;
                case 1: title = Loc.T("TUT_TRAY_T"); body = Loc.T("TUT_TRAY"); break;
                case 2: title = Loc.T("TUT_PLATE_T"); body = Loc.T("TUT_PLATE"); break;
                case 3: title = Loc.T("TUT_TOOLS_T"); body = Loc.T("TUT_TOOLS"); break;
                default: title = Loc.T("TUT_DONE_T"); body = Loc.T("TUT_DONE"); break;
            }

            var card = TutorialCard();
            ui.Shadow(card, 34);
            ui.RoundRect(card, 34, new Color(255, 251, 240, 252));

            ui.Title(34, title, new Vector2(card.X + 36, card.Y + 12), Config.BrandDark);

            // puntos de progreso
            for (int i = 0; i < 5; i++)
            {
                var dot = new Vector2(card.X + 42 + i * 30, card.Y + 82);
                ui.Circle(dot, i == _tutStep ? 9 : 6,
                          i == _tutStep ? Config.Brand : Config.Grey);
            }

            string wrapped = ui.Wrap(23, body, card.Width - 380);
            ui.Text(23, wrapped, new Vector2(card.X + 36, card.Y + 116), Config.Navy);

            string label = _tutStep == 3 ? Loc.T("TUT_OK")
                         : _tutStep >= 4 ? Loc.T("TUT_PLAY")
                         : Loc.T("TUT_NEXT");

            var action = new Rectangle(card.Right - 316, card.Bottom - 104, 272, 76);
            if (ui.Button(action, label, true, true, 24))
            {
                if (_tutStep >= 4) CloseTutorial();
                else _tutStep++;
            }

            var skip = new Rectangle(card.Right - 186, card.Y + 12, 158, 48);
            if (ui.Button(skip, Loc.T("TUT_SKIP"), false, true, 17))
                CloseTutorial();
        }

        private void DrawTutorialTarget(UiKit ui, Vector2 t)
        {
            float pulse = 0.5f + 0.5f * MathF.Sin(_tutTime * 5f);

            // anillo de puntos giratorio alrededor del objetivo
            int dots = 12;
            for (int i = 0; i < dots; i++)
            {
                float ang = _tutTime * 0.9f + i * MathF.Tau / dots;
                float rad = 62 + 5 * pulse;
                var p = new Vector2(t.X + MathF.Cos(ang) * rad, t.Y + MathF.Sin(ang) * rad * 0.55f);
                ui.Circle(p, 7, Config.Honey * 0.95f);
            }

            // rastro de puntos que baja hacia el objetivo
            for (int i = 0; i < 3; i++)
            {
                float k = (_tutTime * 1.1f + i / 3f) % 1f;
                float y = t.Y - 190 + k * 130;
                ui.Circle(new Vector2(t.X, y), 9, Config.Honey * (1f - k));
            }
        }

        private void CloseTutorial()
        {
            _tutorial = false;
            SaveManager.Data.TutorialDone = true;
            SaveManager.Save();
        }
    }
}
