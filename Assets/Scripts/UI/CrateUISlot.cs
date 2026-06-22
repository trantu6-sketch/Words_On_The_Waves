using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WordsOnTheWaves.Data;
using DG.Tweening; // Thêm thư viện DOTween

namespace WordsOnTheWaves.UI
{
    public class CrateUISlot : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI descText;
        
        public Image boxImage;
        public GameObject soldOverlay;
        public GameObject newOverlay;
        public Button buyButton;

        public Sprite boxUnboughtSprite; // Hình box1.png
        public Sprite boxSoldSprite;     // Hình box2.png

        private string currentCrateId;
        private bool isSold = false;

        public void Setup(CrateData crateData, string description)
        {
            currentCrateId = crateData.id;
            isSold = false;

            if (nameText != null) nameText.text = crateData.name.ToUpper();
            if (priceText != null) priceText.text = crateData.cost.ToString();
            if (descText != null) descText.text = description;

            if (boxImage != null && boxUnboughtSprite != null) 
            {
                boxImage.sprite = boxUnboughtSprite;
                // Đảm bảo scale hộp về bình thường
                boxImage.transform.localScale = Vector3.one;
            }
                
            if (soldOverlay != null) soldOverlay.SetActive(false);
            if (newOverlay != null) newOverlay.SetActive(true);

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClicked);
                buyButton.interactable = true;
            }
        }

        private void OnBuyClicked()
        {
            if (isSold || string.IsNullOrEmpty(currentCrateId)) return;

            // Gọi logic mua từ CargoManager
            bool success = CargoManager.Instance.BuyCrate(currentCrateId);
            if (success)
            {
                SetSoldState();
            }
        }

        private void SetSoldState()
        {
            isSold = true;
            if (buyButton != null) buyButton.interactable = false;
            if (newOverlay != null) newOverlay.SetActive(false);

            // Tạo chuỗi Animation bằng DOTween
            Sequence buySequence = DOTween.Sequence();

            if (boxImage != null && boxSoldSprite != null)
            {
                // Hiệu ứng hộp giật nảy lên một chút rồi mới đổi hình rách (box2)
                buySequence.Append(boxImage.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 5, 0.5f)
                    .OnComplete(() => boxImage.sprite = boxSoldSprite));
            }

            if (soldOverlay != null)
            {
                soldOverlay.SetActive(true);
                // Bắt đầu với kích thước khổng lồ
                soldOverlay.transform.localScale = new Vector3(4f, 4f, 4f);
                
                // Hiệu ứng đập mạnh con dấu xuống và nảy đàn hồi (Slam down)
                buySequence.Append(soldOverlay.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));
            }
        }
    }
}
