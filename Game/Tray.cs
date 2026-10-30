using System.Collections.Generic;
using System.Linq;

namespace TwistSort3D.Game
{
    /// <summary>Bandeja temporal de 6 (u 8) ranuras con combinación de tríos.</summary>
    public class Tray
    {
        public int Capacity { get; private set; }
        public List<int> Slots { get; } = new();

        public Tray(int capacity)
        {
            Capacity = capacity;
            for (int i = 0; i < capacity; i++) Slots.Add(-1);
        }

        public int Free => Slots.Count(s => s < 0);
        public bool IsFull => Free == 0;
        public bool IsEmpty => Slots.All(s => s < 0);

        public bool CanFit(int needed) => Free >= needed;

        public bool Add(int color)
        {
            int i = Slots.IndexOf(-1);
            if (i < 0) return false;
            Slots[i] = color;
            return true;
        }

        public int CountColor(int color) => Slots.Count(s => s == color);

        public bool HasTriple => Slots.Where(s => s >= 0)
                                      .GroupBy(s => s)
                                      .Any(g => g.Count() >= 3);

        /// <summary>Elimina todos los tríos posibles y devuelve cuántos se limpiaron.</summary>
        public int PopTriples()
        {
            int pops = 0;
            bool again = true;
            while (again)
            {
                again = false;
                var group = Slots.Where(s => s >= 0)
                                 .GroupBy(s => s)
                                 .FirstOrDefault(g => g.Count() >= 3);
                if (group == null) break;

                int color = group.Key;
                int removed = 0;
                for (int i = 0; i < Slots.Count && removed < 3; i++)
                {
                    if (Slots[i] == color) { Slots[i] = -1; removed++; }
                }
                pops++;
                again = true;
            }
            return pops;
        }

        /// <summary>Quita la primera ranura del color indicado. Devuelve true si lo hizo.</summary>
        public bool RemoveOne(int color)
        {
            int i = Slots.IndexOf(color);
            if (i < 0) return false;
            Slots[i] = -1;
            return true;
        }

        public bool RemoveFirstAny()
        {
            int i = Slots.FindIndex(s => s >= 0);
            if (i < 0) return false;
            Slots[i] = -1;
            return true;
        }

        public int MostCommonColor()
        {
            var g = Slots.Where(s => s >= 0).GroupBy(s => s)
                         .OrderByDescending(x => x.Count())
                         .FirstOrDefault();
            return g?.Key ?? -1;
        }

        public void Expand(int extra)
        {
            Capacity = System.Math.Min(8, Capacity + extra);
            while (Slots.Count < Capacity) Slots.Add(-1);
        }

        public void Clear() { for (int i = 0; i < Slots.Count; i++) Slots[i] = -1; }
    }
}
