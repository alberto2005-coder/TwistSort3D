using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TwistSort3D.Core
{
    public class SettingsData
    {
        public float Music { get; set; } = 0.65f;
        public float Sfx { get; set; } = 0.85f;
        public bool Vibration { get; set; } = true;
        public bool Symbols { get; set; } = false;
        public bool ReduceMotion { get; set; } = false;
        public string Quality { get; set; } = "alta";
    }

    public class SaveData
    {
        public int Version { get; set; } = 1;
        public string Language { get; set; } = "";
        public int CurrentLevel { get; set; } = 1;
        public int MarathonLevel { get; set; } = 1;
        public int Coins { get; set; } = 0;
        public Dictionary<int, int> Stars { get; set; } = new();
        public Dictionary<string, int> Tools { get; set; } = new()
        {
            ["hammer"] = 1, ["swap"] = 1, ["expand"] = 1, ["hint"] = 2, ["freeze"] = 0
        };
        public List<string> ScrewSkins { get; set; } = new() { "standard" };
        public List<string> TraySkins { get; set; } = new() { "wood" };
        public string ActiveScrewSkin { get; set; } = "standard";
        public string ActiveTraySkin { get; set; } = "wood";
        public SettingsData Settings { get; set; } = new();
        public string LastDaily { get; set; } = "";
        public int LastToolGiftLevel { get; set; } = 0;
        public bool TutorialDone { get; set; }
        public int LevelsWon { get; set; } = 0;
        public int LevelsLost { get; set; } = 0;
        public int ScrewsRemoved { get; set; } = 0;
    }

    /// <summary>Persistencia local en JSON (sin cuentas ni internet).</summary>
    public static class SaveManager
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true,
            IncludeFields = false
        };

        public static SaveData Data { get; private set; } = new();

        private static string Dir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwistSort3D");

        private static string FilePath => Path.Combine(Dir, "save.json");

        public static void Load()
        {
            try
            {
                if (!File.Exists(FilePath)) { Data = new SaveData(); Save(); return; }
                var json = File.ReadAllText(FilePath);
                Data = JsonSerializer.Deserialize<SaveData>(json, JsonOpts) ?? new SaveData();
                Normalize();
            }
            catch
            {
                Data = new SaveData();
            }
        }

        private static void Normalize()
        {
            Data.Tools ??= new Dictionary<string, int>();
            foreach (var k in new[] { "hammer", "swap", "expand", "hint", "freeze" })
                if (!Data.Tools.ContainsKey(k)) Data.Tools[k] = 0;
            Data.Stars ??= new Dictionary<int, int>();
            Data.ScrewSkins ??= new List<string> { "standard" };
            Data.TraySkins ??= new List<string> { "wood" };
            Data.Settings ??= new SettingsData();
            if (string.IsNullOrWhiteSpace(Data.ActiveScrewSkin)) Data.ActiveScrewSkin = "standard";
            if (string.IsNullOrWhiteSpace(Data.ActiveTraySkin)) Data.ActiveTraySkin = "wood";
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(Data, JsonOpts));
            }
            catch { /* el juego sigue funcionando aunque no se pueda guardar */ }
        }

        public static void Reset()
        {
            Data = new SaveData();
            Save();
        }

        // ------------------------------------------------------------- helpers
        public static int GetTool(string id) =>
            Data.Tools.TryGetValue(id, out var n) ? n : 0;

        public static void AddTool(string id, int n = 1)
        {
            Data.Tools[id] = Math.Min(Config.MaxToolStock, GetTool(id) + n);
            Save();
        }

        public static bool SpendTool(string id)
        {
            if (GetTool(id) <= 0) return false;
            Data.Tools[id]--;
            Save();
            return true;
        }

        public static void AddCoins(int n)
        {
            Data.Coins = Math.Max(0, Data.Coins + n);
            Save();
        }

        public static bool SpendCoins(int n)
        {
            if (Data.Coins < n) return false;
            Data.Coins -= n;
            Save();
            return true;
        }
    }
}
