using System;
using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TwistSort3D.Core;

namespace TwistSort3D.Render
{
    /// <summary>UI inmediata: rectángulos redondeados, botones, sliders, textos.</summary>
    public class UiKit : IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly SpriteBatch _sb;
        private readonly Texture2D _pixel;
        private readonly Texture2D _corner;
        private readonly Texture2D _star;
        private readonly Texture2D _hex;
        private readonly FontSystem _body;
        private readonly FontSystem _display;

        /// <summary>
        /// Los textos se diseñan sobre un lienzo de 1080x1920 que luego se reduce
        /// a la ventana real. Este factor compensa esa reducción para que el texto
        /// se lea con un tamaño cómodo en pantalla.
        /// </summary>
        public const float FontScale = 1.38f;

        public Vector2 Pointer;
        public bool Down, Pressed, Released;

        public UiKit(GraphicsDevice gd)
        {
            _gd = gd;
            _sb = new SpriteBatch(gd);

            _pixel = new Texture2D(gd, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _corner = MakeCorner(gd, 32);
            _star = MakeStar(gd, 64);
            _hex = MakeHex(gd, 128);

            _body = LoadFont(new[] { "Nunito-SemiBold.ttf", "Nunito.ttf", "Nunito-Regular.ttf" }, false);
            _display = LoadFont(new[] { "LilitaOne-Regular.ttf", "Fredoka.ttf" }, true);
        }

        // ------------------------------------------------------------- fuentes
        private static FontSystem LoadFont(string[] files, bool boldFallback)
        {
            var fs = new FontSystem();
            foreach (var file in files)
            {
                string custom = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", file);
                if (!File.Exists(custom)) continue;
                try
                {
                    fs.AddFont(File.ReadAllBytes(custom));
                    return fs;
                }
                catch { /* fuente no soportada: probar la siguiente */ }
            }

            string winFonts = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
            string[] candidates = boldFallback
                ? new[] { "segoeuib.ttf", "arialbd.ttf", "tahomabd.ttf", "segoeui.ttf", "arial.ttf" }
                : new[] { "segoeui.ttf", "arial.ttf", "tahoma.ttf", "calibri.ttf" };

            foreach (var c in candidates)
            {
                string p = Path.Combine(winFonts, c);
                if (!File.Exists(p)) continue;
                try
                {
                    fs.AddFont(File.ReadAllBytes(p));
                    return fs;
                }
                catch { }
            }
            return fs;
        }

        // ------------------------------------------------------------- ciclo
        public void Begin(Matrix transform)
        {
            _gd.Viewport = new Viewport(0, 0,
                _gd.PresentationParameters.BackBufferWidth,
                _gd.PresentationParameters.BackBufferHeight);
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                      DepthStencilState.None, RasterizerState.CullNone, null, transform);
        }

        public void End() => _sb.End();

        // ------------------------------------------------------------- primitivas
        public void Rect(Rectangle r, Color c)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            _sb.Draw(_pixel, r, c);
        }

        public void RoundRect(Rectangle r, int radius, Color c)
        {
            int rad = Math.Max(0, Math.Min(radius, Math.Min(r.Width, r.Height) / 2));
            if (rad <= 0) { Rect(r, c); return; }

            Rect(new Rectangle(r.X + rad, r.Y, r.Width - rad * 2, r.Height), c);
            Rect(new Rectangle(r.X, r.Y + rad, rad, r.Height - rad * 2), c);
            Rect(new Rectangle(r.Right - rad, r.Y + rad, rad, r.Height - rad * 2), c);

            _sb.Draw(_corner, new Rectangle(r.X, r.Y, rad, rad), null, c, 0, Vector2.Zero, SpriteEffects.None, 0);
            _sb.Draw(_corner, new Rectangle(r.Right - rad, r.Y, rad, rad), null, c, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            _sb.Draw(_corner, new Rectangle(r.X, r.Bottom - rad, rad, rad), null, c, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
            _sb.Draw(_corner, new Rectangle(r.Right - rad, r.Bottom - rad, rad, rad), null, c, 0, Vector2.Zero,
                     SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0);
        }

        public void Circle(Vector2 center, float radius, Color c)
        {
            RoundRect(new Rectangle((int)(center.X - radius), (int)(center.Y - radius),
                                    (int)(radius * 2), (int)(radius * 2)), (int)radius, c);
        }

        public void Shadow(Rectangle r, int radius)
        {
            RoundRect(new Rectangle(r.X, r.Y + 10, r.Width, r.Height), radius, new Color(0, 0, 0, 40));
        }

        public void Gradient(Rectangle r, Color top, Color bottom, int steps = 32)
        {
            int h = (int)MathF.Ceiling(r.Height / (float)steps);
            for (int i = 0; i < steps; i++)
            {
                float t = steps == 1 ? 0f : i / (float)(steps - 1);
                Rect(new Rectangle(r.X, r.Y + i * h, r.Width, h + 1), Color.Lerp(top, bottom, t));
            }
        }

        public void Star(Vector2 center, float radius, Color c)
        {
            var dst = new Rectangle((int)(center.X - radius), (int)(center.Y - radius),
                                    (int)(radius * 2), (int)(radius * 2));
            _sb.Draw(_star, dst, c);
        }

        /// <summary>Hexágono (cabeza de tornillo) centrado en <paramref name="center"/>.</summary>
        public void Hex(Vector2 center, float radius, Color c)
        {
            var dst = new Rectangle((int)(center.X - radius), (int)(center.Y - radius),
                                    (int)(radius * 2), (int)(radius * 2));
            _sb.Draw(_hex, dst, c);
        }

        public void Dim(float alpha = 0.55f)
        {
            Rect(new Rectangle(0, 0, Config.VirtualW, Config.VirtualH), new Color(0, 0, 0, (int)(255 * alpha)));
        }

        // ------------------------------------------------------------- texto
        public Vector2 Text(float size, string text, Vector2 pos, Color color)
        {
            var f = _body.GetFont(size * FontScale);
            _sb.DrawString(f, text, pos, color);
            return f.MeasureString(text);
        }

        public Vector2 Title(float size, string text, Vector2 pos, Color color)
        {
            var f = _display.GetFont(size * FontScale);
            _sb.DrawString(f, text, pos, color);
            return f.MeasureString(text);
        }

        public void TextCentered(float size, string text, Rectangle r, Color color, bool title = false)
        {
            var f = (title ? _display : _body).GetFont(size * FontScale);
            var m = f.MeasureString(text);
            _sb.DrawString(f, text, new Vector2(r.X + (r.Width - m.X) / 2f, r.Y + (r.Height - m.Y) / 2f), color);
        }

        public void TextRight(float size, string text, Rectangle r, Color color, bool title = false)
        {
            var f = (title ? _display : _body).GetFont(size * FontScale);
            var m = f.MeasureString(text);
            _sb.DrawString(f, text, new Vector2(r.Right - m.X, r.Y + (r.Height - m.Y) / 2f), color);
        }

        public string Wrap(float size, string text, float width)
        {
            var font = _body.GetFont(size * FontScale);
            var words = text.Split(' ');
            var sb = new System.Text.StringBuilder();
            string line = "";
            foreach (var w in words)
            {
                string test = line.Length == 0 ? w : line + " " + w;
                if (font.MeasureString(test).X > width && line.Length > 0)
                {
                    sb.AppendLine(line);
                    line = w;
                }
                else line = test;
            }
            sb.Append(line);
            return sb.ToString();
        }

        // ------------------------------------------------------------- controles
        public bool Button(Rectangle r, string label, bool primary = true, bool enabled = true,
                           float fontSize = 36f, bool titleFont = true)
        {
            bool hover = enabled && r.Contains(Pointer);
            bool press = enabled && Down && hover;
            bool click = enabled && Pressed && hover;
            int off = press ? 3 : 0;

            RoundRect(new Rectangle(r.X, r.Y + 8, r.Width, r.Height), 26, new Color(0, 0, 0, 42));

            Color bg = primary ? Config.Brand : Config.Grey;
            if (!enabled) bg = Config.Disabled;
            else if (hover) bg = Color.Lerp(bg, Color.White, 0.10f);

            var body = new Rectangle(r.X, r.Y + off, r.Width, r.Height);
            RoundRect(body, 26, bg);
            RoundRect(new Rectangle(body.X + 8, body.Bottom - 10, body.Width - 16, 10), 5,
                      Color.Lerp(bg, Color.Black, 0.20f));

            var textColor = primary ? Color.White : Config.Navy;
            if (!enabled) textColor = new Color(120, 128, 134);
            TextCentered(fontSize, label, body, textColor, titleFont);

            if (click) AudioManager.Play("click");
            return click;
        }

        public bool Toggle(Rectangle r, bool value, out bool changed)
        {
            changed = false;
            if (Pressed && r.Contains(Pointer))
            {
                value = !value;
                changed = true;
                AudioManager.Play("click");
            }

            var bg = value ? Config.Teal : new Color(189, 195, 199);
            RoundRect(new Rectangle(r.X, r.Y + 4, r.Width, r.Height), r.Height / 2, new Color(0, 0, 0, 30));
            RoundRect(r, r.Height / 2, bg);

            float knob = r.Height - 10;
            float x = value ? r.Right - knob - 5 : r.X + 5;
            Circle(new Vector2(x + knob / 2f, r.Y + r.Height / 2f), knob / 2f, Color.White);
            return value;
        }

        public bool Slider(Rectangle r, ref float value)
        {
            bool changed = false;
            if (Down && r.Contains(Pointer))
            {
                value = Math.Clamp((Pointer.X - r.X) / (float)r.Width, 0f, 1f);
                changed = true;
            }

            RoundRect(r, r.Height / 2, new Color(210, 216, 222));
            int w = (int)(r.Width * value);
            if (w > 0)
                RoundRect(new Rectangle(r.X, r.Y, Math.Max(r.Height, w), r.Height), r.Height / 2, Config.Brand);
            Circle(new Vector2(r.X + w, r.Y + r.Height / 2f), r.Height / 2f + 6, Color.White);
            return changed;
        }

        public void Badge(Rectangle r, string text, Color bg)
        {
            RoundRect(r, r.Height / 2, bg);
            TextCentered(r.Height * 0.62f, text, r, Color.White, true);
        }

        // ------------------------------------------------------------- recursos
        private static Texture2D MakeCorner(GraphicsDevice gd, int size)
        {
            var data = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - size;
                    float dy = y + 0.5f - size;
                    float d = MathF.Sqrt(dx * dx + dy * dy);
                    float a = Math.Clamp(size - d + 0.5f, 0f, 1f);
                    data[y * size + x] = Color.White * a;
                }
            }
            var t = new Texture2D(gd, size, size);
            t.SetData(data);
            return t;
        }

        private static Texture2D MakeStar(GraphicsDevice gd, int size)
        {
            var data = new Color[size * size];
            var pts = new Vector2[10];
            float cx = size / 2f, cy = size / 2f;
            for (int i = 0; i < 10; i++)
            {
                float ang = -MathF.PI / 2f + i * MathF.PI / 5f;
                float rad = i % 2 == 0 ? size * 0.48f : size * 0.21f;
                pts[i] = new Vector2(cx + MathF.Cos(ang) * rad, cy + MathF.Sin(ang) * rad);
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = false;
                    for (int i = 0, j = 9; i < 10; j = i++)
                    {
                        if (((pts[i].Y > y) != (pts[j].Y > y)) &&
                            (x < (pts[j].X - pts[i].X) * (y - pts[i].Y) / (pts[j].Y - pts[i].Y) + pts[i].X))
                            inside = !inside;
                    }
                    data[y * size + x] = inside ? Color.White : Color.Transparent;
                }
            }

            var t = new Texture2D(gd, size, size);
            t.SetData(data);
            return t;
        }

        private static Texture2D MakeHex(GraphicsDevice gd, int size)
        {
            var data = new Color[size * size];
            float cx = size / 2f, cy = size / 2f;
            float r = size * 0.5f - 1f;
            float halfW = r * 0.8660254f;   // √3/2

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = MathF.Abs(x + 0.5f - cx);
                    float dy = MathF.Abs(y + 0.5f - cy);
                    bool inside = dx <= halfW && dy <= r - dx * 0.5773503f; // 1/√3
                    data[y * size + x] = inside ? Color.White : Color.Transparent;
                }
            }

            var t = new Texture2D(gd, size, size);
            t.SetData(data);
            return t;
        }

        public void Dispose()
        {
            _sb.Dispose();
            _pixel.Dispose();
            _corner.Dispose();
            _star.Dispose();
            _hex.Dispose();
        }
    }
}
