using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Yêu cầu phải có DOTween trong dự án
using System.Collections;

namespace WordsOnTheWaves.UI
{
    [System.Serializable]
    public class UIScreen
    {
        public string screenName;
        public GameObject screenObj;
        public RectTransform originButtonRect; // Kéo thả nút Button tương ứng vào đây
        [HideInInspector] public Vector3 originalWorldPos;
        [HideInInspector] public Vector3 originalScale;
    }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public List<UIScreen> screens;

        [Header("--- Navigation Buttons ---")]
        public Button menuButton;
        public Button cargoButton;
        public Button preparationButton;
        public Button decorButton;
        public Button serviceButton;
        
        [Header("--- Close Buttons ---")]
        public List<Button> closeButtons;

        private string currentScreen = "";

        private void Awake()
        {
            Instance = this;

            foreach (var screen in screens)
            {
                if (screen.screenObj != null)
                {
                    RectTransform rect = screen.screenObj.GetComponent<RectTransform>();
                    screen.originalWorldPos = rect.position;
                    screen.originalScale = Vector3.one; // Ép cứng luôn bằng 1 để không bao giờ bị dính lỗi thu nhỏ về 0
                    screen.screenObj.SetActive(false); // Ẩn mặc định ban đầu
                }
            }
        }

        private void Start()
        {
            // Register button clicks to FSM State Changes
            if (menuButton != null) menuButton.onClick.AddListener(OnGameplayButtonClicked);
            if (cargoButton != null) cargoButton.onClick.AddListener(OnCargoButtonClicked);
            if (preparationButton != null) preparationButton.onClick.AddListener(OnPreparationButtonClicked);
            if (decorButton != null) decorButton.onClick.AddListener(OnDecorButtonClicked);
            if (serviceButton != null) serviceButton.onClick.AddListener(OnServiceButtonClicked);
            
            // Register multiple close buttons
            if (closeButtons != null)
            {
                foreach (var btn in closeButtons)
                {
                    if (btn != null) btn.onClick.AddListener(OnCloseButtonClicked);
                }
            }
        }

        public void OnGameplayButtonClicked()
        {
            // Từ màn hình Intro, nhấn Play để vào màn hình Map
            WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.MapState);
        }

        public void OnCargoButtonClicked()
        {
            WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.CargoState);
        }

        public void OnPreparationButtonClicked()
        {
            WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.PreparationState);
        }

        public void OnDecorButtonClicked()
        {
            WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.DecorState);
        }

        public void OnServiceButtonClicked()
        {
            WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.ServiceState);
        }

        public void OnCloseButtonClicked()
        {
            // Đưa về màn hình Map (chính) và kéo Camera trở lại
            WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.MapState);
        }

        public void CloseAllScreens()
        {
            currentScreen = "";
            foreach (var screen in screens)
            {
                if (screen.screenObj != null && screen.screenObj.activeSelf)
                {
                    RectTransform panelRect = screen.screenObj.GetComponent<RectTransform>();
                    
                    if (screen.originButtonRect != null)
                    {
                        panelRect.DOKill();
                        panelRect.DOMove(screen.originButtonRect.position, 0.3f).SetEase(Ease.InBack);
                        panelRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                            .OnComplete(() => screen.screenObj.SetActive(false));
                    }
                    else
                    {
                        screen.screenObj.SetActive(false);
                    }
                }
            }
        }

        public void ShowScreen(string screenName)
        {
            Debug.Log($"UIManager: Yêu cầu mở UI -> {screenName}");
            if (currentScreen == screenName) 
            {
                Debug.Log($"UIManager: UI {screenName} đang mở sẵn rồi, bỏ qua!");
                return;
            }
            currentScreen = screenName;

            bool found = false;
            foreach (var screen in screens)
            {
                if (screen.screenObj != null)
                {
                    RectTransform panelRect = screen.screenObj.GetComponent<RectTransform>();

                    if (screen.screenName == screenName)
                    {
                        found = true;
                        Debug.Log($"UIManager: Đã tìm thấy UI {screenName} trong danh sách, tiến hành BẬT (SetActive = true)");
                        screen.screenObj.SetActive(true);
                        
                        if (screen.originButtonRect != null)
                        {
                            panelRect.DOKill();
                            // Đặt vị trí bắt đầu từ nút bấm và thu nhỏ về 0
                            panelRect.position = screen.originButtonRect.position;
                            panelRect.localScale = Vector3.zero;
                            
                            // Phóng to và di chuyển ra giữa màn hình
                            panelRect.DOMove(screen.originalWorldPos, 0.4f).SetEase(Ease.OutBack);
                            panelRect.DOScale(screen.originalScale, 0.4f).SetEase(Ease.OutBack);
                        }
                        else
                        {
                            // Reset lại scale và vị trí về mặc định (chống kẹt scale = 0)
                            panelRect.DOKill();
                            panelRect.position = screen.originalWorldPos;
                            panelRect.localScale = screen.originalScale;
                        }
                    }
                    else
                    {
                        // Đóng các màn hình khác đang mở
                        if (screen.screenObj.activeSelf)
                        {
                            if (screen.originButtonRect != null)
                            {
                                panelRect.DOKill();
                                panelRect.DOMove(screen.originButtonRect.position, 0.3f).SetEase(Ease.InBack);
                                panelRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                                    .OnComplete(() => screen.screenObj.SetActive(false));
                            }
                            else
                            {
                                screen.screenObj.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
    }
}
