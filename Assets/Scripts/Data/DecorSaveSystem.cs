using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace WordsOnTheWaves.Data
{
    [System.Serializable]
    public class DecorSaveData
    {
        [System.Serializable]
        public struct SlotData
        {
            public string slotID;
            public string decorID;
        }
        
        public List<SlotData> savedSlots = new List<SlotData>();
    }

    public static class DecorSaveSystem
    {
        // Đường dẫn file Save an toàn trên mọi thiết bị (PC, Android, iOS)
        private static string SavePath => Path.Combine(Application.persistentDataPath, "decorSave.json");

        public static void SaveDecor(Dictionary<string, string> currentDecor)
        {
            DecorSaveData data = new DecorSaveData();
            foreach (var kvp in currentDecor)
            {
                data.savedSlots.Add(new DecorSaveData.SlotData { slotID = kvp.Key, decorID = kvp.Value });
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[DecorSaveSystem] Đã lưu thiết kế xe kéo thành công tại: {SavePath}");
        }

        public static Dictionary<string, string> LoadDecor()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                DecorSaveData data = JsonUtility.FromJson<DecorSaveData>(json);
                if (data != null && data.savedSlots != null)
                {
                    foreach (var slot in data.savedSlots)
                    {
                        result[slot.slotID] = slot.decorID;
                    }
                }
                Debug.Log($"[DecorSaveSystem] Đã tải thành công {result.Count} món đồ từ file Save.");
            }
            else
            {
                Debug.Log("[DecorSaveSystem] Không tìm thấy file Save cũ, bắt đầu với xe trống.");
            }
            return result;
        }
    }
}
