using PQ.Common;


namespace PQ
{
    // todo: find a better place for this
    public struct HorizontalInput
    {
        public enum Type { None, Left, Right }
        public readonly Type value;
        public HorizontalInput(Type value)
        {
            this.value = value;
        }
    }

    // todo: replace this with GameEventRegistries for UI, Commands, etc

    /*
    All game events are held here (as static data, so it can be accessed across scenes).

    Note that events are triggered and handled ENTIRELY programmatically (via listeners and invocations).
    */
    public class GameEventCenter
    {
        public GameEvent<HorizontalInput>       movementInputChanged = new("command.movement");
        public GameEvent<IEventPayload.Empty>   jumpCommand          = new("command.jump");
        public GameEvent<IEventPayload.Empty>   lieDownCommand       = new("command.lieDown");
        public GameEvent<IEventPayload.Empty>   standUpCommand       = new("command.standUp");
        public GameEvent<IEventPayload.Empty>   useCommand           = new("command.use");
        public GameEvent<IEventPayload.Empty>   fireCommand          = new("command.fire");

        public GameEvent<PlayerProgressionInfo> scoreChange          = new("score.changed");
        public GameEvent<PlayerSettingsInfo>    startNewGame         = new("game.new");
        public GameEvent<PlayerProgressionInfo> pauseGame            = new("game.pause");
        public GameEvent<PlayerProgressionInfo> gameOver             = new("game.over");
        public GameEvent<IEventPayload.Empty>   resumeGame           = new("game.resume");
        public GameEvent<IEventPayload.Empty>   gotoMainMenu         = new("game.openMainMenu");
        public GameEvent<IEventPayload.Empty>   restartGame          = new("game.restart");

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
