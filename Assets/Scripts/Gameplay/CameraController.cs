using UnityEngine;
using DG.Tweening;

namespace WordsOnTheWaves.Gameplay
{
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
        public void MoveToService() => MoveToTarget(serviceTarget, serviceSetup);
    }
}
