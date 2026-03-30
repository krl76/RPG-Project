namespace Core.Gameplay.State
{
    /// <summary>
    /// Набор основных состояний игрового цикла.
    /// </summary>
    public enum GameState
    {
        None = 0,
        Bootstrapping = 1,
        Loading = 2,
        MainMenu = 3,
        Gameplay = 4,
        Paused = 5,
        GameOver = 6
    }
}
