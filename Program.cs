namespace MonoFighter;

public static class Program
{
    private static void Main()
    {
        using var game = new MonoFighterGame();
        game.Run();
    }
}