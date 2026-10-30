using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TwistSort3D.Core;

namespace TwistSort3D.Game
{
    public class ScrewState
    {
        public ScrewData Data;
        public PlateState Plate;
        public Vector3 Position;
        public bool Removed;
        public float FlyT;
        public Vector3 FlyFrom;
        public bool Highlight;
    }

    public class PlateState
    {
        public PlateData Data;
        public bool Accessible;
        public bool Fallen;
        public bool Falling;
        public float FallT;
        public Vector3 Origin;
        public List<ScrewState> Screws = new();

        public bool Empty => Screws.All(s => s.Removed);
    }

    /// <summary>Estado en tiempo de ejecución del tablero 3D.</summary>
    public class Board
    {
        public const float LayerHeight = 0.56f;
        public const float PlateY = 0.26f;

        public LevelData Level { get; }
        public List<PlateState> Plates { get; } = new();
        public List<ScrewState> Screws { get; } = new();

        public Board(LevelData level)
        {
            Level = level;
            foreach (var pd in level.Plates)
            {
                var plate = new PlateState { Data = pd };
                float y = PlateY + pd.Layer * LayerHeight;
                plate.Origin = new Vector3(pd.X, y, pd.Z);

                var offs = ScrewOffsets(pd.Screws.Count);
                for (int i = 0; i < pd.Screws.Count; i++)
                {
                    var sd = pd.Screws[i];
                    var pos = plate.Origin + new Vector3(offs[i].X, 0.16f, offs[i].Y);
                    var ss = new ScrewState { Data = sd, Plate = plate, Position = pos };
                    plate.Screws.Add(ss);
                    Screws.Add(ss);
                }
                Plates.Add(plate);
            }
            RecomputeAccessibility();
        }

        public bool AllCleared => Plates.All(p => p.Fallen);

        public IEnumerable<ScrewState> Playable =>
            Screws.Where(s => !s.Removed && s.Plate.Accessible);

        public void Update(float dt)
        {
            foreach (var p in Plates)
            {
                if (p.Falling)
                {
                    p.FallT += dt * 2.2f;
                    if (p.FallT >= 1f)
                    {
                        p.FallT = 1f;
                        p.Falling = false;
                        p.Fallen = true;
                        RecomputeAccessibility();
                    }
                }
            }

            foreach (var s in Screws)
                if (s.Removed && s.FlyT < 1f)
                    s.FlyT = Math.Min(1f, s.FlyT + dt * 3f);
        }

        public void RecomputeAccessibility()
        {
            foreach (var p in Plates)
            {
                if (p.Fallen) { p.Accessible = false; continue; }
                bool blocked = false;
                foreach (var q in Plates)
                {
                    if (q == p || q.Fallen) continue;
                    if (q.Data.Layer > p.Data.Layer &&
                        Math.Abs(q.Data.X - p.Data.X) < 1.05f &&
                        Math.Abs(q.Data.Z - p.Data.Z) < 1.05f)
                    {
                        blocked = true;
                        break;
                    }
                }
                p.Accessible = !blocked;
            }
        }

        public void MarkRemoved(ScrewState s)
        {
            if (s.Removed) return;
            s.Removed = true;
            s.FlyT = 0f;
            s.FlyFrom = s.Position;
            AudioManager.Play("screw", (float)(Random.Shared.NextDouble() * 0.1 - 0.05));
        }

        /// <summary>Inicia la caída de las placas que se han quedado vacías.</summary>
        public void CheckPlateFalls()
        {
            foreach (var p in Plates)
            {
                if (!p.Fallen && !p.Falling && p.Empty)
                {
                    p.Falling = true;
                    p.FallT = 0f;
                    AudioManager.Play("drop");
                }
            }
        }

        public ScrewState FindPartner(ScrewState s)
        {
            if (s.Data.ChainId < 0) return null;
            return Screws.FirstOrDefault(o => o != s && !o.Removed &&
                                              o.Data.Obstacle == ObstacleType.Linked &&
                                              o.Data.ChainId == s.Data.ChainId);
        }

        public bool KeyRemoved =>
            Screws.All(s => !s.Data.IsKey || s.Removed);

        // ------------------------------------------------------------- geometría
        private static Vector2[] ScrewOffsets(int n)
        {
            switch (n)
            {
                case 1: return new[] { new Vector2(0, 0) };
                case 2: return new[] { new Vector2(-0.42f, 0), new Vector2(0.42f, 0) };
                case 3: return new[]
                {
                    new Vector2(-0.48f, -0.33f), new Vector2(0.48f, -0.33f), new Vector2(0, 0.45f)
                };
                case 4: return new[]
                {
                    new Vector2(-0.45f, -0.45f), new Vector2(0.45f, -0.45f),
                    new Vector2(-0.45f, 0.45f), new Vector2(0.45f, 0.45f)
                };
                default:
                    var list = new List<Vector2>();
                    for (int i = 0; i < n; i++)
                    {
                        int col = i % 3;
                        int row = i / 3;
                        list.Add(new Vector2((col - 1) * 0.5f, (row - 0.5f) * 0.55f));
                    }
                    return list.ToArray();
            }
        }
    }
}
