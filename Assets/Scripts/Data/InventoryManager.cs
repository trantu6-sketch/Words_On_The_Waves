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
                _bookStorage[genre] = 0;
            }
        }

        public void AddBook(BookGenre genre, int amount)
        {
            if (_bookStorage.ContainsKey(genre))
            {
                _bookStorage[genre] += amount;
                inventoryChangedEvent?.Invoke();
                Debug.Log($"Nhập {amount} cuốn {genre}. Tồn kho: {_bookStorage[genre]}");
            }
        }

        public bool RemoveBook(BookGenre genre, int amount)
        {
            if (_bookStorage.ContainsKey(genre) && _bookStorage[genre] >= amount)
            {
                _bookStorage[genre] -= amount;
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
