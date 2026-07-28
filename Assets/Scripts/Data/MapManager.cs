using UnityEngine;

namespace WordsOnTheWaves.Data
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        public string SelectedMapId { get; private set; } = "Beach";
        public float TravelCost { get; private set; } = 10f;

        private void Awake()
        {
            Instance = this;
            LoadSelectedMap();
        }

        public void SelectMap(string mapId, float cost)
        {
            SelectedMapId = mapId;
            TravelCost = cost;
            
            // Lưu vào PlayerPrefs để truyền sang các Scene sau mà không sợ lỗi DontDestroyOnLoad
            PlayerPrefs.SetString("SelectedMapId", mapId);
            PlayerPrefs.SetFloat("SelectedMapCost", cost);
            PlayerPrefs.Save();

            Debug.Log($"MapManager: Đã chọn điểm đến '{mapId}' với phí di chuyển: {cost}");
        }

        public void LoadSelectedMap()
        {
            SelectedMapId = PlayerPrefs.GetString("SelectedMapId", "Beach");
            TravelCost = PlayerPrefs.GetFloat("SelectedMapCost", 10f);
        }
    }
}
