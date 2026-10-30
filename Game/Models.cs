using System.Collections.Generic;

namespace TwistSort3D.Game
{
    public enum ObstacleType
    {
        None,
        Double,   // ocupa 2 ranuras
        Wood,     // 2 toques
        Ice,      // espera N extracciones
        Linked,   // pareja encadenada
        Locked,   // necesita llave
        Swivel,   // placa que reordena
        Paint,    // tiñe otro tornillo
        Bomb      // cuenta atrás
    }

    public class ScrewData
    {
        public int Color;
        public ObstacleType Obstacle = ObstacleType.None;
        public int IceRemaining = 3;
        public int BombMoves = 6;
        public int WoodHits = 2;
        public int ChainId = -1;
        public bool IsKey;
    }

    public class PlateData
    {
        public int Layer;
        public float X;
        public float Z;
        public float W = 1.75f;
        public float D = 1.75f;
        public bool Wooden;
        public List<ScrewData> Screws = new();
    }

    public class LevelData
    {
        public int Number;
        public int Colors;
        public int Biome;
        public ObstacleType Obstacle;
        public List<PlateData> Plates = new();
        public int OptimalMoves;
        public bool Daily;
        public bool Marathon;
    }
}
