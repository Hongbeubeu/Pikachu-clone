using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EasyButtons;
using Lean.Touch;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pokemon
{
    public class BoardGenerator : MonoBehaviour
    {
        [SerializeField] private int _shiftIndex = 0;
        [SerializeField] private float _timeDelayDisappear = 0.3f;
        [SerializeField] private Camera _camera;
        [SerializeField] private Sprite[] pokemonIcons;
        [SerializeField] private Tile TilePrefab;
        [SerializeField] private Vector2Int boardSize = new(10, 10);
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private int numberOfPokemon = 10;
        [SerializeField] private LineRenderer lineRenderer;

        private readonly List<int> board = new();
        [SerializeField] private List<Tile> tiles = new();
        private Tile tileHolded;
        private bool isConnecting;
        private float matchedCount = 0;

        private void OnEnable()
        {
            LeanTouch.OnFingerTap += HandleFingerTap;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerTap -= HandleFingerTap;
        }

        [Button]
        private void Generate()
        {
            DeleteChildren();
            board.Clear();
            tiles.Clear();
            for (var i = 0; i < boardSize.x * boardSize.y; i++)
            {
                board.Add((i % numberOfPokemon + _shiftIndex) % pokemonIcons.Length);
            }

            board.Shuffle();

            var minX = -(boardSize.x * cellSize / 2f - cellSize / 2f);
            var minY = -(boardSize.y * cellSize / 2f - cellSize / 2f);
            var position = new Vector2(minX, minY);
            for (var y = 0; y < boardSize.y; y++)
            {
                position.x = minX;
                for (var x = 0; x < boardSize.x; x++)
                {
                    var pokemon = PrefabUtility.InstantiatePrefab(TilePrefab, transform) as Tile;
                    pokemon.transform.localPosition = position;
                    var id = board[y * boardSize.x + x];
                    var gridPosition = new Vector2Int(x, y);
                    pokemon.Setup(pokemonIcons[id], id, gridPosition);
                    position.x += cellSize;
                    pokemon.name = $"pokemon_({x},{y})";
                    tiles.Add(pokemon);
                }

                position.y += cellSize;
            }
        }

        private bool isShuffling = false;

        [SerializeField] private float shuffleTime = 0.3f;

        [Button]
        private void ShuffleBoard()
        {
            isShuffling = true;
            DOVirtual.DelayedCall(shuffleTime, () => { isShuffling = false; });
            var tempTiles = tiles.Where(t => t != null).ToList();
            tempTiles.Shuffle();
            for (var i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] == null) continue;

                tiles[i] = tempTiles[0];
                var gridPosition = new Vector2Int(i % boardSize.x, i / boardSize.x);
                tiles[i].GridPosition = gridPosition;
                tiles[i].transform.DOMove(GridPositionToWorldPosition(gridPosition), shuffleTime);
                tempTiles.RemoveAt(0);
            }
        }

        [Button]
        private void DeleteChildren()
        {
            var children = GetComponentsInChildren<Tile>();
            foreach (var child in children)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        public bool WorldPositionToGridPosition(Vector2 worldPosition, out Vector2Int gridPosition)
        {
            gridPosition = Vector2Int.zero;
            if (!IsInBoard(worldPosition)) return false;

            var x = worldPosition.x + boardSize.x * cellSize / 2f;
            x /= cellSize;
            var y = worldPosition.y + boardSize.y * cellSize / 2f;
            y /= cellSize;
            gridPosition = new Vector2Int((int) x, (int) y);
            return true;
        }

        private Vector2 GridPositionToWorldPosition(Vector2Int gridPosition)
        {
            var x = gridPosition.x * cellSize - boardSize.x * cellSize / 2f;
            x += cellSize / 2f;
            var y = gridPosition.y * cellSize - boardSize.y * cellSize / 2f;
            y += cellSize / 2f;
            return new Vector2(x, y);
        }

        private bool IsInBoard(Vector2 worldPosition)
        {
            if (Mathf.Abs(worldPosition.x) > boardSize.x / 2f || Mathf.Abs(worldPosition.y) > boardSize.y / 2f)
            {
                return false;
            }

            return true;
        }

        public Tile GetTile(Vector2Int gridPosition)
        {
            var index = gridPosition.x + gridPosition.y * boardSize.x;
            if (index < 0 || index > tiles.Count)
                return null;
            return tiles[index];
        }

        private void HandleFingerTap(LeanFinger finger)
        {
            if (isConnecting || isShuffling) return;

            var worldPosition = _camera.ScreenToWorldPoint(finger.ScreenPosition);

            if (WorldPositionToGridPosition(worldPosition, out var gridPosition))
            {
                var tile = GetTile(gridPosition);
                if (tile == null)
                {
                    tileHolded?.OnClick();
                    tileHolded = null;
                    return;
                }

                if (tile == tileHolded)
                {
                    tileHolded = null;
                    tile.OnClick();
                    return;
                }

                if (tileHolded == null)
                {
                    tile.OnClick();
                    tileHolded = tile;
                    return;
                }

                if (tile.Id != tileHolded.Id)
                {
                    tileHolded.OnClick();
                    tileHolded = null;
                    return;
                }

                if (CanMatch(tileHolded, tile, out List<Vector2Int> path))
                {
                    StartCoroutine(Clear(tile, path));
                    return;
                }

                tileHolded.OnClick();
                tileHolded = null;
            }
            else
            {
                if (tileHolded == null) return;
                tileHolded.OnClick();
                tileHolded = null;
            }
        }

        private IEnumerator Clear(Tile tile, List<Vector2Int> path)
        {
            isConnecting = true;
            lineRenderer.positionCount = path.Count;
            for (var i = 0; i < path.Count; i++)
            {
                Vector3 pos = GridPositionToWorldPosition(path[i]);
                pos.z = -1;
                lineRenderer.SetPosition(i, pos);
            }

            tile.OnClick();
            yield return new WaitForSeconds(_timeDelayDisappear);
            Destroy(tile.gameObject);
            Destroy(tileHolded.gameObject);
            tileHolded = null;
            lineRenderer.positionCount = 0;
            isConnecting = false;
            matchedCount += 2;

            if (matchedCount != tiles.Count) yield break;

            _shiftIndex = Random.Range(0, pokemonIcons.Length);
            matchedCount = 0;
            Generate();
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
                    if (length > minLength) return true;
                    minLength = length;
                    path = tempPath;

                    return true;
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
                    if (length > minLength) return true;
                    minLength = length;
                    path = tempPath;

                    return true;
                }
            }

            return false;
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
                if (IsInBoard(GridPositionToWorldPosition(checkGridPosition)))
                    if (tiles[checkGridPosition.y * boardSize.x + checkGridPosition.x] != null)
                        break;

                result.Add(checkGridPosition);
            } while (IsInBoard(GridPositionToWorldPosition(checkGridPosition)));

            checkGridPosition = currentGridPosition;
            do
            {
                checkGridPosition -= Vector2Int.up;
                if (IsInBoard(GridPositionToWorldPosition(checkGridPosition)))
                    if (tiles[checkGridPosition.y * boardSize.x + checkGridPosition.x] != null)
                        break;

                result.Add(checkGridPosition);
            } while (IsInBoard(GridPositionToWorldPosition(checkGridPosition)));


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
                if (IsInBoard(GridPositionToWorldPosition(checkGridPosition)))
                    if (tiles[checkGridPosition.y * boardSize.x + checkGridPosition.x] != null)
                        break;

                result.Add(checkGridPosition);
            } while (IsInBoard(GridPositionToWorldPosition(checkGridPosition)));

            checkGridPosition = currentGridPosition;
            do
            {
                checkGridPosition -= Vector2Int.right;
                if (IsInBoard(GridPositionToWorldPosition(checkGridPosition)))
                    if (tiles[checkGridPosition.y * boardSize.x + checkGridPosition.x] != null)
                        break;

                result.Add(checkGridPosition);
            } while (IsInBoard(GridPositionToWorldPosition(checkGridPosition)));

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

            if (Vector2.Distance(fromPositon, toPosition) <= cellSize)
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
                if (!IsInBoard(GridPositionToWorldPosition(checkPosition)))
                    continue; //TODO Need to check bound of board
                if (GetTile(checkPosition) != null)
                {
                    return false;
                }
            }

            return true;
        }

        [Button]
        private void Check(Vector2 from, Vector2 to)
        {
            Debug.Log(CanConnect(new Vector2Int((int) from.x, (int) from.y), new Vector2Int((int) to.x, (int) to.y)));
        }

        private bool IsOnSameLine(Vector2Int fromPosition, Vector2Int toPosition, out Vector2Int direction)
        {
            direction = toPosition - fromPosition;
            return direction.x == 0 || direction.y == 0;
        }
    }
}