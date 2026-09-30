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
            System.Collections.Generic.HashSet<string> seenSlots = new System.Collections.Generic.HashSet<string>();
            
            int totalOnShelf = 0;
            foreach (var slot in allSlots)
            {
                // Bỏ qua nếu là kệ Service
                if (slot.isServiceShelf) continue;
                
                // Bỏ qua nếu đã xử lý slot này rồi (hoặc là slot rác/bị destroy)
                if (slot == null || seenSlots.Contains(slot.slotID)) continue;
                seenSlots.Add(slot.slotID);

                if (slot.isOccupied && slot.currentGenre == genreToCount)
                {
                    totalOnShelf++;
                }
            }

            countText.text = totalOnShelf.ToString();
        }
    }
}
