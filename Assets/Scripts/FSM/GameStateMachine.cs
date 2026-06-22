using UnityEngine;

namespace WordsOnTheWaves.FSM
{
    public class GameStateMachine : MonoBehaviour
    {
        public static GameStateMachine Instance { get; private set; }

        public GameState CurrentState { get; private set; }

        public MainMenuState MainMenuState { get; private set; }
        public CargoState CargoState { get; private set; }
        public PreparationState PreparationState { get; private set; }
        public ServiceState ServiceState { get; private set; }
        public IdleState IdleState { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Initialize states
            MainMenuState = new MainMenuState(this);
            CargoState = new CargoState(this);
            PreparationState = new PreparationState(this);
            ServiceState = new ServiceState(this);
            IdleState = new IdleState(this);
        }

        private void Start()
        {
            // Start at Main Menu
            ChangeState(MainMenuState);
        }

        private void Update()
        {
            CurrentState?.Update();
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState != null)
            {
                CurrentState.Exit();
            }

            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}
