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

        public ShelfSlot currentSlot;
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
                slot.PlaceBook(genre, this);

                Vector3 basePos = slot.transform.position;
                if (slot.TryGetComponent<BoxCollider>(out BoxCollider slotBox))
                {
                    basePos.y = slotBox.bounds.min.y;
                }
                basePos += snapOffset;

                Quaternion targetRot = slot.transform.rotation;

                // Đưa tạm sách về vị trí sàn kệ để đo va chạm chính xác 100%
                Vector3 oldPos = transform.position;
                Quaternion oldRot = transform.rotation;
                transform.position = basePos;
                transform.rotation = targetRot;

                Vector3 finalPos = basePos;

                if (myCollider is BoxCollider box)
                {
                    float minProjection = float.MaxValue;
                    Vector3 extents = box.size / 2f;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 localCorner = box.center;
                        localCorner.x += ((i & 1) == 0) ? extents.x : -extents.x;
                        localCorner.y += ((i & 2) == 0) ? extents.y : -extents.y;
                        localCorner.z += ((i & 4) == 0) ? extents.z : -extents.z;
                        Vector3 worldCorner = transform.TransformPoint(localCorner);

                        float projection = Vector3.Dot(worldCorner - basePos, slot.transform.up);
                        if (projection < minProjection) minProjection = projection;
                    }

                    finalPos -= slot.transform.up * minProjection;
                }

                transform.position = oldPos;
                transform.rotation = oldRot;

                transform.DOKill();
                transform.DOMove(finalPos, 0.2f).SetEase(Ease.OutQuad);
                transform.DORotateQuaternion(targetRot, 0.2f);
                
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
