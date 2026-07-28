using UnityEngine;

namespace WordsOnTheWaves.Data
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        public float CurrentCash { get; private set; } = 500f; // Vốn ban đầu

        // Khai báo Event để UI tự động cập nhật
        public delegate void OnCashChanged(float newCash);
        public event OnCashChanged cashChangedEvent;

        private void Awake()
        {
            Instance = this;
            CurrentCash = PlayerPrefs.GetFloat("PlayerCash", 500f);
        }

        public bool SpendCash(float amount)
        {
            if (CurrentCash >= amount)
            {
                CurrentCash -= amount;
                PlayerPrefs.SetFloat("PlayerCash", CurrentCash);
                cashChangedEvent?.Invoke(CurrentCash);
                Debug.Log($"Tiêu {amount} xu. Còn lại: {CurrentCash}");
                return true;
            }
            Debug.LogWarning("Không đủ tiền!");
            return false;
        }

        public void AddCash(float amount)
        {
            CurrentCash += amount;
            PlayerPrefs.SetFloat("PlayerCash", CurrentCash);
            cashChangedEvent?.Invoke(CurrentCash);
            Debug.Log($"Kiếm được {amount} xu. Tổng: {CurrentCash}");
        }
    }
}
