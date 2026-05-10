using System;
using SplashKitSDK;

public class Program
{
    public static void Main()
    {
        SplashKit.OpenWindow("Grid Tactics", 720, 640);

        Game game = new Game();

        while (!SplashKit.WindowCloseRequested("Grid Tactics"))
        {
            game.HandleInput();
            game.Draw();
        }
    }
}
