using Ultimate.Core.Runtime.Singleton;
using UnityEngine;

namespace Pokemon
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private ObjectPooler objectPooler;
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private Sprite[] pokemonIcons;
        [SerializeField] private LevelDatabase levelDatabase;

        public Camera MainCamera => mainCamera;
        public GameConfig GameConfig => gameConfig;
        public ObjectPooler ObjectPooler => objectPooler;
        public LevelDatabase LevelDatabase => levelDatabase;
        public int PokemonCount => pokemonIcons.Length;

        public override void Init()
        {
        }

        public Sprite GetIconAtIndex(int index)
        {
            index %= pokemonIcons.Length;
            return pokemonIcons[index];
        }

        public void SetCameraSize(int height)
        {
            mainCamera.orthographicSize = height + 1;
        }
    }
}