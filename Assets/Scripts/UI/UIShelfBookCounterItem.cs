using UnityEngine;
using TMPro;
using WordsOnTheWaves.Data;
using WordsOnTheWaves.Gameplay;

namespace WordsOnTheWaves.UI
{
    public class UIShelfBookCounterItem : MonoBehaviour
    {
        [Header("Thể loại sách cần đếm trên kệ")]
        public BookGenre genreToCount;
        
        [Header("Kéo chữ Text hiển thị số lượng vào đây")]
        public TextMeshProUGUI countText;

        private void OnEnable()
        {
            // Lắng nghe sự kiện mỗi khi có sách được đặt lên hoặc tháo khỏi kệ
            ShelfSlot.OnAnyShelfChanged += UpdateCountUI;
        }

        private void OnDisable()
        {
            ShelfSlot.OnAnyShelfChanged -= UpdateCountUI;
        }

        private void Start()
        {
            // Cập nhật ngay khi game vừa bật
            UpdateCountUI();
        }

        public void UpdateCountUI()
        {
            if (countText == null) return;

            // Tìm tất cả các ShelfSlot đang có trong Scene
            ShelfSlot[] allSlots = FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            
            int totalOnShelf = 0;
            foreach (var slot in allSlots)
            {
                if (!slot.isServiceShelf && slot.isOccupied && slot.currentGenre == genreToCount)
                {
                    totalOnShelf++;
                }
            }

            countText.text = totalOnShelf.ToString();
        }
    }
}
