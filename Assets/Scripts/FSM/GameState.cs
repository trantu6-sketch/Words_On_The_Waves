namespace WordsOnTheWaves.FSM
{
    public abstract class GameState
    {
        protected GameStateMachine StateMachine;

        public GameState(GameStateMachine stateMachine)
        {
            this.StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }
}
