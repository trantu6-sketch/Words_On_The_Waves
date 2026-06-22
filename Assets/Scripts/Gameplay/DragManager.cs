using UnityEngine;
using System.Collections.Generic;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.Gameplay
{
    public class DragManager : MonoBehaviour
    {
        public static DragManager Instance { get; private set; }
        
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
        public float spawnDistanceFromCamera = 15f;

        private DraggableItem currentlyDragging;
        private ShelfSlot currentHoveredSlot;
        private Plane dragPlane;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
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
        }

        // Gọi hàm này từ UI Event Trigger (PointerDown)
        public void SpawnBookFromUI(string genreString)
        {
            if (System.Enum.TryParse(genreString, out BookGenre genre))
            {
                if (InventoryManager.Instance.RemoveBook(genre, 1))
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

                    if (prefabToSpawn == null) return;

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
