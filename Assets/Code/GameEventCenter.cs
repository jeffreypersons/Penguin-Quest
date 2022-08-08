using PQ.Common;


namespace PQ
{
    /*
    All game events are held here (as static data, so it can be accessed across scenes).

    Note that events are triggered and handled ENTIRELY programmatically (via listeners and invocations).
    */
    public static class GameEventCenter
    {
        public static GameEvent<string>                 jumpCommand                = new();
        public static GameEvent<int>                    startHorizontalMoveCommand = new();
        public static GameEvent<string>                 stopHorizontalMoveCommand  = new();
        public static GameEvent<string>                 lieDownCommand             = new();
        public static GameEvent<string>                 standUpCommand             = new();
        public static GameEvent<string>                 useCommand                 = new();
        public static GameEvent<string>                 fireCommand                = new();

        public static GameEvent<string>                 enemyHit                   = new();
        public static GameEvent<int>                    enemyKilled                = new();
        public static GameEvent<PlayerProgressionInfo>  scoreChange                = new();
        
        public static GameEvent<PlayerSettingsInfo>     startNewGame               = new();
        public static GameEvent<PlayerProgressionInfo>  pauseGame                  = new();
        public static GameEvent<string>                 resumeGame                 = new();
        public static GameEvent<string>                 gotoMainMenu               = new();
        public static GameEvent<PlayerProgressionInfo>  gameOver                   = new();
        public static GameEvent<string>                 restartGame                = new();
    }
}
