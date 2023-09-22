using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pokemon
{
    public class BoardController : MonoBehaviour
    {
        private static readonly Vector2Int[] ShiftDirections =
        {
            Vector2Int.zero,
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private List<Tile> tiles = new();

        public bool CanSelect => !(_isConnecting || _isShuffling);
        private Vector2Int BoardSize => _currentLevelData.BoardSize;
        private int NumberOfPokemon => _currentLevelData.NumberOfPokemon;

        private LevelData _currentLevelData;
        private ShiftDirection _shiftDirection;
        private float _cellSize;
        private float _timeDelayDisappear;
        private float _shuffleTime;
        private float _shiftTime;
        private readonly List<int> _board = new();
        private Tile _tileHolded;
        private bool _isConnecting;
        private float _matchedCount;
        private bool _isShuffling;
        private int _currentLevel;

        public void StartGame()
        {
            OnStart();
            GameManager.Instance.SetCameraSize(_currentLevelData.BoardSize.y);
            Generate();
        }

        private void OnStart()
        {
            if (GameManager.Instance == null) throw new Exception("Missing GameManager!!");

            _cellSize = GameManager.Instance.GameConfig.CellSize;
            _timeDelayDisappear = GameManager.Instance.GameConfig.TimeDelayDisappear;
            _shuffleTime = GameManager.Instance.GameConfig.ShuffleTime;
            _shiftTime = GameManager.Instance.GameConfig.ShiftTime;
            _currentLevel = GameController.Instance.CurrentLevel;
            _currentLevelData = GameManager.Instance.GetLevelData(_currentLevel);
            _shiftDirection = _currentLevelData.ShiftDirection;
        }

        [Button(ButtonSizes.Medium)]
        private void Generate()
        {
            if (!Application.isPlaying) return;
            ClearPreviousBoard();
            RandomTiles();
            SpawnBoard();
        }

        private void ClearPreviousBoard()
        {
            foreach (var tile in tiles.Where(tile => tile != null))
            {
                tile.ReturnPool();
            }

            _board.Clear();
            tiles.Clear();
        }

        private void RandomTiles()
        {
            var listRandomIcon = new List<int>();
            do
            {
                var id = Random.Range(0, GameManager.Instance.PokemonCount);
                if (!listRandomIcon.Contains(id))
                    listRandomIcon.Add(id);
            } while (listRandomIcon.Count < NumberOfPokemon);

            for (var i = 0; i < BoardSize.x * BoardSize.y / NumberOfPokemon; i++)
            {
                _board.AddRange(listRandomIcon);
            }

            _board.Shuffle();
        }

        private void SpawnBoard()
        {
            var minX = -(BoardSize.x * _cellSize / 2f - _cellSize / 2f);
            var minY = -(BoardSize.y * _cellSize / 2f - _cellSize / 2f);
            var position = new Vector2(minX, minY);
            for (var y = 0; y < BoardSize.y; y++)
            {
                position.x = minX;
                for (var x = 0; x < BoardSize.x; x++)
                {
                    var tile = GameManager.Instance.ObjectPooler.GetTile();

                    var tileTransform = tile.transform;
                    tileTransform.SetParent(transform);
                    tileTransform.localPosition = position;

                    var index = _board[y * BoardSize.x + x];
                    var gridPosition = new Vector2Int(x, y);
                    tile.Setup(GameManager.Instance.GetIconAtIndex(index), index, gridPosition);
                    position.x += _cellSize;
                    tile.name = $"pokemon_({x},{y})";
                    tiles.Add(tile);
                }

                position.y += _cellSize;
            }

            if (!CheckHasMatch())
            {
                ShuffleBoard();
            }
        }

        [Button(ButtonSizes.Medium)]
        private void ShuffleBoard()
        {
            if (!Application.isPlaying) return;

            while (true)
            {
                _isShuffling = true;
                DOVirtual.DelayedCall(_shuffleTime, () => { _isShuffling = false; });
                var tempTiles = tiles.Where(t => t != null).ToList();
                tempTiles.Shuffle();
                for (var i = 0; i < tiles.Count; i++)
                {
                    if (tiles[i] == null) continue;

                    tiles[i] = tempTiles[0];
                    var gridPosition = new Vector2Int(i % BoardSize.x, i / BoardSize.x);
                    tiles[i].GridPosition = gridPosition;
                    tiles[i].transform.DOMove(GridPositionToWorldPosition(gridPosition), _shuffleTime);
                    tempTiles.RemoveAt(0);
                }

                if (!CheckHasMatch()) continue;
                break;
            }
        }

        private bool CheckHasMatch()
        {
            var listChecked = new List<int>();
            for (var i = 0; i < tiles.Count - 1; i++)
            {
                var tile = tiles[i];
                if (tile == null) continue;
                if (listChecked.Contains(tile.Id)) continue;
                for (var j = i + 1; j < tiles.Count; j++)
                {
                    if (tiles[j] == null) continue;
                    if (listChecked.Contains(tiles[j].Id)) continue;
                    if (tile.Id != tiles[j].Id) continue;
                    if (CanMatch(tile, tiles[j], out _))
                    {
                        return true;
                    }
                }

                listChecked.Add(tile.Id);
            }

            return false;
        }

        public bool WorldPositionToGridPosition(Vector2 worldPosition, out Vector2Int gridPosition)
        {
            gridPosition = Vector2Int.zero;
            if (!IsInsideBoard(worldPosition)) return false;

            var x = worldPosition.x + BoardSize.x * _cellSize / 2f;
            x /= _cellSize;
            var y = worldPosition.y + BoardSize.y * _cellSize / 2f;
            y /= _cellSize;
            gridPosition = new Vector2Int((int) x, (int) y);
            return true;
        }

        private Vector2 GridPositionToWorldPosition(Vector2Int gridPosition)
        {
            var x = gridPosition.x * _cellSize - BoardSize.x * _cellSize / 2f;
            x += _cellSize / 2f;
            var y = gridPosition.y * _cellSize - BoardSize.y * _cellSize / 2f;
            y += _cellSize / 2f;
            return new Vector2(x, y);
        }

        private bool IsInsideBoard(Vector2 worldPosition)
        {
            return Mathf.Abs(worldPosition.x) <= BoardSize.x * _cellSize / 2f &&
                   Mathf.Abs(worldPosition.y) <= BoardSize.y * _cellSize / 2f;
        }

        private bool IsInsideBoard(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < BoardSize.x && gridPosition.y >= 0 &&
                   gridPosition.y < BoardSize.y;
        }

        public Tile GetTile(Vector2Int gridPosition)
        {
            var index = gridPosition.x + gridPosition.y * BoardSize.x;
            if (index < 0 || index > tiles.Count)
                return null;
            return tiles[index];
        }

        public void OnTapTile(Vector2Int gridPosition)
        {
            var tile = GetTile(gridPosition);
            if (tile == null)
            {
                if (_tileHolded == null) return;
                _tileHolded.OnClick();
                _tileHolded = null;
                return;
            }

            if (tile == _tileHolded)
            {
                _tileHolded = null;
                tile.OnClick();
                return;
            }

            if (_tileHolded == null)
            {
                tile.OnClick();
                _tileHolded = tile;
                return;
            }

            if (tile.Id != _tileHolded.Id)
            {
                _tileHolded.OnClick();
                _tileHolded = null;
                return;
            }

            if (CanMatch(_tileHolded, tile, out List<Vector2Int> path))
            {
                StartCoroutine(Clear(tile, path));
                return;
            }

            _tileHolded.OnClick();
            _tileHolded = null;
        }

        public void OnTapEmptyTile()
        {
            if (_tileHolded == null) return;
            _tileHolded.OnClick();
            _tileHolded = null;
        }

        private IEnumerator Clear(Tile tile, List<Vector2Int> path)
        {
            _isConnecting = true;
            lineRenderer.positionCount = path.Count;
            for (var i = 0; i < path.Count; i++)
            {
                Vector3 pos = GridPositionToWorldPosition(path[i]);
                pos.z = -1;
                lineRenderer.SetPosition(i, pos);
            }

            tile.OnClick();
            yield return new WaitForSeconds(_timeDelayDisappear);
            tile.ReturnPool();
            _tileHolded.ReturnPool();
            tiles[GridPositionToIndex(tile.GridPosition)] = null;
            tiles[GridPositionToIndex(_tileHolded.GridPosition)] = null;
            _tileHolded = null;
            lineRenderer.positionCount = 0;
            _isConnecting = false;
            _matchedCount += 2;
            Shift();

            if (_matchedCount < tiles.Count)
            {
                yield return new WaitForSeconds(_shiftTime * 2);
                if (!CheckHasMatch()) ShuffleBoard();
                yield break;
            }

            _matchedCount = 0;
            GameController.Instance.LevelUp();
        }

        //TODO Use strategy pattern
        private void Shift()
        {
            switch (_shiftDirection)
            {
                case ShiftDirection.None:
                    break;
                case ShiftDirection.Up:
                    ShiftUp();
                    break;
                case ShiftDirection.Right:
                    ShiftRight();
                    break;
                case ShiftDirection.Down:
                    ShiftDown();
                    break;
                case ShiftDirection.Left:
                    ShiftLeft();
                    break;
                case ShiftDirection.UpRight:
                    ShiftUpRight();
                    break;
                case ShiftDirection.DownRight:
                    ShiftDownRight();
                    break;
                case ShiftDirection.DownLeft:
                    ShiftDownLeft();
                    break;
                case ShiftDirection.UpLeft:
                    ShiftUpLeft();
                    break;
                case ShiftDirection.Center:
                    ShiftCenter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ShiftCenter()
        {
            ShiftCenterVertical();
            DOVirtual.DelayedCall(_shiftTime, ShiftCenterHorizontal);
        }

        private void ShiftCenterVertical()
        {
            for (var x = 0; x < BoardSize.x; x++)
            {
                for (var y = BoardSize.y / 2 - 1; y >= 0; y--)
                {
                    DoShiftCenter(x, y, Vector2Int.up);
                }

                for (var y = BoardSize.y / 2 + 1; y < BoardSize.y; y++)
                {
                    DoShiftCenter(x, y, Vector2Int.down);
                }
            }
        }

        private void ShiftCenterHorizontal()
        {
            for (var y = 0; y < BoardSize.y; y++)
            {
                for (var x = BoardSize.x / 2 - 1; x >= 0; x--)
                {
                    DoShiftCenter(x, y, Vector2Int.right);
                }

                for (var x = BoardSize.x / 2 + 1; x < BoardSize.x; x++)
                {
                    DoShiftCenter(x, y, Vector2Int.left);
                }
            }
        }


        private void ShiftUpLeft()
        {
            ShiftUp();
            DOVirtual.DelayedCall(_shiftTime, ShiftLeft);
        }

        private void ShiftDownLeft()
        {
            ShiftDown();
            DOVirtual.DelayedCall(_shiftTime, ShiftLeft);
        }

        private void ShiftDownRight()
        {
            ShiftDown();
            DOVirtual.DelayedCall(_shiftTime, ShiftRight);
        }

        private void ShiftUpRight()
        {
            ShiftUp();
            DOVirtual.DelayedCall(_shiftTime, ShiftRight);
        }

        private void ShiftDown()
        {
            for (var y = 0; y < BoardSize.y; y++)
            {
                for (var x = 0; x < BoardSize.x; x++)
                {
                    DoShift(x, y, ShiftDirections[(int) ShiftDirection.Down]);
                }
            }
        }

        private void ShiftUp()
        {
            for (var y = BoardSize.y - 1; y >= 0; y--)
            {
                for (var x = 0; x < BoardSize.x; x++)
                {
                    DoShift(x, y, ShiftDirections[(int) ShiftDirection.Up]);
                }
            }
        }

        private void ShiftLeft()
        {
            for (var x = 0; x < BoardSize.x; x++)
            {
                for (var y = 0; y < BoardSize.y; y++)
                {
                    DoShift(x, y, ShiftDirections[(int) ShiftDirection.Left]);
                }
            }
        }

        private void ShiftRight()
        {
            for (var x = BoardSize.x - 1; x >= 0; x--)
            {
                for (var y = 0; y < BoardSize.y; y++)
                {
                    DoShift(x, y, ShiftDirections[(int) ShiftDirection.Right]);
                }
            }
        }

        private void DoShiftCenter(int x, int y, Vector2Int direction)
        {
            var tile = tiles[y * BoardSize.x + x];
            if (tile == null) return;
            var target = FindCenterEmptyTile(tile.GridPosition, direction);
            if (target == tile.GridPosition) return;

            DoSwapAndMoveTile(target, tile);
        }

        private void DoShift(int x, int y, Vector2Int direction)
        {
            var tile = tiles[y * BoardSize.x + x];
            if (tile == null) return;
            var target = FindEmptyTile(tile.GridPosition, direction);
            if (target == tile.GridPosition) return;

            DoSwapAndMoveTile(target, tile);
        }

        private void DoSwapAndMoveTile(Vector2Int target, Tile tile)
        {
            tiles[GridPositionToIndex(target)] = tile;
            tiles[GridPositionToIndex(tile.GridPosition)] = null;
            tile.GridPosition = target;
            tile.transform.DOMove(GridPositionToWorldPosition(target), _shiftTime);
        }

        private Vector2Int FindEmptyTile(Vector2Int currentGridPosition, Vector2Int direction)
        {
            var checkPosition = currentGridPosition + direction;
            while (IsInsideBoard(checkPosition))
            {
                if (tiles[GridPositionToIndex(checkPosition)] != null)
                    return checkPosition - direction;
                checkPosition += direction;
            }

            return checkPosition - direction;
        }

        private Vector2Int FindCenterEmptyTile(Vector2Int currentGridPosition, Vector2Int direction)
        {
            if (direction == Vector2Int.up && currentGridPosition.y > BoardSize.y / 2)
                throw new ArgumentException("wtf lmao");
            if (direction == Vector2Int.down && currentGridPosition.y < BoardSize.y / 2)
                throw new ArgumentException("wtf lmao");
            if (direction == Vector2Int.right && currentGridPosition.x > BoardSize.y / 2)
                throw new ArgumentException("wtf lmao");
            if (direction == Vector2Int.left && currentGridPosition.x < BoardSize.y / 2)
                throw new ArgumentException("wtf lmao");

            var checkPosition = currentGridPosition + direction;

            while (!IsCenterTile(checkPosition, direction))
            {
                var index = GridPositionToIndex(checkPosition);
                if (index < 0 || index >= tiles.Count)
                {
                    Debug.LogError("wtf lmao");
                }

                if (tiles[index] != null)
                    return checkPosition - direction;
                checkPosition += direction;
            }

            if (tiles[GridPositionToIndex(checkPosition)] != null)
                return checkPosition - direction;
            return checkPosition;
        }

        private bool IsCenterTile(Vector2Int gridPosition, Vector2Int direction)
        {
            if (direction.x != 0)
                return gridPosition.x == BoardSize.x / 2;
            return gridPosition.y == BoardSize.y / 2;
        }

        private int GridPositionToIndex(Vector2Int gridPosition)
        {
            return gridPosition.x + gridPosition.y * BoardSize.x;
        }

        private bool CanMatch(Tile fromTile, Tile toTile, out List<Vector2Int> path)
        {
            var verticalLine1 = GetVerticalLine(fromTile);
            var verticalLine2 = GetVerticalLine(toTile);
            var horizontalLine1 = GetHorizontalLine(fromTile);
            var horizontalLine2 = GetHorizontalLine(toTile);

            path = new List<Vector2Int>();
            var minLength = float.MaxValue;

            foreach (var point1 in verticalLine1)
            {
                foreach (var point2 in verticalLine2)
                {
                    if (point1.y != point2.y) continue;
                    if (!CanConnect(point1, point2)) continue;
                    var tempPath = new List<Vector2Int> {fromTile.GridPosition};
                    if (!tempPath.Contains(point1))
                        tempPath.Add(point1);
                    if (!tempPath.Contains(point2))
                        tempPath.Add(point2);
                    if (!tempPath.Contains(toTile.GridPosition))
                        tempPath.Add(toTile.GridPosition);

                    var length = CalculateLength(tempPath);
                    if (length > minLength) continue;
                    minLength = length;
                    path = tempPath;
                }
            }

            foreach (var point1 in horizontalLine1)
            {
                foreach (var point2 in horizontalLine2)
                {
                    if (point1.x != point2.x) continue;
                    if (!CanConnect(point1, point2)) continue;
                    var tempPath = new List<Vector2Int> {fromTile.GridPosition};
                    if (!tempPath.Contains(point1))
                        tempPath.Add(point1);
                    if (!tempPath.Contains(point2))
                        tempPath.Add(point2);
                    if (!tempPath.Contains(toTile.GridPosition))
                        tempPath.Add(toTile.GridPosition);

                    var length = CalculateLength(tempPath);
                    if (length > minLength) continue;
                    minLength = length;
                    path = tempPath;
                }
            }

            return path.Count > 0;
        }

        private float CalculateLength(List<Vector2Int> path)
        {
            float length = 0;
            for (var i = 0; i < path.Count - 1; i++)
            {
                length += Vector2Int.Distance(path[i], path[i + 1]);
            }

            return length;
        }

        private List<Vector2Int> GetVerticalLine(Tile tile)
        {
            var result = new List<Vector2Int>();
            var currentGridPosition = tile.GridPosition;

            result.Add(currentGridPosition);
            var checkGridPosition = currentGridPosition;
            do
            {
                checkGridPosition += Vector2Int.up;
                if (IsInsideBoard(GridPositionToWorldPosition(checkGridPosition)))
                    if (tiles[checkGridPosition.y * BoardSize.x + checkGridPosition.x] != null)
                        break;

                result.Add(checkGridPosition);
            } while (IsInsideBoard(GridPositionToWorldPosition(checkGridPosition)));

            checkGridPosition = currentGridPosition;
            do
            {
                checkGridPosition -= Vector2Int.up;
                if (IsInsideBoard(GridPositionToWorldPosition(checkGridPosition)))
                    if (tiles[checkGridPosition.y * BoardSize.x + checkGridPosition.x] != null)
                        break;

                result.Add(checkGridPosition);
            } while (IsInsideBoard(GridPositionToWorldPosition(checkGridPosition)));


            return result;
        }

        private List<Vector2Int> GetHorizontalLine(Tile tile)
        {
            var result = new List<Vector2Int>();
            var currentGridPosition = tile.GridPosition;
            result.Add(currentGridPosition);

            var checkGridPosition = currentGridPosition;
            do
            {
                checkGridPosition += Vector2Int.right;
                if (IsInsideBoard(GridPositionToWorldPosition(checkGridPosition)))
                    if (tiles[checkGridPosition.y * BoardSize.x + checkGridPosition.x] != null)
                        break;

                result.Add(checkGridPosition);
            } while (IsInsideBoard(GridPositionToWorldPosition(checkGridPosition)));

            checkGridPosition = currentGridPosition;
            do
            {
                checkGridPosition -= Vector2Int.right;
                if (IsInsideBoard(GridPositionToWorldPosition(checkGridPosition)))
                    if (tiles[checkGridPosition.y * BoardSize.x + checkGridPosition.x] != null)
                        break;

                result.Add(checkGridPosition);
            } while (IsInsideBoard(GridPositionToWorldPosition(checkGridPosition)));

            return result;
        }

        /// <summary>
        /// Checks if the fromPosition and toPosition is on the same horizontal/vertical line and hasn't blocked by the other tiles
        /// </summary>
        /// <param name="fromPositon"></param>
        /// <param name="toPosition"></param>
        /// <returns></returns>
        private bool CanConnect(Vector2Int fromPositon, Vector2Int toPosition)
        {
            if (!IsOnSameLine(fromPositon, toPosition, out var direction))
            {
                throw new InvalidOperationException(
                    $"from position {fromPositon} is not on the same horizontal/ vertical line to {toPosition}");
            }

            if (Vector2.Distance(fromPositon, toPosition) <= _cellSize)
            {
                return true;
            }

            var numberNodeBetween = Mathf.Abs(direction.x != 0 ? direction.x : direction.y);
            var step = direction;

            //normal direction;
            step.x = step.x != 0 ? (step.x > 0 ? 1 : -1) : 0;
            step.y = step.y != 0 ? (step.y > 0 ? 1 : -1) : 0;

            for (var i = 0; i < numberNodeBetween - 1; i++)
            {
                var checkPosition = fromPositon + (i + 1) * step;
                if (!IsInsideBoard(GridPositionToWorldPosition(checkPosition)))
                    continue;
                if (GetTile(checkPosition) != null)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsOnSameLine(Vector2Int fromPosition, Vector2Int toPosition, out Vector2Int direction)
        {
            direction = toPosition - fromPosition;
            return direction.x == 0 || direction.y == 0;
        }
    }
}