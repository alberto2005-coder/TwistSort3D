using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TwistSort3D.Core;

namespace TwistSort3D.Game
{
    public class FloatingText
    {
        public string Text;
        public Color Tint;
        public Vector3 World;
        public float T;
    }

    /// <summary>
    /// Lógica de una partida: tablero, bandeja, obstáculos, herramientas,
    /// puntuación y condiciones de victoria/derrota.
    /// </summary>
    public class GameSession
    {
        public static readonly string[] ToolIds = { "hammer", "swap", "expand", "hint", "freeze" };

        public int LevelNumber { get; }
        public LevelData Level { get; }
        public Board Board { get; }
        public Tray Tray { get; }

        public int Moves { get; private set; }
        public int Score { get; private set; }
        public int Stars { get; private set; }
        public int CoinsEarned { get; private set; }

        public bool Won { get; private set; }
        public bool Lost { get; private set; }
        public string LoseReasonKey { get; private set; } = "LOSE_DESC";
        public float EndTimer { get; private set; }

        public bool Daily { get; }
        public bool Marathon { get; }

        public string Message { get; private set; } = "";
        public float MessageTimer { get; private set; }
        public float HintTimer { get; private set; }
        public ScrewState HintScrew { get; private set; }
        public float TrayFlash { get; private set; }
        public float Shake { get; private set; }
        public int TriplesPopped { get; private set; }

        public List<FloatingText> Floats { get; } = new();

        public GameSession(int level, bool daily = false, bool marathon = false)
        {
            LevelNumber = level;
            Daily = daily;
            Marathon = marathon;
            Level = LevelGenerator.Generate(level, daily, marathon);
            Board = new Board(Level);
            int slots = marathon ? Math.Clamp(5 + level / 10, 5, 8) : 6;
            Tray = new Tray(slots);
        }

        // ------------------------------------------------------------------ update
        public void Update(float dt)
        {
            Board.Update(dt);
            Board.CheckPlateFalls();

            MessageTimer = Math.Max(0f, MessageTimer - dt);
            TrayFlash = Math.Max(0f, TrayFlash - dt * 3f);
            Shake = Math.Max(0f, Shake - dt * 4f);

            if (HintTimer > 0f)
            {
                HintTimer -= dt;
                if (HintTimer <= 0f) HintScrew = null;
            }

            foreach (var f in Floats) f.T += dt * 1.5f;
            Floats.RemoveAll(f => f.T >= 1f);

            if (Won || Lost) EndTimer += dt;

            if (!Won && !Lost && Board.AllCleared && Tray.IsEmpty)
                Win();
        }

        // ------------------------------------------------------------------ jugar
        public bool TryTake(ScrewState s)
        {
            if (Won || Lost || s == null || s.Removed) return false;

            if (!s.Plate.Accessible)
            {
                Msg("MSG_BLOCKED");
                AudioManager.Play("error");
                return false;
            }

            if (s.Data.Obstacle == ObstacleType.Ice && s.Data.IceRemaining > 0)
            {
                Msg("MSG_ICE");
                AudioManager.Play("error");
                return false;
            }

            if (s.Data.Obstacle == ObstacleType.Locked && !Board.KeyRemoved)
            {
                Msg("MSG_LOCKED");
                AudioManager.Play("error");
                return false;
            }

            if (s.Data.Obstacle == ObstacleType.Wood && s.Data.WoodHits > 1)
            {
                s.Data.WoodHits--;
                AudioManager.Play("error", 0.15f);
                Msg("MSG_WOOD");
                return true;
            }

            var group = new List<ScrewState> { s };
            var partner = Board.FindPartner(s);
            if (partner != null) group.Add(partner);

            int needed = group.Sum(x => x.Data.Obstacle == ObstacleType.Double ? 2 : 1);
            if (!Tray.CanFit(needed))
            {
                if (AnyToolAvailable)
                {
                    Msg("MSG_TRAY_FULL");
                    AudioManager.Play("error");
                    return false;
                }
                Lose("MSG_TRAY_FULL");
                return false;
            }

            Moves++;

            foreach (var t in group)
                RemoveScrew(t);

            // la placa puede caer tras quitar sus tornillos
            Board.CheckPlateFalls();

            // tríos
            int pops = Tray.PopTriples();
            if (pops > 0)
            {
                Score += 45 * pops;
                TriplesPopped += pops;
                TrayFlash = 1f;
                Shake = 1f;
                AudioManager.Play("match");
            }

            // bombas: cada acción consume un movimiento
            foreach (var b in Board.Screws.Where(x => !x.Removed && x.Data.Obstacle == ObstacleType.Bomb))
            {
                b.Data.BombMoves--;
                if (b.Data.BombMoves <= 0)
                {
                    Lose("LOSE_BOMB");
                    return true;
                }
                if (b.Data.BombMoves <= 2) AudioManager.Play("bomb");
            }

            if (!Board.AllCleared && Tray.IsFull && !Tray.HasTriple)
                Msg("MSG_TRAY_FULL");

            return true;
        }

        private void RemoveScrew(ScrewState s)
        {
            Board.MarkRemoved(s);
            SaveManager.Data.ScrewsRemoved++;

            Score += 10;
            int color = s.Data.Color;

            int times = s.Data.Obstacle == ObstacleType.Double ? 2 : 1;
            for (int i = 0; i < times; i++) Tray.Add(color);

            if (s.Data.Obstacle == ObstacleType.Paint)
            {
                int idx = Tray.Slots.FindIndex(x => x >= 0);
                if (idx >= 0)
                {
                    int newColor = Random.Shared.Next(Math.Max(1, Level.Colors));
                    Tray.Slots[idx] = newColor;
                    AudioManager.Play("ice");
                }
            }

            Floats.Add(new FloatingText
            {
                Text = "+10",
                Tint = Config.Honey,
                World = s.Position + new Vector3(0, 0.35f, 0),
                T = 0f
            });

            // hielo: cada extracción acerca a los tornillos helados a liberarse
            foreach (var o in Board.Screws)
                if (!o.Removed && o.Data.Obstacle == ObstacleType.Ice && o.Data.IceRemaining > 0)
                    o.Data.IceRemaining--;
        }

        // ------------------------------------------------------------------ herramientas
        public bool AnyToolAvailable => ToolIds.Any(id => SaveManager.GetTool(id) > 0);

        public bool UseTool(string id)
        {
            if (Won || Lost) return false;
            if (!SaveManager.SpendTool(id))
            {
                Msg("MSG_NO_TOOL");
                AudioManager.Play("error");
                return false;
            }

            switch (id)
            {
                case "hammer":
                    Tray.RemoveFirstAny();
                    AudioManager.Play("click");
                    break;

                case "swap":
                    SwapFirstTwo();
                    AudioManager.Play("click", 0.1f);
                    break;

                case "expand":
                    Tray.Expand(2);
                    AudioManager.Play("expand");
                    break;

                case "hint":
                    HintScrew = Board.Playable.OrderBy(_ => Random.Shared.Next()).FirstOrDefault();
                    if (HintScrew != null)
                    {
                        HintTimer = 4f;
                        Msg("MSG_HINT");
                        AudioManager.Play("hint");
                    }
                    break;

                case "freeze":
                    int c = Tray.MostCommonColor();
                    if (c >= 0) Tray.RemoveOne(c);
                    AudioManager.Play("ice");
                    break;
            }

            int pops = Tray.PopTriples();
            if (pops > 0)
            {
                Score += 30 * pops;
                TriplesPopped += pops;
                TrayFlash = 1f;
                AudioManager.Play("match");
            }
            return true;
        }

        private void SwapFirstTwo()
        {
            var idx = Tray.Slots.Select((v, i) => (v, i)).Where(x => x.v >= 0).ToList();
            if (idx.Count < 2) return;
            int i0 = idx[0].i;
            int i1 = idx.First(x => x.v != idx[0].v).i;
            (Tray.Slots[i0], Tray.Slots[i1]) = (Tray.Slots[i1], Tray.Slots[i0]);
        }

        // ------------------------------------------------------------------ fin
        private void Win()
        {
            Won = true;
            EndTimer = 0f;
            AudioManager.Play("win");

            Stars = Moves <= Level.OptimalMoves ? 3
                  : Moves <= Level.OptimalMoves + 3 ? 2 : 1;
            Score += Stars * 50;
            CoinsEarned = Stars switch { 3 => 20, 2 => 10, _ => 5 };

            SaveManager.AddCoins(CoinsEarned);
            SaveManager.Data.LevelsWon++;
            SaveManager.Data.Stars[LevelNumber] =
                Math.Max(SaveManager.Data.Stars.GetValueOrDefault(LevelNumber), Stars);

            if (Daily) SaveManager.Data.LastDaily = DateTime.Today.ToString("yyyy-MM-dd");
            else if (Marathon) SaveManager.Data.MarathonLevel = Math.Max(SaveManager.Data.MarathonLevel, LevelNumber + 1);
            else SaveManager.Data.CurrentLevel = Math.Max(SaveManager.Data.CurrentLevel, LevelNumber + 1);

            if (!Daily && !Marathon && LevelNumber % 3 == 0)
                SaveManager.AddTool(ToolIds[Random.Shared.Next(ToolIds.Length)]);

            SaveManager.Save();
        }

        private void Lose(string reasonKey)
        {
            if (Lost) return;
            Lost = true;
            EndTimer = 0f;
            LoseReasonKey = reasonKey;
            AudioManager.Play("lose");
            SaveManager.Data.LevelsLost++;
            SaveManager.Save();
        }

        public void Msg(string key)
        {
            Message = Loc.T(key);
            MessageTimer = 2.2f;
        }
    }
}
