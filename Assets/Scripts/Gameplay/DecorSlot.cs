using UnityEngine;
using DG.Tweening;

namespace WordsOnTheWaves.Gameplay
{
    public class DecorSlot : MonoBehaviour
    {
        [Header("ID Duy Nhất (Vd: Window_L1)")]
        public string slotID;

        [Header("Loại vị trí (Vd: Wall, Window, Floor)")]
        public string slotType;

        [Header("Chấm tròn báo hiệu (Kéo 1 Sprite/Hình tròn vào đây)")]
        public GameObject highlightEffect;

        public bool isOccupied { get; private set; }
        public DecorItem currentItem { get; private set; }

        private Vector3 originalHighlightScale;

        private void Start()
        {
            if (highlightEffect != null) 
            {
                originalHighlightScale = highlightEffect.transform.localScale;
                
                // Mặc định luôn hiện chấm trắng nếu đang ở màn hình Decor (Editor) và slot còn trống
                bool isEditor = DecorManager.Instance != null && DecorManager.Instance.isEditorMode;
                highlightEffect.SetActive(isEditor && !isOccupied);
            }
        }

        public void SetHighlight(bool isHovered)
        {
            if (highlightEffect == null || isOccupied) return;

            highlightEffect.transform.DOKill();
            
            if (isHovered)
            {
                // Phóng to lên 1.5 lần tạo hiệu ứng nảy (Snap)
                highlightEffect.transform.DOScale(originalHighlightScale * 1.5f, 0.2f).SetEase(Ease.OutBack);
                
                // Đổi màu thành VÀNG nếu là Sprite
                var sprite = highlightEffect.GetComponent<SpriteRenderer>();
                if (sprite != null) sprite.color = Color.yellow;
                
                // Đổi màu thành VÀNG nếu là Mesh (Quả cầu 3D)
                var mesh = highlightEffect.GetComponent<MeshRenderer>();
                if (mesh != null) mesh.material.color = Color.yellow;
            }
            else
            {
                // Thu nhỏ về kích thước ban đầu
                highlightEffect.transform.DOScale(originalHighlightScale, 0.2f).SetEase(Ease.OutBack);
                
                // Trả về màu trắng nguyên bản
                var sprite = highlightEffect.GetComponent<SpriteRenderer>();
                if (sprite != null) sprite.color = Color.white;
                
                // Trả về màu trắng nguyên bản cho Mesh (Quả cầu 3D)
                var mesh = highlightEffect.GetComponent<MeshRenderer>();
                if (mesh != null) mesh.material.color = Color.white;
            }
        }

        public void PlaceDecor(DecorItem item)
        {
            if (isOccupied) return;
            
            currentItem = item;
            isOccupied = true;
            
            // Ẩn chấm tròn đi vì đã có đồ đè lên
            if (highlightEffect != null) highlightEffect.SetActive(false);
            
            // Gắn vào slot nhưng giữ nguyên vị trí và góc xoay đã được DecorManager căn chỉnh chuẩn xác
            item.transform.SetParent(this.transform, true);
            
            item.AssignToSlot(this);
        }

        public void ClearSlot()
        {
            currentItem = null;
            isOccupied = false;
            
            // Hiện lại chấm tròn nếu gỡ đồ ra trong màn hình Decor
            bool isEditor = DecorManager.Instance != null && DecorManager.Instance.isEditorMode;
            if (highlightEffect != null && isEditor) 
            {
                highlightEffect.SetActive(true);
                highlightEffect.transform.localScale = originalHighlightScale;
            }
        }
    }
}
