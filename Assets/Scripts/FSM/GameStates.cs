using UnityEngine;
using WordsOnTheWaves.UI;

namespace WordsOnTheWaves.FSM
{
    public class MainMenuState : GameState
    {
        public MainMenuState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            Debug.Log("FSM: Entered MainMenu State");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowScreen("MainMenu");
            }
        }
    }

    public class CargoState : GameState
    {
        public CargoState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            Debug.Log("FSM: Entered Cargo State");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowScreen("Cargo");
            }
        }
    }

    public class PreparationState : GameState
    {
        public PreparationState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            Debug.Log("FSM: Entered Preparation State");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowScreen("Preparation");
            }
        }
    }

    public class ServiceState : GameState
    {
        public ServiceState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            Debug.Log("FSM: Entered Service State");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowScreen("Service");
            }
        }
    }

    public class IdleState : GameState
    {
        public IdleState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            Debug.Log("FSM: Entered Gameplay (Idle) State - HUD is active");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseAllScreens();
            }
        }
    }
}
