

namespace PenguinQuest.Data
{
    /*
    All game events are held here (as static data, so it can be accessed across scenes).

    Note that events are triggered and handled ENTIRELY programmatically (via listeners and invocations).
    */
    public static class GlobalGameEventCenter
    {
        public static GameEvent<string>           enemyHit    = new GameEvent<string>();
        public static GameEvent<int>              enemyKilled = new GameEvent<int>();
        public static GameEvent<PlayerStatsInfo>  scoreChange = new GameEvent<PlayerStatsInfo>();

        public static GameEvent<GameSettingsInfo> startNewGame = new GameEvent<GameSettingsInfo>();
        public static GameEvent<PlayerStatsInfo>  pauseGame    = new GameEvent<PlayerStatsInfo>();
        public static GameEvent<string>           resumeGame   = new GameEvent<string>();
        public static GameEvent<string>           gotoMainMenu = new GameEvent<string>();
        public static GameEvent<PlayerStatsInfo>  gameOver     = new GameEvent<PlayerStatsInfo>();
        public static GameEvent<string>           restartGame  = new GameEvent<string>();
    }
}
