using System;
using Lean.Touch;
using Sirenix.OdinInspector;
using Ultimate.Core.Runtime.Singleton;
using UnityEngine;

namespace Pokemon
{
    public class GameController : Singleton<GameController>
    {
        [SerializeField] private BoardController boardController;

        public BoardController BoardController => boardController;

        public override void Init()
        {
        }

        [Button(ButtonSizes.Gigantic)]
        public void StartGame()
        {
            boardController.StartGame();
        }
    }
}