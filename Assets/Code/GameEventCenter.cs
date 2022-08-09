using PQ.Common;


namespace PQ
{
    /*
    All game events are held here (as static data, so it can be accessed across scenes).

    Note that events are triggered and handled ENTIRELY programmatically (via listeners and invocations).
    */
    public static class GameEventCenter
    {
        public static PQEvent<string>                 jumpCommand                = new();
        public static PQEvent<int>                    startHorizontalMoveCommand = new();
        public static PQEvent<string>                 stopHorizontalMoveCommand  = new();
        public static PQEvent<string>                 lieDownCommand             = new();
        public static PQEvent<string>                 standUpCommand             = new();
        public static PQEvent<string>                 useCommand                 = new();
        public static PQEvent<string>                 fireCommand                = new();

        public static PQEvent<string>                 enemyHit                   = new();
        public static PQEvent<int>                    enemyKilled                = new();
        public static PQEvent<PlayerProgressionInfo>  scoreChange                = new();
        
        public static PQEvent<PlayerSettingsInfo>     startNewGame               = new();
        public static PQEvent<PlayerProgressionInfo>  pauseGame                  = new();
        public static PQEvent<string>                 resumeGame                 = new();
        public static PQEvent<string>                 gotoMainMenu               = new();
        public static PQEvent<PlayerProgressionInfo>  gameOver                   = new();
        public static PQEvent<string>                 restartGame                = new();
    }
}
