using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TwistSort3D.Core;
using TwistSort3D.Game;

namespace TwistSort3D.Render
{
    /// <summary>Cámara orbital y renderizado 3D del tablero.</summary>
    public class BoardRenderer
    {
        private readonly GraphicsDevice _gd;
        private readonly BasicEffect _fx;
        private readonly Mesh _box;
        private readonly Mesh _cyl6;
        private readonly Mesh _cyl16;

        private Matrix _view;
        private Matrix _proj;
        private Rectangle _viewport;

        public float Yaw { get; set; } = -0.55f;
        public float Pitch { get; set; } = 0.62f;
        public float Distance { get; set; } = 8.8f;
        public Vector3 Target { get; set; } = new Vector3(0, 1.1f, 0);

        public Matrix View => _view;
        public Matrix Projection => _proj;

        public BoardRenderer(GraphicsDevice gd)
        {
            _gd = gd;
            _fx = new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
                TextureEnabled = false
            };
            _box = MeshFactory.Box(gd);
            _cyl6 = MeshFactory.Cylinder(gd, 6);
            _cyl16 = MeshFactory.Cylinder(gd, 16);
        }

        public void UpdateCamera(Rectangle viewport, float targetY)
        {
            _viewport = viewport;
            Target = new Vector3(0, targetY, 0);
            float aspect = viewport.Width / (float)Math.Max(1, viewport.Height);

            var dir = new Vector3(
                MathF.Cos(Pitch) * MathF.Sin(Yaw),
                MathF.Sin(Pitch),
                MathF.Cos(Pitch) * MathF.Cos(Yaw));

            var pos = Target + dir * Distance;
            _view = Matrix.CreateLookAt(pos, Target, Vector3.Up);
            _proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(42f), aspect, 0.1f, 120f);
        }

        public void Draw(Board board, bool reduceMotion, float shake, Rectangle viewport)
        {
            _viewport = viewport;
            _gd.Viewport = new Viewport(viewport);
            _gd.Clear(ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            var oldRaster = _gd.RasterizerState;
            var oldDepth = _gd.DepthStencilState;
            var oldBlend = _gd.BlendState;

            _gd.RasterizerState = RasterizerState.CullNone;
            _gd.DepthStencilState = DepthStencilState.Default;
            _gd.BlendState = BlendState.Opaque;

            _fx.View = _view;
            _fx.Projection = _proj;
            _fx.Alpha = 1f;

            float shakeX = 0, shakeY = 0;
            if (shake > 0 && !reduceMotion)
            {
                shakeX = (float)(Random.Shared.NextDouble() - 0.5) * 0.06f * shake;
                shakeY = (float)(Random.Shared.NextDouble() - 0.5) * 0.06f * shake;
            }
            _fx.View = Matrix.CreateTranslation(shakeX, shakeY, 0) * _view;

            // plataforma base
            DrawMesh(_box, Matrix.CreateScale(3.7f, 0.22f, 3.7f) * Matrix.CreateTranslation(0, -0.12f, 0),
                     new Color(226, 233, 243));

            foreach (var plate in board.Plates)
            {
                if (plate.Fallen) continue;

                float yOff = plate.Falling ? -2.4f * EaseIn(plate.FallT) : 0f;
                var origin = plate.Origin + new Vector3(0, yOff, 0);

                var plateColor = plate.Data.Wooden
                    ? new Color(176, 128, 84)
                    : Color.Lerp(new Color(250, 249, 246), Config.Brand, 0.05f * plate.Data.Layer);

                DrawMesh(_box, Matrix.CreateScale(plate.Data.W, 0.16f, plate.Data.D) * Matrix.CreateTranslation(origin),
                         plateColor);

                // borde superior de la placa
                DrawMesh(_box, Matrix.CreateScale(plate.Data.W * 0.94f, 0.03f, plate.Data.D * 0.94f) *
                                Matrix.CreateTranslation(origin + new Vector3(0, 0.09f, 0)),
                         Color.Lerp(plateColor, Color.White, 0.35f));
            }

            foreach (var s in board.Screws)
            {
                if (s.Removed && s.FlyT >= 1f) continue;

                var pos = s.Position;
                float scale = 1f;
                float alpha = 1f;

                if (s.Removed)
                {
                    float t = EaseOut(s.FlyT);
                    pos += new Vector3(0, 1.4f * t, 0);
                    scale = 1f - 0.7f * t;
                    alpha = 1f - t;
                }

                var color = Config.ScrewColors[s.Data.Color % Config.ScrewColors.Length];

                if (s.Data.Obstacle == ObstacleType.Locked)
                    color = Color.Lerp(color, new Color(90, 90, 110), 0.55f);

                if (s.Highlight)
                    scale *= 1.12f;

                // cuerpo metálico
                DrawMesh(_cyl6,
                    Matrix.CreateScale(0.15f * scale, 0.30f * scale, 0.15f * scale) *
                    Matrix.CreateTranslation(pos + new Vector3(0, -0.02f, 0)),
                    new Color(78, 84, 96), alpha);

                // cabeza de color
                var headMesh = SaveManager.Data.ActiveScrewSkin == "round" ? _cyl16 : _cyl6;
                float headRadius = SaveManager.Data.ActiveScrewSkin switch
                {
                    "round" => 0.25f,
                    "hex" => 0.30f,
                    _ => 0.27f
                };
                float headHeight = SaveManager.Data.ActiveScrewSkin == "hex" ? 0.14f : 0.11f;
                DrawMesh(headMesh,
                    Matrix.CreateScale(headRadius * scale, headHeight * scale, headRadius * scale) *
                    Matrix.CreateTranslation(pos + new Vector3(0, 0.27f, 0)),
                    color, alpha);

                // hielo
                if (s.Data.Obstacle == ObstacleType.Ice && s.Data.IceRemaining > 0)
                {
                    _gd.BlendState = BlendState.AlphaBlend;
                    _fx.Alpha = 0.30f;
                    DrawMesh(_cyl16,
                        Matrix.CreateScale(0.34f * scale, 0.46f * scale, 0.34f * scale) *
                        Matrix.CreateTranslation(pos + new Vector3(0, 0.05f, 0)),
                        new Color(170, 220, 255), 0.30f);
                    _gd.BlendState = BlendState.Opaque;
                    _fx.Alpha = 1f;
                }
            }

            _gd.RasterizerState = oldRaster;
            _gd.DepthStencilState = oldDepth;
            _gd.BlendState = oldBlend;
        }

        private void DrawMesh(Mesh mesh, Matrix world, Color color, float alpha = 1f)
        {
            _fx.World = world;
            _fx.DiffuseColor = color.ToVector3();
            _fx.Alpha = alpha;

            _gd.SetVertexBuffer(mesh.VertexBuffer);
            _gd.Indices = mesh.IndexBuffer;

            foreach (var pass in _fx.CurrentTechnique.Passes)
            {
                pass.Apply();
                _gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mesh.PrimitiveCount);
            }
        }

        public Ray GetRay(Vector2 screenInViewport, Rectangle viewport)
        {
            var vp = new Viewport(viewport);
            var near = vp.Unproject(new Vector3(screenInViewport.X, screenInViewport.Y, 0f),
                                    _proj, _view, Matrix.Identity);
            var far = vp.Unproject(new Vector3(screenInViewport.X, screenInViewport.Y, 1f),
                                   _proj, _view, Matrix.Identity);
            return new Ray(near, Vector3.Normalize(far - near));
        }

        public Vector2 WorldToScreen(Vector3 world, Rectangle viewport)
        {
            var vp = new Viewport(viewport);
            var p = vp.Project(world, _proj, _view, Matrix.Identity);
            return new Vector2(p.X, p.Y);
        }

        private static float EaseOut(float t) => 1f - MathF.Pow(1f - t, 3f);
        private static float EaseIn(float t) => t * t * t;
    }
}
