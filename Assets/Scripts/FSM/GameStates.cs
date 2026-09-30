using UnityEngine;
using WordsOnTheWaves.UI;

namespace WordsOnTheWaves.FSM
{
    public class MainMenuState : GameState
    {
        public MainMenuState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            Debug.Log("FSM: Entered MainMenu State (Intro)");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseAllScreens();
                UIManager.Instance.ShowScreen("MainMenu");
            }
            if (WordsOnTheWaves.Gameplay.CameraController.Instance != null) WordsOnTheWaves.Gameplay.CameraController.Instance.MoveToDefault();
        }
    }

    public class MapState : GameState
    {
        public MapState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            Debug.Log("FSM: Entered Map State");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseAllScreens();
                UIManager.Instance.ShowScreen("Map");
            }
            if (WordsOnTheWaves.Gameplay.CameraController.Instance != null) WordsOnTheWaves.Gameplay.CameraController.Instance.MoveToDefault();
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
            if (WordsOnTheWaves.Gameplay.CameraController.Instance != null) WordsOnTheWaves.Gameplay.CameraController.Instance.MoveToShelf();
            
            if (UIManager.Instance != null)
            {
                // Tắt các UI cũ ngay lập tức
                UIManager.Instance.CloseAllScreens();
                // Đợi 1 giây để Camera bay đến nơi rồi mới bật UI
                DG.Tweening.DOVirtual.DelayedCall(1f, () => {
                    if (GameStateMachine.Instance.CurrentState == this)
                    {
                        UIManager.Instance.ShowScreen("Preparation");
                        var prepUI = Object.FindAnyObjectByType<UIPreparationScreen>(FindObjectsInactive.Include);
                        if (prepUI != null) prepUI.SetServiceMode(false); // Bật mode Preparation
                        if (WordsOnTheWaves.Gameplay.DragManager.Instance != null) WordsOnTheWaves.Gameplay.DragManager.Instance.isActive = true;
                    }
                });
            }
        }

        public override void Exit()
        {
            if (WordsOnTheWaves.Gameplay.DragManager.Instance != null) WordsOnTheWaves.Gameplay.DragManager.Instance.isActive = false;
        }
    }

    public class DecorState : GameState
    {
        public DecorState(GameStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            Debug.Log("FSM: Entered Decor State");
            if (WordsOnTheWaves.Gameplay.CameraController.Instance != null) WordsOnTheWaves.Gameplay.CameraController.Instance.MoveToDecor();
            
            if (UIManager.Instance != null)
            {
                // Tắt các UI cũ ngay lập tức
                UIManager.Instance.CloseAllScreens();
                DG.Tweening.DOVirtual.DelayedCall(1f, () => {
                    if (GameStateMachine.Instance.CurrentState == this)
                    {
                        UIManager.Instance.ShowScreen("Decor");
                        if (WordsOnTheWaves.Gameplay.DecorManager.Instance != null) WordsOnTheWaves.Gameplay.DecorManager.Instance.ActivateDecorMode(true);
                    }
                });
            }
        }

        public override void Exit()
        {
            if (WordsOnTheWaves.Gameplay.DecorManager.Instance != null) WordsOnTheWaves.Gameplay.DecorManager.Instance.ActivateDecorMode(false);
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
                UIManager.Instance.CloseAllScreens();
                UIManager.Instance.ShowScreen("Preparation"); // Dùng chung UI với Preparation
                var prepUI = Object.FindAnyObjectByType<UIPreparationScreen>(FindObjectsInactive.Include);
                if (prepUI != null) prepUI.SetServiceMode(true); // Bật mode Service (Ẩn kho)
            }
            if (WordsOnTheWaves.Gameplay.CameraController.Instance != null) WordsOnTheWaves.Gameplay.CameraController.Instance.MoveToService();
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
