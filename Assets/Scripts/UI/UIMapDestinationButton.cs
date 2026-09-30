using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.UI
{
    [RequireComponent(typeof(Button))]
    public class UIMapDestinationButton : MonoBehaviour
    {
        [Header("Mã điểm đến (Vd: Beach, City_Center)")]
        public string destinationId = "Beach";

        [Header("Phí di chuyển")]
        public float travelCost = 15f;

        [Header("Tên Scene tiếp theo (Mặc định: Preparation)")]
        public string nextSceneName = "Preparation";

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(OnDestinationClicked);
            }
        }

        private void OnDestinationClicked()
        {
            if (MapManager.Instance != null)
            {
                MapManager.Instance.SelectMap(destinationId, travelCost);
            }
            else
            {
                // Tự động lưu thẳng vào hệ thống nếu Scene chưa có MapManager
                PlayerPrefs.SetString("SelectedMapId", destinationId);
                PlayerPrefs.SetFloat("SelectedMapCost", travelCost);
                PlayerPrefs.Save();
            }

            Debug.Log($"UIMapDestinationButton: Đã chọn '{destinationId}' -> Chuyển sang trạng thái '{nextSceneName}'");
            
            if (WordsOnTheWaves.FSM.GameStateMachine.Instance != null)
            {
                if (nextSceneName == "Preparation")
                    WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.PreparationState);
                else if (nextSceneName == "Service")
                    WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.ServiceState);
                else
                    WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.PreparationState);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
