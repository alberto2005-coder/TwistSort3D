using System;
using Microsoft.Xna.Framework;

namespace TwistSort3D.Core
{
    /// <summary>
    /// Constantes globales del juego: resolución virtual, paleta de colores,
    /// tipografías, biomas y fórmulas de progresión.
    /// </summary>
    public static class Config
    {
        // ---------------------------------------------------------------- pantalla
        public const int VirtualW = 1080;
        public const int VirtualH = 1920;
        public const int DefaultWindowW = 540;
        public const int DefaultWindowH = 960;

        public const string GameName = "Twist & Sort 3D";
        public const string Version = "1.0.0";

        // ---------------------------------------------------------------- marca
        public static readonly Color Brand = new Color(108, 92, 231);       // #6C5CE7
        public static readonly Color BrandDark = new Color(72, 52, 153);    // #483499
        public static readonly Color Teal = new Color(0, 206, 201);         // #00CEC9
        public static readonly Color Coral = new Color(225, 112, 85);       // #E17055
        public static readonly Color Honey = new Color(253, 203, 110);      // #FDCB6E
        public static readonly Color Cream = new Color(247, 249, 251);      // #F7F9FB
        public static readonly Color Navy = new Color(45, 52, 54);          // #2D3436
        public static readonly Color Grey = new Color(223, 228, 234);       // #DFE4EA
        public static readonly Color TextGrey = new Color(99, 110, 114);    // #636E72
        public static readonly Color White = Color.White;
        public static readonly Color Black = Color.Black;
        public static readonly Color Danger = new Color(214, 48, 49);      // #D63031
        public static readonly Color Success = new Color(0, 184, 148);      // #00B894
        public static readonly Color Disabled = new Color(178, 190, 195);   // #B2BEC3

        // ---------------------------------------------------------------- tornillos
        public static readonly Color[] ScrewColors =
        {
            new Color(255, 107, 107), // 0 rojo    #FF6B6B
            new Color(77, 171, 247),  // 1 azul    #4DABF7
            new Color(81, 207, 102),  // 2 verde   #51CF66
            new Color(252, 196, 25),  // 3 amarillo#FCC419
            new Color(151, 117, 250), // 4 violeta #9775FA
            new Color(255, 146, 43),  // 5 naranja #FF922B
            new Color(247, 131, 172), // 6 rosa    #F783AC
            new Color(59, 201, 219)   // 7 cian    #3BC9DB
        };

        public static readonly string[] ColorNamesEs =
        { "Rojo", "Azul", "Verde", "Amarillo", "Violeta", "Naranja", "Rosa", "Cian" };

        public static readonly string[] ColorNamesEn =
        { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Cyan" };

        // ---------------------------------------------------------------- biomas
        public static readonly Color[] BiomeTop =
        {
            new Color(168, 224, 99),  // 1 jardín
            new Color(255, 153, 102), // 2 amanecer
            new Color(116, 235, 213), // 3 playa
            new Color(246, 211, 101), // 4 candy
            new Color(161, 196, 253), // 5 cielo
            new Color(75, 108, 183),  // 6 noche
            new Color(248, 54, 0),    // 7 lava
            new Color(65, 41, 90)     // 8 cosmos
        };

        public static readonly Color[] BiomeBottom =
        {
            new Color(86, 171, 47),   // 1 jardín
            new Color(255, 94, 98),   // 2 amanecer
            new Color(11, 163, 96),   // 3 playa
            new Color(253, 160, 133), // 4 candy
            new Color(194, 233, 251), // 5 cielo
            new Color(24, 40, 72),    // 6 noche
            new Color(249, 212, 35),  // 7 lava
            new Color(47, 7, 67)      // 8 cosmos
        };

        /// <summary>Bioma (0-7) correspondiente a un número de nivel.</summary>
        public static int Biome(int level) => ((Math.Max(1, level) - 1) / 25) % 8;

        // ---------------------------------------------------------------- fórmulas
        public static int ScrewCount(int level)
        {
            int n = 6 + (int)(level * 0.8);
            n = Math.Clamp(n, 6, 60);
            return (n / 3) * 3;
        }

        public static int ColorCount(int level) => Math.Clamp(2 + level / 15, 2, 7);

        public static int Layers(int level) => Math.Clamp(1 + level / 12, 1, 5);

        public static float Difficulty(int level) => Math.Clamp(level / 120f, 0f, 1f);

        // ---------------------------------------------------------------- ayuda
        public const float ToolGiftEvery = 3f;
        public const int MaxToolStock = 5;
    }
}
