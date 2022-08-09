using PQ.Common;


namespace PQ
{
    /*
    All game events are held here (as static data, so it can be accessed across scenes).

    Note that events are triggered and handled ENTIRELY programmatically (via listeners and invocations).
    */
    public class GameEventCenter
    {
        public GameEvent<string>                 jumpCommand                = new();
        public GameEvent<int>                    startHorizontalMoveCommand = new();
        public GameEvent<string>                 stopHorizontalMoveCommand  = new();
        public GameEvent<string>                 lieDownCommand             = new();
        public GameEvent<string>                 standUpCommand             = new();
        public GameEvent<string>                 useCommand                 = new();
        public GameEvent<string>                 fireCommand                = new();

        public GameEvent<string>                 enemyHit                   = new();
        public GameEvent<int>                    enemyKilled                = new();
        public GameEvent<PlayerProgressionInfo>  scoreChange                = new();
        
        public GameEvent<PlayerSettingsInfo>     startNewGame               = new();
        public GameEvent<PlayerProgressionInfo>  pauseGame                  = new();
        public GameEvent<string>                 resumeGame                 = new();
        public GameEvent<string>                 gotoMainMenu               = new();
        public GameEvent<PlayerProgressionInfo>  gameOver                   = new();
        public GameEvent<string>                 restartGame                = new();

        private static GameEventCenter _instance;
        public static GameEventCenter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameEventCenter();
                }
                return _instance;
            }
        }
    }
}
