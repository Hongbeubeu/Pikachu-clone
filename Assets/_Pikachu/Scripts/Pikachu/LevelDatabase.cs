using UnityEngine;

namespace Pokemon
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Data/LevelDatabase", order = 0)]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] private LevelData[] levelDatas;

        public LevelData[] LevelDatas => levelDatas;
    }
}