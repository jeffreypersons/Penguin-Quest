using PQ.Common;


namespace PQ
{
    // todo: find a better spot for this
    public enum HorizontalInput
    {
        None,
        Left,
        Right
    }

    /*
    All game events are held here (as static data, so it can be accessed across scenes).

    Note that events are triggered and handled ENTIRELY programmatically (via listeners and invocations).
    */
    public class GameEventCenter
    {
        public GameEvent<string>                jumpCommand          = new("command.jump");
        public GameEvent<HorizontalInput>       movementInputChanged = new("command.movement");
        public GameEvent<string>                lieDownCommand       = new("command.lieDown");
        public GameEvent<string>                standUpCommand       = new("command.standUp");
        public GameEvent<string>                useCommand           = new("command.use");
        public GameEvent<string>                fireCommand          = new("command.fire");

        public GameEvent<string>                enemyHit             = new("enemy.hit");
        public GameEvent<int>                   enemyKilled          = new("enemy.kill");
        public GameEvent<PlayerProgressionInfo> scoreChange          = new("score.changed");
        
        public GameEvent<PlayerSettingsInfo>    startNewGame         = new("game.new");
        public GameEvent<PlayerProgressionInfo> pauseGame            = new("game.pause");
        public GameEvent<string>                resumeGame           = new("game.resume");
        public GameEvent<string>                gotoMainMenu         = new("game.openMainMenu");
        public GameEvent<PlayerProgressionInfo> gameOver             = new("game.over");
        public GameEvent<string>                restartGame          = new("game.restart");

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
