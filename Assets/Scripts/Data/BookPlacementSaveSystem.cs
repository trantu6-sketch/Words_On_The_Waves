using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WordsOnTheWaves.Data
{
    [System.Serializable]
    public class BookSlotSaveItem
    {
        public string slotID;
        public string genreString;
    }

    [System.Serializable]
    public class BookPlacementSaveData
    {
        public List<BookSlotSaveItem> placedBooks = new List<BookSlotSaveItem>();
    }

    public class BookPlacementSaveSystem
    {
        private static string savePath = Application.persistentDataPath + "/bookPlacementSave.json";

        // Tự động lưu khi có thay đổi trên kệ
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoSaveInit()
        {
            WordsOnTheWaves.Gameplay.ShelfSlot.OnAnyShelfChanged -= AutoSaveAllSlots;
            WordsOnTheWaves.Gameplay.ShelfSlot.OnAnyShelfChanged += AutoSaveAllSlots;
        }

        private static void AutoSaveAllSlots()
        {
            WordsOnTheWaves.Gameplay.ShelfSlot[] allSlots = Object.FindObjectsByType<WordsOnTheWaves.Gameplay.ShelfSlot>(FindObjectsSortMode.None);
            Dictionary<string, BookGenre> currentSetup = new Dictionary<string, BookGenre>();

            foreach (var slot in allSlots)
            {
                if (!slot.isServiceShelf && slot.isOccupied && !string.IsNullOrEmpty(slot.slotID))
                {
                    currentSetup[slot.slotID] = slot.currentGenre;
                }
            }
            SavePlacement(currentSetup);
        }

        public static void SavePlacement(Dictionary<string, BookGenre> placementMap)
        {
            BookPlacementSaveData data = new BookPlacementSaveData();
            foreach (var kvp in placementMap)
            {
                data.placedBooks.Add(new BookSlotSaveItem
                {
                    slotID = kvp.Key,
                    genreString = kvp.Value.ToString()
                });
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"BookPlacementSaveSystem: Đã lưu {placementMap.Count} vị trí sách vào {savePath}");
        }

        public static Dictionary<string, BookGenre> LoadPlacement()
        {
            Dictionary<string, BookGenre> result = new Dictionary<string, BookGenre>();
            if (!File.Exists(savePath)) return result;

            try
            {
                string json = File.ReadAllText(savePath);
                BookPlacementSaveData data = JsonUtility.FromJson<BookPlacementSaveData>(json);
                if (data != null && data.placedBooks != null)
                {
                    foreach (var item in data.placedBooks)
                    {
                        if (System.Enum.TryParse(item.genreString, out BookGenre genre))
                        {
                            result[item.slotID] = genre;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BookPlacementSaveSystem: Lỗi đọc save: {ex.Message}");
            }

            return result;
        }
    }
}
