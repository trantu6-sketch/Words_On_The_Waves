using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.Gameplay
{
    public class DecorManager : MonoBehaviour
    {
        public static DecorManager Instance { get; private set; }

        [Header("Chế độ Editor (Bật true nếu nằm trong Decor Scene)")]
        public bool isEditorMode = false;

        [System.Serializable]
        public struct DecorPrefabMapping
        {
            public string decorID;
            public GameObject prefab;
        }

        [Header("Trạng thái Hoạt động (Bật bởi FSM)")]
        public bool isActive = false;

        [Header("Danh sách đồ vật 3D (Cắm Prefab vào đây)")]
        public List<DecorPrefabMapping> decorPrefabs;
        
        // Danh sách các slot đang có trong scene
        private List<DecorSlot> allSlots = new List<DecorSlot>();

        // Variables for UI Drag & Drop
        private GameObject currentGhost;
        private string draggingDecorID;
        private DecorSlot currentHoveredSlot;
        private Quaternion draggingPrefabLocalRotation;
        private bool isDragging = false;

        private void Awake()
        {
            // Luôn lấy DecorManager của Scene mới nhất làm hệ thống chính.
            Instance = this;
            
            // Tự động nhận diện Scene để bật/tắt chế độ Editor (Tránh việc user quên check)
            // Giờ gộp chung Scene nên mặc định là true để cho phép kéo thả
            isEditorMode = true;
        }

        private void Start()
        {
            allSlots = new List<DecorSlot>();
            // Tự động tìm tất cả các DecorSlot tàng hình đang được đính trên xe
            allSlots.AddRange(FindObjectsByType<DecorSlot>(FindObjectsSortMode.None));

            // Luôn luôn Load dữ liệu bất kể đang ở bãi biển hay trong phòng Decor
            LoadDecorFromSave();
        }

        private void Update()
        {
            if (!isActive) return;

            // Xử lý kéo lê đồ vật
            if (isDragging && currentGhost != null)
            {
                UpdateDraggingDecor();
            }
        }

        private void LoadDecorFromSave()
        {
            var savedData = DecorSaveSystem.LoadDecor();
            int spawnedCount = 0;
            
            foreach (var slot in allSlots)
            {
                if (string.IsNullOrEmpty(slot.slotID))
                {
                    Debug.LogWarning("DecorManager: CẢNH BÁO - Có một DecorSlot trên xe bị để trống ô 'Slot ID' trong Inspector! Hãy điền tên cho nó (VD: Window_Left).");
                    continue;
                }

                if (savedData.ContainsKey(slot.slotID))
                {
                    string decorID = savedData[slot.slotID];
                    GameObject prefabToSpawn = null;
                    foreach (var mapping in decorPrefabs)
                    {
                        if (mapping.decorID == decorID)
                        {
                            prefabToSpawn = mapping.prefab;
                            break;
                        }
                    }

                    if (prefabToSpawn != null)
                    {
                        GameObject newDecor = Instantiate(prefabToSpawn);
                        DecorItem newDecorItem = newDecor.GetComponent<DecorItem>();
                        newDecorItem.decorID = decorID;
                        
                        // Căn chỉnh tọa độ TRƯỚC (khi Scale còn là 1) để lấy được kích thước gốc của BoxCollider
                        AlignDecorToSlot(newDecor, slot, prefabToSpawn.transform.localRotation);
                        // Sau đó mới PlaceDecor (hàm này sẽ thu nhỏ Scale về 0 để chạy hiệu ứng nảy)
                        slot.PlaceDecor(newDecorItem);
                        spawnedCount++;
                    }
                    else
                    {
                        Debug.LogError($"DecorManager: LỖI - Không tìm thấy Prefab 3D nào có ID là '{decorID}' trong danh sách 'Decor Prefabs' của DecorManager tại Scene này! Vui lòng gán lại.");
                    }
                }
            }
            
            Debug.Log($"DecorManager: Đã đồng bộ thành công {spawnedCount} món đồ trang trí lên xe ở Scene này!");
        }

        public void ActivateDecorMode(bool on)
        {
            isActive = on;
            UpdateSlotHighlights(on);
        }

        private void UpdateSlotHighlights(bool show)
        {
            if (allSlots == null) return;
            foreach (var slot in allSlots)
            {
                if (slot != null && slot.highlightEffect != null)
                {
                    slot.highlightEffect.SetActive(show && !slot.isOccupied);
                }
            }
        }

        private void ForceSpawnDecor(DecorSlot slot, string decorID)
        {
            foreach (var mapping in decorPrefabs)
            {
                if (mapping.decorID == decorID)
                {
                    GameObject newDecor = Instantiate(mapping.prefab);
                    DecorItem item = newDecor.GetComponent<DecorItem>();
                    item.decorID = decorID;
                    
                    // Căn chỉnh tọa độ TRƯỚC (khi Scale còn là 1) để lấy được kích thước gốc của BoxCollider
                    AlignDecorToSlot(newDecor, slot, mapping.prefab.transform.localRotation);
                    // Sau đó mới PlaceDecor (hàm này sẽ thu nhỏ Scale về 0 để chạy hiệu ứng nảy)
                    slot.PlaceDecor(item);
                    break;
                }
            }
        }

        // --- HÀM TÓAN HỌC CĂN CHỈNH GÓC VÀ CHỐNG CHÌM ---
        private void AlignDecorToSlot(GameObject decorObj, DecorSlot slot, Quaternion prefabLocalRotation)
        {
            // 1. Gắn vào tâm slot và giữ góc xoay gốc của FBX (Ví dụ: -90 độ trục X)
            decorObj.transform.position = slot.transform.position;
            decorObj.transform.rotation = slot.transform.rotation * prefabLocalRotation;

            // 2. Chống chìm: Tính điểm thấp nhất của BoxCollider chiếu lên mặt phẳng của Slot
            BoxCollider box = decorObj.GetComponentInChildren<BoxCollider>();
            if (box != null)
            {
                float minProjection = float.MaxValue;
                Vector3 extents = box.size / 2f;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 localCorner = box.center;
                    localCorner.x += ((i & 1) == 0) ? extents.x : -extents.x;
                    localCorner.y += ((i & 2) == 0) ? extents.y : -extents.y;
                    localCorner.z += ((i & 4) == 0) ? extents.z : -extents.z;
                    // Dùng box.transform để lấy tọa độ chuẩn xác dù Collider nằm ở Root hay Child
                    Vector3 worldCorner = box.transform.TransformPoint(localCorner);
                    
                    // Đo khoảng cách chìm theo trục dọc tuyệt đối của thế giới (Vector3.up)
                    // Bỏ qua việc người dùng lỡ xoay Slot nghiêng ngả sai trục Y
                    float projection = Vector3.Dot(worldCorner - slot.transform.position, Vector3.up);
                    if (projection < minProjection) minProjection = projection;
                }
                
                // Đẩy ngược món đồ thẳng đứng lên đúng bằng khoảng cách bị chìm
                decorObj.transform.position -= Vector3.up * minProjection;
            }
        }

        // Gọi khi người chơi bấm nút "Save & Exit" ở DecorScene
        public void SaveAndReturnToMain()
        {
            if (!isEditorMode) return;

            Dictionary<string, string> currentSetup = new Dictionary<string, string>();
            foreach (var slot in allSlots)
            {
                if (slot.isOccupied && slot.currentItem != null)
                {
                    currentSetup.Add(slot.slotID, slot.currentItem.decorID);
                }
            }

            DecorSaveSystem.SaveDecor(currentSetup);
            
            // Trở về Menu chính hoặc Map thông qua FSM
            if (WordsOnTheWaves.FSM.GameStateMachine.Instance != null)
            {
                WordsOnTheWaves.FSM.GameStateMachine.Instance.ChangeState(WordsOnTheWaves.FSM.GameStateMachine.Instance.MainMenuState);
            }
            else
            {
                SceneManager.LoadScene("SampleScene"); 
            }
        }

        // Gọi từ UI khi người chơi bấm nút trang bị trong Inventory
        public void EquipDecor(string decorID)
        {
            GameObject prefabToSpawn = null;
            string requiredSlotType = "";

            foreach (var mapping in decorPrefabs)
            {
                if (mapping.decorID == decorID)
                {
                    prefabToSpawn = mapping.prefab;
                    requiredSlotType = prefabToSpawn.GetComponent<DecorItem>().allowedSlotType;
                    break;
                }
            }

            if (prefabToSpawn == null) 
            {
                Debug.LogWarning("Không tìm thấy Prefab cho: " + decorID);
                return;
            }

            // Tìm Slot phù hợp còn trống trên xe
            DecorSlot targetSlot = null;
            foreach (var slot in allSlots)
            {
                if (!slot.isOccupied && slot.slotType == requiredSlotType)
                {
                    targetSlot = slot;
                    break;
                }
            }

            if (targetSlot != null)
            {
                GameObject newDecor = Instantiate(prefabToSpawn);
                DecorItem item = newDecor.GetComponent<DecorItem>();
                targetSlot.PlaceDecor(item);
                
                Debug.Log($"Đã trang trí {decorID} lên xe thành công!");
                // (Tương lai): Gọi InventoryManager để trừ đi 1 item trong kho
            }
            else
            {
                Debug.LogWarning($"Xe đã hết chỗ đặt loại {requiredSlotType} cho món đồ {decorID}!");
            }
        }

        public void OnDecorRemoved(DecorItem item)
        {
            Debug.Log($"Đã gỡ {item.decorID} cất vào kho!");
            // (Tương lai): Gọi InventoryManager để cộng lại 1 item vào kho
        }
        // ----------------------------------------------------
        // LOGIC KÉO THẢ XUYÊN KHÔNG GIAN (UI TO 3D)
        // ----------------------------------------------------

        public void StartDraggingDecorFromUI(string decorID)
        {
            if (!isActive || !isEditorMode) return;

            draggingDecorID = decorID;
            GameObject prefabToSpawn = null;
            
            foreach (var mapping in decorPrefabs)
            {
                if (mapping.decorID == decorID)
                {
                    prefabToSpawn = mapping.prefab;
                    break;
                }
            }

            if (prefabToSpawn != null)
            {
                currentGhost = Instantiate(prefabToSpawn);
                draggingPrefabLocalRotation = prefabToSpawn.transform.localRotation;
                
                Collider col = currentGhost.GetComponent<Collider>();
                if (col != null) col.enabled = false;
                
                DecorItem item = currentGhost.GetComponent<DecorItem>();
                
                // Bật báo hiệu (Sáng trắng) cho TẤT CẢ các slot trống và đúng loại
                if (item != null) 
                {
                    string requiredType = item.allowedSlotType;
                    foreach (var slot in allSlots)
                    {
                        if (!slot.isOccupied && slot.slotType == requiredType)
                        {
                            if (slot.highlightEffect != null) slot.highlightEffect.SetActive(true);
                            slot.SetHighlight(false); // Gọi hàm mới với 1 tham số
                        }
                    }
                    Destroy(item); 
                }
            }
        }

        public void UpdateDraggingDecor()
        {
            if (currentGhost == null) return;

            // Xác định loại slot món đồ đang yêu cầu
            string requiredSlotType = "";
            foreach (var mapping in decorPrefabs)
            {
                if (mapping.decorID == draggingDecorID)
                {
                    requiredSlotType = mapping.prefab.GetComponent<DecorItem>().allowedSlotType;
                    break;
                }
            }

            // Reset toàn bộ Highlight về màu Trắng (chưa được neo)
            foreach (var slot in allSlots)
            {
                if (!slot.isOccupied && slot.slotType == requiredSlotType)
                {
                    slot.SetHighlight(false);
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            // Quét xuyên qua mọi vật cản (đề phòng xe kéo chắn mất tia quét)
            foreach (var hit in hits)
            {
                DecorSlot slot = hit.collider.GetComponent<DecorSlot>();
                if (slot != null && !slot.isOccupied)
                {
                    if (slot.slotType == requiredSlotType)
                    {
                        // Đổi màu slot này thành Xanh Lá (báo hiệu đã Neo thành công)
                        slot.SetHighlight(true);

                        // HÚT (Snap) & CĂN CHỈNH
                        AlignDecorToSlot(currentGhost, slot, draggingPrefabLocalRotation);
                        currentHoveredSlot = slot;
                        return;
                    }
                }
            }

            // Nếu không trúng slot hợp lệ, bay lơ lửng cách Camera 5m
            currentHoveredSlot = null;
            currentGhost.transform.position = ray.GetPoint(5f); 
        }

        public bool EndDraggingDecor()
        {
            if (currentGhost == null) return false;

            // Tắt báo hiệu của toàn bộ slot
            foreach (var slot in allSlots)
            {
                slot.SetHighlight(false);
            }

            bool success = false;
            
            if (currentHoveredSlot != null)
            {
                // Thả đúng chỗ -> Đổi Ghost thành đồ thật
                Destroy(currentGhost);
                ForceSpawnDecor(currentHoveredSlot, draggingDecorID);
                Debug.Log($"[UI-to-3D] Đã đặt {draggingDecorID} thành công vào {currentHoveredSlot.slotID}");
                success = true;
            }
            else
            {
                // Thả ngoài không trung -> Biến mất
                Destroy(currentGhost);
                Debug.Log("[UI-to-3D] Hủy thao tác kéo thả.");
            }

            currentGhost = null;
            draggingDecorID = "";
            currentHoveredSlot = null;
            
            return success;
        }
    }
}
