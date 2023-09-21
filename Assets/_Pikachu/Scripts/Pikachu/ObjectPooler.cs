using Ultimate.Core.Runtime.Pool;
using UnityEngine;

namespace Pokemon
{
    [CreateAssetMenu(fileName = "ObjectPooler", menuName = "Data/ObjectPooler")]
    public class ObjectPooler : ScriptableObject
    {
        [SerializeField] private Tile tilePrefab;

        public Tile GetTile()
        {
            return FastPoolManager.GetPool(tilePrefab).FastInstantiate<Tile>();
        }

        public void ReturnPoolTile(GameObject tile)
        {
            FastPoolManager.GetPool(tilePrefab).FastDestroy(tile);
        }
    }
}