using System;

namespace FloodControl
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new FloodControl())
                game.Run();
        }
    }
#endif
}
