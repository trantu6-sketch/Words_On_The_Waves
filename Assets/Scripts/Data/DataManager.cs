using UnityEngine;

namespace WordsOnTheWaves.Data
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        public GameConfig Config { get; private set; }

        private void Awake()
        {
            Instance = this;
            LoadData();
        }

        private void LoadData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("GameConfig");
            if (jsonFile != null)
            {
                Config = JsonUtility.FromJson<GameConfig>(jsonFile.text);
                Debug.Log("<color=green>Game Config Data Loaded Successfully!</color>");
                
                // Print a few logs to verify
                Debug.Log($"Loaded {Config.locations.Count} locations.");
                Debug.Log($"Loaded {Config.crates.Count} crates.");
            }
            else
            {
                Debug.LogError("Could not find GameConfig.json in Resources folder!");
            }
        }
    }
}
