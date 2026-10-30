using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using TwistSort3D.Core;
using TwistSort3D.Game;
using TwistSort3D.Render;
using TwistSort3D.Screens;

namespace TwistSort3D
{
    public class InputState
    {
        public Vector2 Pointer;
        public bool Down;
        public bool Pressed;
        public bool Released;
        public bool EscapePressed;
        public bool EnterPressed;
        public KeyboardState Keys;
    }

    /// <summary>
    /// Bucle principal. Todo el juego corre sobre MonoGame (paquete NuGet),
    /// así que se abre, compila y ejecuta únicamente desde Visual Studio.
    /// </summary>
    public class TwistSortGame : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager _graphics;

        public UiKit Ui;
        public BoardRenderer Renderer;
        public Screen Current;
        public GameSession Session;

        public Matrix UiTransform = Matrix.Identity;
        public Rectangle View3D;

        public InputState Input = new();
        public string Toast = "";
        public float ToastT;

        private bool _prevDown;
        private bool _prevEsc;
        private bool _prevEnter;

        public bool ReduceMotion => SaveManager.Data.Settings.ReduceMotion;

        public TwistSortGame()
        {
            // adaptar la ventana al monitor real para que nunca quede recortada
            int screenW = GetSystemMetrics(0);
            int screenH = GetSystemMetrics(1);
            if (screenW < 320 || screenH < 480)
            {
                screenW = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                screenH = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }

            int h = Math.Min(Config.DefaultWindowH, Math.Max(420, screenH - 110));
            int w = (int)(h * 9.0 / 16.0);
            if (w > screenW - 60)
            {
                w = Math.Max(320, screenW - 60);
                h = (int)(w * 16.0 / 9.0);
            }

            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = w,
                PreferredBackBufferHeight = h,
                SynchronizeWithVerticalRetrace = true
            };

            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.Title = Config.GameName;
        }

        protected override void LoadContent()
        {
            SaveManager.Load();

            if (string.IsNullOrEmpty(SaveManager.Data.Language))
            {
                Loc.DetectFromSystem();
                SaveManager.Data.Language = Loc.Language;
                SaveManager.Save();
            }
            else
            {
                Loc.SetLanguage(SaveManager.Data.Language);
            }

            try { AudioManager.Initialize(); } catch { /* sin dispositivo de audio */ }
            AudioManager.ApplySettings(SaveManager.Data.Settings);

            Ui = new UiKit(GraphicsDevice);
            Renderer = new BoardRenderer(GraphicsDevice);

            GoTo(new SplashScreen(this));
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            dt = Math.Min(dt, 0.05f);

            UpdateLayoutAndInput();
            if (ToastT > 0f) ToastT -= dt;

            Current?.Update(dt);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Current?.Draw(Ui);
            base.Draw(gameTime);
        }

        // ------------------------------------------------------------------ layout
        private void UpdateLayoutAndInput()
        {
            // ajustar el backbuffer al tamaño real del área de cliente (evita recortes)
            var cb = Window.ClientBounds;
            var pp = GraphicsDevice.PresentationParameters;
            if (cb.Width > 0 && cb.Height > 0 &&
                (pp.BackBufferWidth != cb.Width || pp.BackBufferHeight != cb.Height))
            {
                _graphics.PreferredBackBufferWidth = cb.Width;
                _graphics.PreferredBackBufferHeight = cb.Height;
                _graphics.ApplyChanges();
                pp = GraphicsDevice.PresentationParameters;
            }

            float vw = Math.Max(1, pp.BackBufferWidth);
            float vh = Math.Max(1, pp.BackBufferHeight);

            float scale = MathF.Min(vw / Config.VirtualW, vh / Config.VirtualH);
            float ox = (vw - Config.VirtualW * scale) * 0.5f;
            float oy = (vh - Config.VirtualH * scale) * 0.5f;

            UiTransform = Matrix.CreateScale(scale) * Matrix.CreateTranslation(ox, oy, 0);

            View3D = new Rectangle(
                (int)ox, (int)oy,
                Math.Max(1, (int)(Config.VirtualW * scale)),
                Math.Max(1, (int)(1250f * scale)));

            var ms = Mouse.GetState();
            Vector2 raw = new Vector2(ms.X, ms.Y);
            bool down = ms.LeftButton == ButtonState.Pressed;
            bool pressed = down && !_prevDown;
            bool released = !down && _prevDown;
            _prevDown = down;

            var touch = TouchPanel.GetState();
            if (touch.Count > 0)
            {
                var t = touch[0];
                raw = t.Position;
                down = t.State == TouchLocationState.Pressed || t.State == TouchLocationState.Moved;
                if (t.State == TouchLocationState.Pressed) pressed = true;
                if (t.State == TouchLocationState.Released) released = true;
            }

            if (scale <= 0f) scale = 1f;
            var inv = Matrix.Invert(UiTransform);
            Input.Pointer = Vector2.Transform(raw, inv);
            Input.Down = down;
            Input.Pressed = pressed;
            Input.Released = released;
            Input.Keys = Keyboard.GetState();

            bool esc = Input.Keys.IsKeyDown(Keys.Escape);
            Input.EscapePressed = esc && !_prevEsc;
            _prevEsc = esc;

            bool enter = Input.Keys.IsKeyDown(Keys.Enter) || Input.Keys.IsKeyDown(Keys.Space);
            Input.EnterPressed = enter && !_prevEnter;
            _prevEnter = enter;

            Ui.Pointer = Input.Pointer;
            Ui.Down = Input.Down;
            Ui.Pressed = Input.Pressed;
            Ui.Released = Input.Released;
        }

        // ------------------------------------------------------------------ navegación
        public void GoTo(Screen screen)
        {
            Current = screen;
            screen.OnEnter();
        }

        public void StartLevel(int level, bool daily = false, bool marathon = false)
        {
            Session = new GameSession(level, daily, marathon);
            GoTo(new GameScreen(this, Session));
        }

        public void StartAdventure() => StartLevel(SaveManager.Data.CurrentLevel);

        public void StartMarathon() => StartLevel(SaveManager.Data.MarathonLevel, marathon: true);

        public void StartDaily() => StartLevel(200 + DateTime.Today.DayOfYear, daily: true);

        public void RestartLevel(GameSession s) => StartLevel(s.LevelNumber, s.Daily, s.Marathon);

        public void NextLevel(GameSession s)
        {
            if (s.Daily || s.Marathon) { GoTo(new MenuScreen(this)); return; }
            StartAdventure();
        }

        public void ShowToast(string msg)
        {
            Toast = msg;
            ToastT = 2.6f;
        }

        public void DrawToast(UiKit ui)
        {
            float a = Math.Min(1f, ToastT);
            var r = new Rectangle(120, 1180, 840, 72);
            ui.RoundRect(r, 30, new Color(0, 0, 0, (int)(190 * a)));
            ui.TextCentered(28, Toast, r, Color.White * a, true);
        }

        protected override void UnloadContent()
        {
            Ui?.Dispose();
            base.UnloadContent();
        }

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
    }
}
