using System;
using Sirenix.OdinInspector;
using Ultimate.Core.Runtime.Singleton;
using UnityEngine;

namespace Pokemon
{
    public class GameController : Singleton<GameController>
    {
        [SerializeField] private BoardController boardController;
        public BoardController BoardController => boardController;
        public int CurrentLevel => _currentLevel;

        private int _currentLevel;

        public override void Init()
        {
        }

        private void OnEnable()
        {
            Load();
        }

        private void OnDisable()
        {
            Save();
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void Start()
        {
            StartGame();
        }

        [Button(ButtonSizes.Gigantic)]
        public void StartGame()
        {
            boardController.StartGame();
        }

        private void Load()
        {
            _currentLevel = PlayerPrefs.GetInt(Constants.SaveData.CURRENT_LEVEL, 0);
        }

        private void Save()
        {
            PlayerPrefs.SetInt(Constants.SaveData.CURRENT_LEVEL, _currentLevel);
        }

        public void LevelUp()
        {
            if (_currentLevel == GameManager.Instance.MaxLevel)
                return;
            _currentLevel++;
            Save();
            StartGame();
        }
    }
}