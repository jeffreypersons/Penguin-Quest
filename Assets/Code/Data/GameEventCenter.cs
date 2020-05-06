

// all game events are held here (as static data, so it can be accessed across scenes)
// note events are triggered and handled ENTIRELY programmatically (via listeners and invocations)
public static class GameEventCenter
{
    public static GameEvent<string>     enemyHit    = new GameEvent<string>();
    public static GameEvent<int>        enemyKilled = new GameEvent<int>();
    public static GameEvent<PlayerInfo> scoreChange = new GameEvent<PlayerInfo>();

    public static GameEvent<GameSettingsInfo> startNewGame = new GameEvent<GameSettingsInfo>();
    public static GameEvent<PlayerInfo>       pauseGame    = new GameEvent<PlayerInfo>();
    public static GameEvent<string>           resumeGame   = new GameEvent<string>();
    public static GameEvent<string>           gotoMainMenu = new GameEvent<string>();
    public static GameEvent<PlayerInfo>       gameOver     = new GameEvent<PlayerInfo>();
    public static GameEvent<string>           restartGame  = new GameEvent<string>();
}
