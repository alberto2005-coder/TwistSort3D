using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TwistSort3D.Render
{
    public sealed class Mesh
    {
        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;
        public int PrimitiveCount;
    }

    /// <summary>Generación de geometría procedural (sin modelos externos).</summary>
    public static class MeshFactory
    {
        public static Mesh Box(GraphicsDevice gd, float size = 1f)
        {
            float h = size * 0.5f;
            var verts = new VertexPositionColor[24];
            var idx = new int[36];
            int v = 0, i = 0;

            void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float shade)
            {
                var col = Color.White * shade;
                verts[v + 0] = new VertexPositionColor(a, col);
                verts[v + 1] = new VertexPositionColor(b, col);
                verts[v + 2] = new VertexPositionColor(c, col);
                verts[v + 3] = new VertexPositionColor(d, col);
                idx[i + 0] = v + 0; idx[i + 1] = v + 1; idx[i + 2] = v + 2;
                idx[i + 3] = v + 0; idx[i + 4] = v + 2; idx[i + 5] = v + 3;
                v += 4; i += 6;
            }

            // +Z, -Z, +X, -X, +Y, -Y
            Face(new Vector3(-h, -h, h), new Vector3(h, -h, h), new Vector3(h, h, h), new Vector3(-h, h, h), 0.86f);
            Face(new Vector3(h, -h, -h), new Vector3(-h, -h, -h), new Vector3(-h, h, -h), new Vector3(h, h, -h), 0.72f);
            Face(new Vector3(h, -h, h), new Vector3(h, -h, -h), new Vector3(h, h, -h), new Vector3(h, h, h), 0.78f);
            Face(new Vector3(-h, -h, -h), new Vector3(-h, -h, h), new Vector3(-h, h, h), new Vector3(-h, h, -h), 0.74f);
            Face(new Vector3(-h, h, h), new Vector3(h, h, h), new Vector3(h, h, -h), new Vector3(-h, h, -h), 1.0f);
            Face(new Vector3(-h, -h, -h), new Vector3(h, -h, -h), new Vector3(h, -h, h), new Vector3(-h, -h, h), 0.55f);

            return Build(gd, verts, idx);
        }

        public static Mesh Cylinder(GraphicsDevice gd, int segments = 16)
        {
            int vCount = segments * 8 + 2;
            var verts = new VertexPositionColor[vCount];
            var idx = new int[segments * 12];

            int v = 0, i = 0;
            for (int s = 0; s < segments; s++)
            {
                float a0 = MathHelper.TwoPi * s / segments;
                float a1 = MathHelper.TwoPi * (s + 1) / segments;
                float x0 = (float)Math.Cos(a0), z0 = (float)Math.Sin(a0);
                float x1 = (float)Math.Cos(a1), z1 = (float)Math.Sin(a1);

                var cSide = Color.White * 0.78f;
                var cTop = Color.White;

                verts[v + 0] = new VertexPositionColor(new Vector3(x0, 0, z0), cSide);
                verts[v + 1] = new VertexPositionColor(new Vector3(x1, 0, z1), cSide);
                verts[v + 2] = new VertexPositionColor(new Vector3(x1, 1, z1), cTop);
                verts[v + 3] = new VertexPositionColor(new Vector3(x0, 1, z0), cTop);

                idx[i + 0] = v + 0; idx[i + 1] = v + 1; idx[i + 2] = v + 2;
                idx[i + 3] = v + 0; idx[i + 4] = v + 2; idx[i + 5] = v + 3;
                v += 4; i += 6;
            }

            // tapas
            int topCenter = v;
            verts[v++] = new VertexPositionColor(new Vector3(0, 1, 0), Color.White);
            int bottomCenter = v;
            verts[v++] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White * 0.6f);

            for (int s = 0; s < segments; s++)
            {
                float a0 = MathHelper.TwoPi * s / segments;
                float a1 = MathHelper.TwoPi * (s + 1) / segments;
                float x0 = (float)Math.Cos(a0), z0 = (float)Math.Sin(a0);
                float x1 = (float)Math.Cos(a1), z1 = (float)Math.Sin(a1);

                // tapa superior
                verts[v + 0] = new VertexPositionColor(new Vector3(x0, 1, z0), Color.White);
                verts[v + 1] = new VertexPositionColor(new Vector3(x1, 1, z1), Color.White);
                idx[i + 0] = topCenter; idx[i + 1] = v + 0; idx[i + 2] = v + 1;
                v += 2; i += 3;

                // tapa inferior
                verts[v + 0] = new VertexPositionColor(new Vector3(x1, 0, z1), Color.White * 0.6f);
                verts[v + 1] = new VertexPositionColor(new Vector3(x0, 0, z0), Color.White * 0.6f);
                idx[i + 0] = bottomCenter; idx[i + 1] = v + 0; idx[i + 2] = v + 1;
                v += 2; i += 3;
            }

            Array.Resize(ref idx, i);
            Array.Resize(ref verts, v);

            return Build(gd, verts, idx);
        }

        private static Mesh Build(GraphicsDevice gd, VertexPositionColor[] verts, int[] idx)
        {
            var vb = new VertexBuffer(gd, VertexPositionColor.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vb.SetData(verts);
            var ib = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, idx.Length, BufferUsage.WriteOnly);
            ib.SetData(idx);
            return new Mesh { VertexBuffer = vb, IndexBuffer = ib, PrimitiveCount = idx.Length / 3 };
        }
    }
}
