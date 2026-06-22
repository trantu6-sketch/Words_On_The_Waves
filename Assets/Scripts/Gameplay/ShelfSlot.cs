using UnityEngine;
using WordsOnTheWaves.Data;

namespace WordsOnTheWaves.Gameplay
{
    public class ShelfSlot : MonoBehaviour
    {
        public bool isOccupied = false;
        public BookGenre currentGenre;
        
        [Header("Mô hình 3D bán trong suốt để hiển thị xem trước")]
        public GameObject previewMesh;

        private void Awake()
        {
            if (previewMesh != null) previewMesh.SetActive(false);
        }

        public void ShowPreview(bool show)
        {
            if (isOccupied) return;
            if (previewMesh != null) previewMesh.SetActive(show);
        }

        public void PlaceBook(BookGenre genre)
        {
            isOccupied = true;
            currentGenre = genre;
            ShowPreview(false);
        }

        public void RemoveBook()
        {
            isOccupied = false;
        }
    }
}
