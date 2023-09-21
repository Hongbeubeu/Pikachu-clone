using System;
using Lean.Touch;
using UnityEngine;

namespace Pokemon
{
    public class InputController : MonoBehaviour
    {
        private void OnEnable()
        {
            LeanTouch.OnFingerTap += HandleFingerTap;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerTap -= HandleFingerTap;
        }

        private void HandleFingerTap(LeanFinger finger)
        {
            if (GameController.Instance == null) throw new Exception("Board Controller is missing!");

            var boardController = GameController.Instance.BoardController;

            if (GameController.Instance.BoardController == null) throw new Exception("Board Controller is missing!");

            if (!boardController.CanSelect) return;

            var worldPosition = GameManager.Instance.MainCamera.ScreenToWorldPoint(finger.ScreenPosition);

            if (boardController.WorldPositionToGridPosition(worldPosition, out var gridPosition))
            {
                boardController.OnTapTile(gridPosition);
            }
            else
            {
                boardController.OnTapEmptyTile();
            }
        }
    }
}