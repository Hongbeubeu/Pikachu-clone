using UnityEngine;

namespace Pokemon
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Data/Game Config", order = 0)]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private Vector2Int boardSize;
        [SerializeField] private float cellSize;
        [SerializeField] private float timeDelayDisappear = 0.3f;
        [SerializeField] private float shuffleTime = 0.3f;
        [SerializeField] private float shiftTime = 0.2f;
        

        public Vector2Int BoardSize => boardSize;
        public float CellSize => cellSize;
        public float TimeDelayDisappear => timeDelayDisappear;
        public float ShuffleTime => shuffleTime;
        public float ShiftTime => shiftTime;

    }
}