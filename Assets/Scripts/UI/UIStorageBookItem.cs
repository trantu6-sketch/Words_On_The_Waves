using UnityEngine;
using TMPro;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Gameplay;

namespace WordsOnTheWaves.UI
{
    public class UIStorageBookItem : MonoBehaviour
    {
        public BookGenre genre;
        public TextMeshProUGUI countText;

        private void Start()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.inventoryChangedEvent += UpdateUI;
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.inventoryChangedEvent -= UpdateUI;
            }
        }

        public void UpdateUI()
        {
            if (InventoryManager.Instance != null && countText != null)
            {
                int count = InventoryManager.Instance.GetBookCount(genre);
                countText.text = count.ToString();
            }
        }

        public void OnPointerDownSpawnBook()
        {
            Debug.Log($"UIStorageBookItem: Nhấn chuột vào Icon '{genre}'!");
            if (DragManager.Instance != null)
            {
                DragManager.Instance.SpawnBookFromUI(genre.ToString());
            }
            else
            {
                Debug.LogError("UIStorageBookItem: LỖI - Không tìm thấy DragManager.Instance trong Scene!");
            }
        }
    }
}
