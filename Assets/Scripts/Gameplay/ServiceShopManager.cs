using System.Collections.Generic;
using UnityEngine;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.Gameplay
{
    public class ServiceShopManager : MonoBehaviour
    {
        [System.Serializable]
        public struct BookPrefabMapping
        {
            public BookGenre genre;
            public GameObject prefab;
        }

        [Header("Gán Prefab sách 3D để bày bán trên xe")]
        public List<BookPrefabMapping> bookPrefabs;

        private void Start()
        {
            LoadPreparedBooks();
        }

        private void LoadPreparedBooks()
        {
            Dictionary<string, BookGenre> savedSetup = BookPlacementSaveSystem.LoadPlacement();
            if (savedSetup == null || savedSetup.Count == 0)
            {
                Debug.LogWarning("ServiceShopManager: Không có dữ liệu sách được chuẩn bị từ màn trước!");
                return;
            }

            ShelfSlot[] allSlots = FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            int spawnedCount = 0;

            foreach (var slot in allSlots)
            {
                if (slot.isServiceShelf && !string.IsNullOrEmpty(slot.slotID) && savedSetup.ContainsKey(slot.slotID))
                {
                    BookGenre genre = savedSetup[slot.slotID];
                    SpawnBookOnSlot(slot, genre);
                    spawnedCount++;
                }
            }

            Debug.Log($"ServiceShopManager: Đã bày bán thành công {spawnedCount} cuốn sách lên xe kéo!");
        }

        private void SpawnBookOnSlot(ShelfSlot slot, BookGenre genre)
        {
            GameObject prefabToSpawn = null;
            foreach (var m in bookPrefabs)
            {
                if (m.genre == genre && m.prefab != null)
                {
                    prefabToSpawn = m.prefab;
                    break;
                }
            }

            if (prefabToSpawn == null)
            {
                Debug.LogError($"ServiceShopManager: Lỗi - Chưa gán Prefab 3D cho thể loại '{genre}'!");
                return;
            }

            Vector3 basePos = slot.transform.position;
            if (slot.TryGetComponent<BoxCollider>(out BoxCollider slotBox))
            {
                basePos.y = slotBox.bounds.min.y;
            }

            Quaternion targetRot = slot.transform.rotation;
            GameObject newBook = Instantiate(prefabToSpawn, basePos, targetRot);

            if (newBook.TryGetComponent<BoxCollider>(out BoxCollider box))
            {
                float minProjection = float.MaxValue;
                Vector3 extents = box.size / 2f;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 localCorner = box.center;
                    localCorner.x += ((i & 1) == 0) ? extents.x : -extents.x;
                    localCorner.y += ((i & 2) == 0) ? extents.y : -extents.y;
                    localCorner.z += ((i & 4) == 0) ? extents.z : -extents.z;
                    Vector3 worldCorner = newBook.transform.TransformPoint(localCorner);

                    float projection = Vector3.Dot(worldCorner - basePos, slot.transform.up);
                    if (projection < minProjection) minProjection = projection;
                }

                newBook.transform.position -= slot.transform.up * minProjection;
            }

            if (newBook.TryGetComponent<DraggableItem>(out DraggableItem dragItem))
            {
                dragItem.enabled = false; // Tắt tính năng kéo thả khi đang mở cửa bán hàng
                slot.PlaceBook(genre, dragItem);
            }
        }
    }
}
