

namespace PenguinQuest.Data
{
    /*
    All game events are held here (as static data, so it can be accessed across scenes).

    Note that events are triggered and handled ENTIRELY programmatically (via listeners and invocations).
    */
    public static class GameEventCenter
    {
        public static GameEvent<string>           jumpCommand                = new GameEvent<string>();
        public static GameEvent<int>              startHorizontalMoveCommand = new GameEvent<int>();
        public static GameEvent<string>           stopHorizontalMoveCommand  = new GameEvent<string>();
        public static GameEvent<string>           lieDownCommand             = new GameEvent<string>();
        public static GameEvent<string>           standupCommand             = new GameEvent<string>();
        public static GameEvent<string>           useCommand                 = new GameEvent<string>();
        public static GameEvent<string>           fireCommand                = new GameEvent<string>();
        
        public static GameEvent<string>           enemyHit                   = new GameEvent<string>();
        public static GameEvent<int>              enemyKilled                = new GameEvent<int>();
        public static GameEvent<PlayerStatsInfo>  scoreChange                = new GameEvent<PlayerStatsInfo>();
        
        public static GameEvent<GameSettingsInfo> startNewGame               = new GameEvent<GameSettingsInfo>();
        public static GameEvent<PlayerStatsInfo>  pauseGame                  = new GameEvent<PlayerStatsInfo>();
        public static GameEvent<string>           resumeGame                 = new GameEvent<string>();
        public static GameEvent<string>           gotoMainMenu               = new GameEvent<string>();
        public static GameEvent<PlayerStatsInfo>  gameOver                   = new GameEvent<PlayerStatsInfo>();
        public static GameEvent<string>           restartGame                = new GameEvent<string>();
    }
}
