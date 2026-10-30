using System;

namespace TwistSort3D
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using var game = new TwistSortGame();
            game.Run();
        }
    }
}
