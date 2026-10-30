using System;
using System.Collections.Generic;
using System.Linq;
using TwistSort3D.Core;

namespace TwistSort3D.Game
{
    /// <summary>
    /// Generador procedural determinista. La misma semilla produce siempre el
    /// mismo nivel, y todos los colores aparecen en múltiplos de 3 (siempre soluble).
    /// </summary>
    public static class LevelGenerator
    {
        public static LevelData Generate(int level, bool daily = false, bool marathon = false)
        {
            var rng = new Random(SeedFor(level, daily));
            int n = Config.ScrewCount(level);
            int colors = Math.Min(Config.ColorCount(level), Math.Max(1, n / 3));
            int layers = Config.Layers(level);

            var data = new LevelData
            {
                Number = level,
                Colors = colors,
                Biome = Config.Biome(level),
                Daily = daily,
                Marathon = marathon,
                OptimalMoves = n
            };

            // 1) bolsa de colores: cada color aparece un múltiplo de 3
            var bag = BuildColorBag(rng, n, colors);
            Shuffle(bag, rng);

            // 2) repartir placas entre capas
            int platesTotal = Math.Max(1, (int)Math.Ceiling(n / 3.0));
            var perLayer = new int[layers];
            for (int i = 0; i < platesTotal; i++) perLayer[i % layers]++;

            int cursor = 0;
            for (int layer = 0; layer < layers; layer++)
            {
                int count = perLayer[layer];
                for (int p = 0; p < count && cursor < bag.Count; p++)
                {
                    float x = count == 1 ? 0f : (p - (count - 1) / 2f) * 1.62f;
                    var plate = new PlateData { Layer = layer, X = x, Z = 0f };

                    int remainingPlates = count - p;
                    int remainingScrews = bag.Count - cursor;
                    int take = Math.Min(rng.Next(2, 5), Math.Max(2, remainingScrews / remainingPlates));
                    if (remainingPlates == 1) take = Math.Min(remainingScrews, 4);
                    if (remainingScrews <= 2) take = remainingScrews;

                    for (int i = 0; i < take && cursor < bag.Count; i++)
                        plate.Screws.Add(new ScrewData { Color = bag[cursor++] });

                    data.Plates.Add(plate);
                }
            }

            // 3) obstáculo según el nivel
            data.Obstacle = ObstacleForLevel(level);
            ApplyObstacle(data, rng);

            // 4) validación: todos los colores deben ser múltiplos de 3
            if (!IsSolvable(data))
                data = Generate(level + 1, daily, marathon); // regenera con otra semilla

            return data;
        }

        public static int SeedFor(int level, bool daily)
        {
            if (!daily) return level * 7919;
            var today = DateTime.Today;
            return today.Year * 10000 + today.Month * 100 + today.Day;
        }

        // ------------------------------------------------------------------ helpers
        private static List<int> BuildColorBag(Random rng, int n, int colors)
        {
            var bag = new List<int>();
            int assigned = 0;

            // 3 de cada color para empezar
            for (int c = 0; c < colors; c++) { bag.Add(c); bag.Add(c); bag.Add(c); assigned += 3; }

            // resto en grupos de 3
            while (assigned < n)
            {
                int c = rng.Next(colors);
                bag.Add(c); bag.Add(c); bag.Add(c);
                assigned += 3;
            }
            return bag;
        }

        private static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static ObstacleType ObstacleForLevel(int level)
        {
            if (level < 6) return ObstacleType.None;
            if (level < 12) return ObstacleType.Double;
            if (level < 20) return ObstacleType.Wood;
            if (level < 30) return ObstacleType.Ice;
            if (level < 45) return ObstacleType.Linked;
            if (level < 60) return ObstacleType.Locked;
            if (level < 80) return ObstacleType.Swivel;
            if (level < 100) return ObstacleType.Paint;
            return ObstacleType.Bomb;
        }

        private static void ApplyObstacle(LevelData data, Random rng)
        {
            var all = data.Plates.SelectMany(p => p.Screws).ToList();
            if (all.Count == 0) return;

            switch (data.Obstacle)
            {
                case ObstacleType.Double:
                    all[rng.Next(all.Count)].Obstacle = ObstacleType.Double;
                    break;

                case ObstacleType.Wood:
                    foreach (var s in Pick(all, rng, 2)) s.Obstacle = ObstacleType.Wood;
                    break;

                case ObstacleType.Ice:
                    foreach (var s in Pick(all, rng, 2))
                    {
                        s.Obstacle = ObstacleType.Ice;
                        s.IceRemaining = 3 + data.Number / 50;
                    }
                    break;

                case ObstacleType.Linked:
                    var pairs = Pick(all, rng, 4);
                    for (int i = 0; i + 1 < pairs.Count; i += 2)
                    {
                        pairs[i].Obstacle = ObstacleType.Linked;
                        pairs[i + 1].Obstacle = ObstacleType.Linked;
                        pairs[i].ChainId = pairs[i + 1].ChainId = i / 2 + 1;
                    }
                    break;

                case ObstacleType.Locked:
                    // la llave va en la capa superior para que sea accesible
                    var top = data.Plates.Where(p => p.Layer == 0).SelectMany(p => p.Screws).ToList();
                    if (top.Count > 0) top[rng.Next(top.Count)].IsKey = true;
                    var locked = Pick(all.Where(s => !s.IsKey).ToList(), rng, 1);
                    if (locked.Count > 0) locked[0].Obstacle = ObstacleType.Locked;
                    break;

                case ObstacleType.Swivel:
                    var plate = data.Plates[rng.Next(data.Plates.Count)];
                    plate.Screws[rng.Next(plate.Screws.Count)].Obstacle = ObstacleType.Swivel;
                    break;

                case ObstacleType.Paint:
                    all[rng.Next(all.Count)].Obstacle = ObstacleType.Paint;
                    break;

                case ObstacleType.Bomb:
                    all[rng.Next(all.Count)].Obstacle = ObstacleType.Bomb;
                    break;
            }
        }

        private static List<ScrewData> Pick(List<ScrewData> list, Random rng, int count)
        {
            var copy = list.Where(s => s.Obstacle == ObstacleType.None && !s.IsKey).ToList();
            Shuffle(copy, rng);
            return copy.Take(Math.Min(count, copy.Count)).ToList();
        }

        public static bool IsSolvable(LevelData data)
        {
            var counts = new Dictionary<int, int>();
            foreach (var s in data.Plates.SelectMany(p => p.Screws))
                counts[s.Color] = counts.GetValueOrDefault(s.Color) + 1;
            return counts.Values.All(v => v % 3 == 0);
        }
    }
}
