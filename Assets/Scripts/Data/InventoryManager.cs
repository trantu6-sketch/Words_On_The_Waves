using System.Collections.Generic;
using UnityEngine;

namespace WordsOnTheWaves.Data
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        // Quản lý số lượng tồn kho của từng loại sách
        private Dictionary<BookGenre, int> _bookStorage = new Dictionary<BookGenre, int>();

        // Event báo hiệu kho có sự thay đổi
        public delegate void OnInventoryChanged();
        public event OnInventoryChanged inventoryChangedEvent;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitStorage();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitStorage()
        {
            foreach (BookGenre genre in System.Enum.GetValues(typeof(BookGenre)))
            {
                // Load từ PlayerPrefs. Nếu chưa từng chơi, cho ngẫu nhiên 7-10 cuốn
                int defaultValue = Random.Range(7, 11);
                _bookStorage[genre] = PlayerPrefs.GetInt("BookCount_" + genre.ToString(), defaultValue);
            }
        }

        public void AddBook(BookGenre genre, int amount)
        {
            if (_bookStorage.ContainsKey(genre))
            {
                _bookStorage[genre] += amount;
                PlayerPrefs.SetInt("BookCount_" + genre.ToString(), _bookStorage[genre]);
                inventoryChangedEvent?.Invoke();
                Debug.Log($"Nhập {amount} cuốn {genre}. Tồn kho: {_bookStorage[genre]}");
            }
        }

        public bool RemoveBook(BookGenre genre, int amount)
        {
            if (_bookStorage.ContainsKey(genre) && _bookStorage[genre] >= amount)
            {
                _bookStorage[genre] -= amount;
                PlayerPrefs.SetInt("BookCount_" + genre.ToString(), _bookStorage[genre]);
                inventoryChangedEvent?.Invoke();
                return true;
            }
            return false;
        }

        public int GetBookCount(BookGenre genre)
        {
            return _bookStorage.ContainsKey(genre) ? _bookStorage[genre] : 0;
        }
    }
}
