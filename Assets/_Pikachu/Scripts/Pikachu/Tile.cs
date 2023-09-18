using UnityEngine;

namespace Pokemon
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer bg;
        [SerializeField] private SpriteRenderer icon;
        [SerializeField] private Color selected = new(0, 255, 160);
        private bool isSelected;
        [SerializeField] private int id;
        [SerializeField] private Vector2Int gridPosition;
        public int Id => id;
        public Vector3 Position => transform.position;

        public Vector2Int GridPosition
        {
            get => gridPosition;
            set => gridPosition = value;
        }

        public void Setup(Sprite sprite, int id, Vector2Int gridPosition)
        {
            icon.sprite = sprite;
            this.id = id;
            this.gridPosition = gridPosition;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
        
        private void OnSelect()
        {
            isSelected = true;
            bg.color = selected;
        }

        private void OnDeselect()
        {
            isSelected = false;
            bg.color = Color.white;
        }

        public void OnClick()
        {
            if (isSelected)
                OnDeselect();
            else
                OnSelect();
        }
    }
}