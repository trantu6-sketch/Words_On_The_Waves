using UnityEngine;

namespace WordsOnTheWaves.Data
{
    public class CargoManager : MonoBehaviour
    {
        public static CargoManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public bool BuyCrate(string crateId)
        {
            if (DataManager.Instance == null || DataManager.Instance.Config == null) return false;

            CrateData crateData = DataManager.Instance.Config.crates.Find(c => c.id == crateId);
            if (crateData == null)
            {
                Debug.LogError($"Không tìm thấy thùng hàng ID: {crateId}");
                return false;
            }

            if (PlayerManager.Instance.SpendCash(crateData.cost))
            {
                Debug.Log($"CargoManager: Đã mua thành công {crateData.name}");
                OpenCrate(crateData);
                return true;
            }
            else
            {
                Debug.LogWarning("CargoManager: Ví không đủ tiền mua thùng này!");
                return false;
            }
        }

        private void OpenCrate(CrateData crateData)
        {
            // Theo GDD: Random số lượng sách (tạm cấu hình 1-5 cuốn mỗi lần đập hộp)
            int booksToGenerate = Random.Range(1, 6);

            for (int i = 0; i < booksToGenerate; i++)
            {
                int totalRate = 0;
                foreach (var drop in crateData.dropRates) totalRate += drop.rate;

                int randomVal = Random.Range(0, totalRate);
                int currentThreshold = 0;
                
                foreach (var drop in crateData.dropRates)
                {
                    currentThreshold += drop.rate;
                    if (randomVal < currentThreshold)
                    {
                        if (System.Enum.TryParse(drop.genre, out BookGenre genreEnum))
                        {
                            InventoryManager.Instance.AddBook(genreEnum, 1);
                        }
                        break;
                    }
                }
            }
        }
    }
}
