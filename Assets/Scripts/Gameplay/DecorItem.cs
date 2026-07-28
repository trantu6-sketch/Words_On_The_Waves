using UnityEngine;
using DG.Tweening;

namespace WordsOnTheWaves.Gameplay
{
    public class DecorItem : MonoBehaviour
    {
        public string decorID; // Ví dụ: "Monstera"
        [Header("Loại Khe cắm yêu cầu (Vd: Window)")]
        public string allowedSlotType; 

        private DecorSlot currentSlot;

        public void AssignToSlot(DecorSlot slot)
        {
            currentSlot = slot;
            // Hiệu ứng xuất hiện
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }

        // Bắt sự kiện Click chuột trực tiếp vào 3D model để gỡ
        private void OnMouseDown()
        {
            if (DecorManager.Instance != null && DecorManager.Instance.isEditorMode)
            {
                RemoveFromSlot();
            }
        }

        public void RemoveFromSlot()
        {
            if (currentSlot != null)
            {
                currentSlot.ClearSlot();
                currentSlot = null;
                
                // Thu hồi vào UI
                transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    DecorManager.Instance.OnDecorRemoved(this);
                    Destroy(gameObject);
                });
            }
        }
    }
}
