using UnityEngine;
using System.Collections.Generic;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.Gameplay
{
    public class DragManager : MonoBehaviour
    {
        public static DragManager Instance { get; private set; }
        
        [Header("Trạng thái Hoạt động (Bật bởi FSM)")]
        public bool isActive = false;

        [System.Serializable]
        public struct BookPrefabMapping
        {
            public BookGenre genre;
            public List<GameObject> prefabs;
        }

        [Header("Gán Prefab cho 7 loại sách")]
        public List<BookPrefabMapping> bookPrefabs;

        [Header("Cấu hình Layer cho hệ thống vật lý")]
        public LayerMask shelfLayer;     // Layer cho các BoxCollider của ô kệ xe
        public LayerMask draggableLayer; // Layer cho sách 3D

        [Header("Khoảng cách rơi ra khi bốc từ UI")]
        public float spawnDistanceFromCamera = 4f;

        private DraggableItem currentlyDragging;
        private ShelfSlot currentHoveredSlot;
        private Plane dragPlane;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            LoadBooksFromSave();
        }

        public void LoadBooksFromSave()
        {
            var savedMap = BookPlacementSaveSystem.LoadPlacement();
            if (savedMap == null || savedMap.Count == 0) return;

            ShelfSlot[] allSlots = FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            System.Collections.Generic.HashSet<string> seenSlots = new System.Collections.Generic.HashSet<string>();

            foreach (var slot in allSlots)
            {
                if (slot.isServiceShelf) continue;
                if (slot == null || seenSlots.Contains(slot.slotID)) continue;
                seenSlots.Add(slot.slotID);

                if (!string.IsNullOrEmpty(slot.slotID) && savedMap.ContainsKey(slot.slotID))
                {
                    BookGenre genre = savedMap[slot.slotID];
                    ForceSpawnOnShelf(slot, genre);
                }
            }
        }

        public void ForceSpawnOnShelf(ShelfSlot slot, BookGenre genre)
        {
            GameObject prefabToSpawn = null;
            foreach (var mapping in bookPrefabs)
            {
                if (mapping.genre == genre && mapping.prefabs != null && mapping.prefabs.Count > 0)
                {
                    prefabToSpawn = mapping.prefabs[0];
                    break;
                }
            }

            if (prefabToSpawn == null) return;

            Vector3 basePos = slot.transform.position;
            if (slot.TryGetComponent<BoxCollider>(out BoxCollider slotBox))
            {
                basePos.y = slotBox.bounds.min.y;
            }

            Quaternion targetRot = slot.transform.rotation;

            GameObject newBook = Instantiate(prefabToSpawn, basePos, targetRot);
            DraggableItem item = newBook.GetComponent<DraggableItem>();
            item.genre = genre;
            item.currentSlot = slot;

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

            slot.PlaceBook(genre, item);
        }

        private void Update()
        {
            if (!isActive) return;

            if (Camera.main == null)
            {
                Debug.LogError("DragManager: LỖI - Camera trong Scene mới chưa được gắn Tag là 'MainCamera'!");
                return;
            }
            HandleInput();
        }

        private void HandleInput()
        {
            // 1. Bắt đầu chạm: Tìm sách đang có sẵn trên kệ 3D để nhấc lên
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, draggableLayer))
                {
                    DraggableItem item = hit.collider.GetComponent<DraggableItem>();
                    if (item != null)
                    {
                        currentlyDragging = item;
                        currentlyDragging.StartDragging();
                        // Mặt phẳng ảo đi qua vị trí sách vuông góc với hướng nhìn camera
                        dragPlane = new Plane(Camera.main.transform.forward, currentlyDragging.transform.position);
                    }
                }
            }

            // 2. Đang rê chuột (Dù sách đó bốc từ kệ hay sinh ra từ UI)
            if (Input.GetMouseButton(0) && currentlyDragging != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                if (dragPlane.Raycast(ray, out float distance))
                {
                    currentlyDragging.transform.position = Vector3.Lerp(currentlyDragging.transform.position, ray.GetPoint(distance), Time.deltaTime * 20f);
                }

                // Phát tia xuyên qua kiểm tra ô kệ (ShelfSlot)
                if (Physics.Raycast(ray, out RaycastHit shelfHit, 100f, shelfLayer))
                {
                    ShelfSlot slot = shelfHit.collider.GetComponent<ShelfSlot>();
                    if (slot != null && !slot.isOccupied)
                    {
                        if (currentHoveredSlot != slot)
                        {
                            if (currentHoveredSlot != null) currentHoveredSlot.ShowPreview(false);
                            currentHoveredSlot = slot;
                            currentHoveredSlot.ShowPreview(true); // Bật hiệu ứng Ghost Preview
                        }
                    }
                }
                else
                {
                    if (currentHoveredSlot != null)
                    {
                        currentHoveredSlot.ShowPreview(false);
                        currentHoveredSlot = null;
                    }
                }
            }

            // 3. Buông tay thả sách
            if (Input.GetMouseButtonUp(0) && currentlyDragging != null)
            {
                if (currentHoveredSlot != null)
                {
                    currentHoveredSlot.ShowPreview(false);
                    currentlyDragging.StopDragging(currentHoveredSlot);
                    currentHoveredSlot = null;
                }
                else
                {
                    currentlyDragging.ReturnToStart();
                }
                
                currentlyDragging = null;
            }

            // 4. Click chuột phải vào sách trên kệ: Hoàn trả vào kho
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, draggableLayer))
                {
                    DraggableItem item = hit.collider.GetComponent<DraggableItem>();
                    if (item != null && item.currentSlot != null)
                    {
                        item.currentSlot.ReturnBookToStorage();
                    }
                }
            }
        }

        // Gọi hàm này từ UI Event Trigger (PointerDown)
        public void SpawnBookFromUI(string genreString)
        {
            if (!isActive) return;
            
            Debug.Log($"DragManager: Nhận lệnh tạo sách thể loại '{genreString}' từ UI");
            if (System.Enum.TryParse(genreString, out BookGenre genre))
            {
                bool canSpawn = true;
                if (InventoryManager.Instance != null)
                {
                    canSpawn = InventoryManager.Instance.RemoveBook(genre, 1);
                }

                if (canSpawn)
                {
                    GameObject prefabToSpawn = null;
                    foreach (var mapping in bookPrefabs)
                    {
                        if (mapping.genre == genre && mapping.prefabs != null && mapping.prefabs.Count > 0)
                        {
                            int randomIndex = Random.Range(0, mapping.prefabs.Count);
                            prefabToSpawn = mapping.prefabs[randomIndex];
                            break;
                        }
                    }

                    if (prefabToSpawn == null)
                    {
                        Debug.LogError($"DragManager: CHƯA GÁN PREFAB sách 3D cho thể loại '{genre}' trong Inspector!");
                        return;
                    }

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Vector3 spawnPos = ray.GetPoint(spawnDistanceFromCamera); 
                    
                    GameObject newBook = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                    DraggableItem item = newBook.GetComponent<DraggableItem>();
                    item.genre = genre;
                    item.spawnedFromUI = true;
                    item.startPosition = spawnPos; 
                    
                    currentlyDragging = item;
                    currentlyDragging.StartDragging();
                    
                    dragPlane = new Plane(Camera.main.transform.forward, spawnPos);
                }
                else
                {
                    Debug.LogWarning("Kho không đủ sách thể loại này!");
                }
            }
        }
    }
}
