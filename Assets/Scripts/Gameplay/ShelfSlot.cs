using UnityEngine;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.Gameplay
{
    public class ShelfSlot : MonoBehaviour
    {
        public bool isOccupied = false;
        public BookGenre currentGenre;
        
        [Header("ID định danh duy nhất (Vd: Shelf_1_1)")]
        public string slotID;

        [Header("Đánh dấu tích nếu đây là kệ bên Khu Vực Bán Hàng")]
        public bool isServiceShelf = false;

        public DraggableItem currentPlacedItem { get; private set; }

        public static event System.Action OnAnyShelfChanged;

        [Header("Mô hình 3D bán trong suốt để hiển thị xem trước")]
        public GameObject previewMesh;

        private void Awake()
        {
            if (previewMesh != null) previewMesh.SetActive(false);
            
            // Tự động nhận diện kệ Service nếu nó là con của ServiceShopManager
            if (GetComponentInParent<ServiceShopManager>() != null)
            {
                isServiceShelf = true;
            }
        }

        public void ShowPreview(bool show)
        {
            if (isOccupied) return;
            if (previewMesh != null) previewMesh.SetActive(show);
        }

        public void PlaceBook(BookGenre genre, DraggableItem item)
        {
            isOccupied = true;
            currentGenre = genre;
            currentPlacedItem = item;
            ShowPreview(false);
            OnAnyShelfChanged?.Invoke();
        }

        public void RemoveBook()
        {
            isOccupied = false;
            currentPlacedItem = null;
            OnAnyShelfChanged?.Invoke();
        }

        public void ReturnBookToStorage()
        {
            if (!isOccupied) return;

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddBook(currentGenre, 1);
            }

            if (currentPlacedItem != null)
            {
                Destroy(currentPlacedItem.gameObject);
                currentPlacedItem = null;
            }

            isOccupied = false;
            Debug.Log($"ShelfSlot: Đã thu hồi sách {currentGenre} trả lại kho!");
            OnAnyShelfChanged?.Invoke();
        }
    }
}
