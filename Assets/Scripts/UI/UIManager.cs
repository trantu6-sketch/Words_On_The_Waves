using System.Collections.Generic;
using UnityEngine;

namespace WordsOnTheWaves.UI
{
    [System.Serializable]
    public struct UIScreen
    {
        public string screenName;
        public GameObject screenObj;
    }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public List<UIScreen> screens;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ShowScreen(string screenName)
        {
            foreach (var screen in screens)
            {
                if (screen.screenObj != null)
                {
                    screen.screenObj.SetActive(screen.screenName == screenName);
                }
            }
        }
    }
}
