using UnityEngine;
using DG.Tweening;

namespace WordsOnTheWaves.Gameplay
{
    [System.Serializable]
    public class ServiceMapConfig
    {
        [Header("Tên Map (phải nhập đúng, Vd: Beach, City)")]
        public string mapId;
        public Transform target;
        public Vector3 setup = new Vector3(20f, -45f, 8f);
    }

    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("--- CÁC MỤC TIÊU LÀM TRUNG TÂM (TARGET) ---")]
        public Transform mapTarget;       // Màn hình chọn Map
        public Transform decorTarget;     // Chiếc xe kéo (để trang trí)
        public Transform shelfTarget;     // Chiếc tủ sách (để xếp sách)
        public Transform serviceTarget;   // Quầy đón khách

        [Header("--- THÔNG SỐ: Góc Cúi (X) - Góc Xoay (Y) - Khoảng cách (Z) ---")]
        public Vector3 mapSetup = new Vector3(30f, 45f, 15f);
        public Vector3 decorSetup = new Vector3(15f, -30f, 6f);
        public Vector3 shelfSetup = new Vector3(10f, 180f, 4f); // 180 độ để nhìn mặt trước
        public Vector3 serviceSetup = new Vector3(20f, -45f, 8f);

        [Header("--- DANH SÁCH CÁC MAP BÁN HÀNG ---")]
        public System.Collections.Generic.List<ServiceMapConfig> serviceMaps;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Chỉ chạy khi game đang Play
            if (Application.isPlaying && WordsOnTheWaves.FSM.GameStateMachine.Instance != null)
            {
                var currentState = WordsOnTheWaves.FSM.GameStateMachine.Instance.CurrentState;
                if (currentState == WordsOnTheWaves.FSM.GameStateMachine.Instance.PreparationState) MoveToShelf();
                else if (currentState == WordsOnTheWaves.FSM.GameStateMachine.Instance.DecorState) MoveToDecor();
                else if (currentState == WordsOnTheWaves.FSM.GameStateMachine.Instance.ServiceState) MoveToService();
                else if (currentState == WordsOnTheWaves.FSM.GameStateMachine.Instance.MapState || 
                         currentState == WordsOnTheWaves.FSM.GameStateMachine.Instance.MainMenuState) MoveToDefault();
            }
        }
#endif

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void MoveToTarget(Transform target, Vector3 setup)
        {
            if (target == null) return;
            
            float pitch = setup.x;
            float yaw = setup.y;
            float distance = setup.z;

            // Tính toán vị trí Camera dựa trên góc xoay và khoảng cách
            Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 targetPosition = target.position + targetRotation * new Vector3(0, 0, -distance);

            // Dừng mọi chuyển động trước đó và bay mượt mà đến vị trí mới trong 1 giây
            transform.DOKill();
            transform.DOMove(targetPosition, 1f).SetEase(Ease.InOutCubic);
            
            // Xoay Camera để nhìn chằm chằm vào Target (và giữ đúng góc Pitch/Yaw)
            transform.DORotateQuaternion(targetRotation, 1f).SetEase(Ease.InOutCubic);
        }

        public void MoveToDefault() => MoveToTarget(mapTarget, mapSetup);
        public void MoveToDecor() => MoveToTarget(decorTarget, decorSetup);
        public void MoveToShelf() => MoveToTarget(shelfTarget, shelfSetup);
        
        public void MoveToService() 
        {
            string selectedMap = WordsOnTheWaves.Data.MapManager.Instance != null 
                ? WordsOnTheWaves.Data.MapManager.Instance.SelectedMapId 
                : PlayerPrefs.GetString("SelectedMapId", "");

            Debug.Log($"[CameraController] Đang chuẩn bị bay đến Map. ID được chọn trong MapManager là: '{selectedMap}'");

            if (serviceMaps != null && serviceMaps.Count > 0)
            {
                foreach (var config in serviceMaps)
                {
                    if (config.mapId == selectedMap)
                    {
                        Debug.Log($"[CameraController] TÌM THẤY Map hợp lệ: '{config.mapId}'. Đang bay đến vị trí đích!");
                        MoveToTarget(config.target, config.setup);
                        return; // Bay đến vị trí của Map được chọn rồi thì dừng
                    }
                }
            }
            
            Debug.LogWarning($"[CameraController] KHÔNG TÌM THẤY ID Map nào là '{selectedMap}' trong danh sách Service Maps của CameraController. Đang bay đến vị trí dự phòng (Service Target). Nếu vị trí dự phòng để trống (None), Camera sẽ KHÔNG di chuyển!");
            // Nếu không tìm thấy Map tương ứng trong danh sách, dùng vị trí cũ làm dự phòng
            MoveToTarget(serviceTarget, serviceSetup);
        }
    }
}
