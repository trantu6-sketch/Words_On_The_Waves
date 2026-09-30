using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Gameplay;

namespace WordsOnTheWaves.UI
{
    public class UIPreparationScreen : MonoBehaviour
    {
        [Header("Nút Let's Go! (Sẽ tự ẩn khi vào Service)")]
        public Button letsGoButton;

        [Header("Khu vực chứa UI Kho sách (Sẽ tự ẩn khi vào Service)")]
        public GameObject storageUIPanel;

        [Header("Tên Scene tiếp theo (Mặc định: Service)")]
        public string nextSceneName = "Service";

        private void Start()
        {
            if (letsGoButton != null)
            {
                letsGoButton.onClick.AddListener(OnLetsGoClicked);
            }
        }

        private void OnLetsGoClicked()
        {
            ShelfSlot[] allSlots = FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            Dictionary<string, BookGenre> currentSetup = new Dictionary<string, BookGenre>();

            foreach (var slot in allSlots)
            {
                if (slot.isOccupied && !string.IsNullOrEmpty(slot.slotID))
                {
                    currentSetup[slot.slotID] = slot.currentGenre;
                }
            }

            BookPlacementSaveSystem.SavePlacement(currentSetup);

            Debug.Log($"UIPreparationScreen: Đã chốt {currentSetup.Count} cuốn sách -> Bắt đầu bán hàng!");
            WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.ServiceState);
        }

        public void SetServiceMode(bool isServiceMode)
        {
            // Nếu vào Service -> Ẩn Kho (Storage), Ẩn nút Let's Go
            // Giữ nguyên các phần khác (như UI Shelf Counter, Back Button)
            if (storageUIPanel != null) storageUIPanel.SetActive(!isServiceMode);
            if (letsGoButton != null) letsGoButton.gameObject.SetActive(!isServiceMode);
        }
    }
}
