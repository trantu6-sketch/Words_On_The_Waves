using UnityEngine;
using WordsOnTheWaves.Data;
using DG.Tweening;

namespace WordsOnTheWaves.Gameplay
{
    public class DraggableItem : MonoBehaviour
    {
        public BookGenre genre;
        public bool isDragging = false;
        public bool spawnedFromUI = false; // Phân biệt sách bốc từ UI ra hay bốc từ trên kệ đi chỗ khác
        public Vector3 startPosition;
        
        [Header("Tinh chỉnh vị trí lúc thả xuống kệ")]
        public Vector3 snapOffset = new Vector3(0, 0, 0); // Thay đổi y để nâng/hạ sách

        private ShelfSlot currentSlot;
        private Collider myCollider;

        private void Awake()
        {
            myCollider = GetComponent<Collider>();
        }

        public void StartDragging()
        {
            isDragging = true;
            if (myCollider != null) myCollider.enabled = false; // Tắt va chạm để Raycast xuyên qua trúng cái kệ bên dưới
            
            if (currentSlot != null)
            {
                currentSlot.RemoveBook();
                currentSlot = null;
            }
            else if (!spawnedFromUI)
            {
                startPosition = transform.position;
            }

            // Animation: Phóng to ra một chút tạo cảm giác bị nhấc lên không trung
            transform.DOKill();
            transform.DOScale(Vector3.one * 1.2f, 0.2f);
        }

        public void StopDragging(ShelfSlot slot)
        {
            isDragging = false;
            if (myCollider != null) myCollider.enabled = true;

            if (slot != null && !slot.isOccupied)
            {
                currentSlot = slot;
                slot.PlaceBook(genre);

                // --- BƯỚC 1: TÌM MẶT ĐÁY CỦA Ô KỆ (SHELF SLOT) ---
                // Thay vì lấy điểm giữa lơ lửng của ô kệ, ta sẽ lấy mặt đáy thấp nhất của nó (chạm gỗ)
                Vector3 targetPos = slot.transform.position;
                BoxCollider slotBox = slot.GetComponent<BoxCollider>();
                if (slotBox != null)
                {
                    targetPos.y = slotBox.bounds.min.y; 
                }
                targetPos += snapOffset;
                
                // --- BƯỚC 2: TÍNH BÙ TRỪ PIVOT CỦA CUỐN SÁCH ---
                if (myCollider is BoxCollider box)
                {
                    float minWorldY = float.MaxValue;
                    Vector3 extents = box.size / 2f;
                    
                    for(int i = 0; i < 8; i++) 
                    {
                        Vector3 localCorner = box.center;
                        localCorner.x += ((i & 1) == 0) ? extents.x : -extents.x;
                        localCorner.y += ((i & 2) == 0) ? extents.y : -extents.y;
                        localCorner.z += ((i & 4) == 0) ? extents.z : -extents.z;
                        
                        Vector3 worldCorner = transform.TransformPoint(localCorner);
                        if(worldCorner.y < minWorldY) minWorldY = worldCorner.y;
                    }
                    
                    float pivotToBottom = transform.position.y - minWorldY;
                    targetPos.y += pivotToBottom; // Nâng cuốn sách lên để mặt đáy sách vừa khít mặt đáy kệ
                }

                transform.DOKill();
                transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutQuad);
                
                // Animation đàn hồi
                Sequence seq = DOTween.Sequence();
                seq.Append(transform.DOScale(new Vector3(1.3f, 0.7f, 1.3f), 0.1f));
                seq.Append(transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack));
                spawnedFromUI = false;
            }
            else
            {
                ReturnToStart();
            }
        }

        public void ReturnToStart()
        {
            transform.DOKill();
            
            // Animation rung lắc báo lỗi và bay ngược về chỗ cũ (Tweenback)
            transform.DOShakeRotation(0.3f, new Vector3(0, 0, 30f));
            transform.DOMove(startPosition, 0.4f).SetEase(Ease.InOutSine).OnComplete(() => 
            {
                // Nếu được kéo từ UI mà thả hụt thì xóa object vật lý đi và cộng lại số lượng vào kho
                if (spawnedFromUI)
                {
                    InventoryManager.Instance.AddBook(genre, 1);
                    Destroy(gameObject);
                }
            });
            transform.DOScale(Vector3.one, 0.2f);
        }
    }
}
