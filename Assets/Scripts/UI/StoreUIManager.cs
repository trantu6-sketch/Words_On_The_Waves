using UnityEngine;
using System.Collections.Generic;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.UI
{
    public class StoreUIManager : MonoBehaviour
    {
        [Header("Danh sách các ô Kiện Hàng trong UI")]
        public List<CrateUISlot> crateSlots;
        
        [Header("Mô tả tùy chỉnh (Tương ứng với thứ tự JSON)")]
        public List<string> customDescriptions;

        private void OnEnable()
        {
            // Tự động load dữ liệu khi Panel Store được bật lên
            PopulateStore();
        }

        public void PopulateStore()
        {
            if (DataManager.Instance == null || DataManager.Instance.Config == null) return;

            var crates = DataManager.Instance.Config.crates;
            
            for (int i = 0; i < crateSlots.Count; i++)
            {
                if (crateSlots[i] == null) continue;

                if (i < crates.Count)
                {
                    crateSlots[i].gameObject.SetActive(true);
                    
                    string desc = (i < customDescriptions.Count) ? customDescriptions[i] : "Chứa sách ngẫu nhiên.";
                    
                    // Cài đặt dữ liệu cho slot
                    crateSlots[i].Setup(crates[i], desc);
                }
                else
                {
                    // Ẩn các ô thừa nếu JSON chỉ có 3 kiện hàng mà UI làm sẵn 4 ô
                    crateSlots[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
