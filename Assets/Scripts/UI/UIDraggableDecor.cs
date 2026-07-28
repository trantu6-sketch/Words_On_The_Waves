using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WordsOnTheWaves.UI
{
    public class UIDraggableDecor : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Tên món đồ (Khớp với DecorManager)")]
        public string decorID;
        
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Vector3 originalPos;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalPos = rectTransform.position;
            canvasGroup.alpha = 0.5f; // Làm mờ đi
            canvasGroup.blocksRaycasts = false; // Xuyên thủng chuột để bắt được các UI bên dưới

            if (Gameplay.DecorManager.Instance != null)
            {
                Gameplay.DecorManager.Instance.StartDraggingDecorFromUI(decorID);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Di chuyển Icon bay theo con chuột trên màn hình 2D
            rectTransform.position = Input.mousePosition;

            if (Gameplay.DecorManager.Instance != null)
            {
                Gameplay.DecorManager.Instance.UpdateDraggingDecor();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            if (Gameplay.DecorManager.Instance != null)
            {
                Gameplay.DecorManager.Instance.EndDraggingDecor();
            }

            // Dù đặt thành công hay thất bại, icon vẫn giật lùi về đúng ô vuông trong Inventory
            rectTransform.position = originalPos;
        }
    }
}
