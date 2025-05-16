using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Code.Scripts.Common;
using Code.Scripts.Runtime.GameResources;
using Code.Scripts.Runtime.Grid;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public class Belt : MonoBehaviour
    {
        public enum BeltType
        {
            Pole,
            EastWest,
            NorthSouth,
            SouthNorth,
            WestEast,
            EastNorth,
            EastSouth,
            NorthEast,
            NorthWest,
            SouthEast,
            SouthWest,
            WestNorth,
            WestSouth,
        }

        public enum BeltDirection
        {
            None,
            North,
            South,
            East,
            West
        }

        [SerializeField] private Building m_building;
        [SerializeField] private BeltType m_beltType;
        [SerializeField] private BeltDirection m_inputDirection;
        [SerializeField] private BeltDirection m_outputDirection;
        [SerializeField] private Belt m_inputBelt;
        [SerializeField] private Belt m_outputBelt;
        [SerializeField] private SerializedDictionary<BeltType, TileBase> m_tiles;
        [SerializeField] private float m_time = 0.15f;

        [Header("Debug")]
        [SerializeField] private GameObject m_inputBuildingObject;
        [SerializeField] private GameObject m_outputBuildingObject;

        private IConveyorEndpoint m_inputBuilding;
        private IConveyorEndpoint m_outputBuilding;
        private bool m_occupied;
        private readonly Queue<MonoItem> m_itemQueue = new(); // Optional if you allow multiple stacking
        private MonoItem m_currentItem;

        public BeltType Type => m_beltType;
        public Belt InputBelt => m_inputBelt;
        public Belt OutputBelt => m_outputBelt;
        public BeltDirection InputDirection => m_inputDirection;
        public BeltDirection OutputDirection => m_outputDirection;
        public Building Building => m_building;
        public IConveyorEndpoint InputBuilding => m_inputBuilding;
        public IConveyorEndpoint OutputBuilding => m_outputBuilding;

        public void ReverseChain()
        {
            var queue = new Queue<Belt>();
            var visited = new HashSet<Belt>();
            queue.Enqueue(this);
            visited.Add(this);
            while (queue.Count > 0)
            {
                var currentBelt = queue.Dequeue();
                (currentBelt.m_inputBelt, currentBelt.m_outputBelt) = (currentBelt.m_outputBelt, currentBelt.m_inputBelt);
                (currentBelt.m_inputDirection, currentBelt.m_outputDirection) = (currentBelt.m_outputDirection, currentBelt.m_inputDirection);
                currentBelt.MakeType(GetBeltType(currentBelt.m_inputDirection, currentBelt.m_outputDirection));
                if (currentBelt.m_inputBelt != null && !visited.Contains(currentBelt.m_inputBelt))
                {
                    queue.Enqueue(currentBelt.m_inputBelt);
                    visited.Add(currentBelt.m_inputBelt);
                }

                if (currentBelt.m_outputBelt != null && !visited.Contains(currentBelt.m_outputBelt))
                {
                    queue.Enqueue(currentBelt.m_outputBelt);
                    visited.Add(currentBelt.m_outputBelt);
                }
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            if(m_outputBelt && m_outputBelt.m_inputBelt == this)
                m_outputBelt.m_inputBelt = null;
            if (m_inputBelt && m_inputBelt.m_outputBelt == this)
                m_inputBelt.m_outputBelt = null;

            m_inputBuilding?.RemoveOutputBelt(this);
            m_outputBuilding?.RemoveInputBelt(this);

            while(m_itemQueue.Count > 0)
            {
                var item = m_itemQueue.Dequeue();
                Destroy(item.gameObject);
            }

            if(m_currentItem)
                Destroy(m_currentItem.gameObject);
        }

        public bool HasInputAtStart()
        {
            var inputBelt = m_inputBelt;
            var inputBuilding = m_inputBuilding;

            if (!inputBelt && inputBuilding == null) return false;
            return inputBelt ? inputBelt.HasInputAtStart() : inputBuilding != null;
        }

        public bool SetInputBuilding(IConveyorEndpoint building, BeltDirection inputDirection)
        {
            if(m_inputBuilding != null)
                return false;
            if (m_outputBuilding != null && m_outputBuilding == building)
                return false;

            m_inputDirection = inputDirection;
            if(m_outputDirection == BeltDirection.None)
                m_outputDirection = Opposite(m_inputDirection);

            m_inputBuilding = building;
            MakeType(GetBeltType(m_inputDirection, m_outputDirection));
            if(m_inputBuilding is MonoBehaviour mono)
                m_inputBuildingObject = mono.gameObject;
            else
                m_inputBuildingObject = null;
            return true;
        }

        public bool SetOutputBuilding(IConveyorEndpoint building, BeltDirection outputDirection)
        {
            if (m_outputBuilding != null)
                return false;
            if (m_inputBuilding != null && m_inputBuilding == building)
                return false;

            m_outputDirection = outputDirection;
            if(m_inputDirection == BeltDirection.None)
                m_inputDirection = Opposite(m_outputDirection);

            m_outputBuilding = building;
            MakeType(GetBeltType(m_inputDirection, m_outputDirection));

            if(m_outputBuilding is MonoBehaviour mono)
                m_outputBuildingObject = mono.gameObject;
            else
                m_outputBuildingObject = null;
            return true;
        }

        public void RemoveInputBuilding(IConveyorEndpoint building)
        {
            if (m_inputBuilding == building)
            {
                m_inputBuilding = null;
                m_inputDirection = BeltDirection.None;
                MakeType(GetBeltType(m_inputDirection, m_outputDirection));
            }
        }

        public void RemoveOutputBuilding(IConveyorEndpoint building)
        {
            if (m_outputBuilding == building)
            {
                m_outputBuilding = null;
                m_outputDirection = BeltDirection.None;
                MakeType(GetBeltType(m_inputDirection, m_outputDirection));
            }
        }

        private void Start()
        {
            Register();
            Invoke(nameof(Resolve), 0.05f); // Let all belts register before resolving types
        }

        private void Register()
        {
            var gridManager = GridManager.Instance;
            var worldPos = transform.position;

            gridManager.TrySnapPosition(worldPos.ToVector2(), out var snappedPos);
            gridManager.WorldToGrid(snappedPos, out var gridPos);

            gridManager.TryAddBuilding(gridPos, m_building);
            gridManager.TryAddBelt(gridPos, this);
        }

        private void Resolve()
        {
            DetectBeltType();
        }

        public void ReceiveItem(MonoItem item)
        {
            m_itemQueue.Enqueue(item);
            if (!m_occupied)
            {
                StartCoroutine(ProcessQueue());
            }
        }

        private IEnumerator ProcessQueue()
        {
            while (m_itemQueue.Count > 0)
            {
                var item = m_itemQueue.Dequeue();
                m_currentItem = item;
                yield return MoveItem(item);
            }
        }

        public bool IsOccupied() => m_occupied;

        private IEnumerator MoveItem(MonoItem item)
        {
            // Wait until the belt is not occupied
            yield return new WaitUntil(() => !m_occupied);

            // resume tilemap animation
            m_occupied = true;
            var start = item.transform.position;
            var end = transform.position;

            var elapsed = 0f;

            while (elapsed < m_time)
            {
                if(m_building.BuildingTilemap.GetTileAnimationFlags(Vector3Int.zero) == TileAnimationFlags.PauseAnimation)
                    m_building.BuildingTilemap.SetTileAnimationFlags(Vector3Int.zero, TileAnimationFlags.None);
                item.transform.position = Vector3.Lerp(start, end, elapsed / m_time);
                elapsed += Time.deltaTime;
                yield return null;
            }

            item.transform.position = end;

            m_building.BuildingTilemap.SetTileAnimationFlags(Vector3Int.zero, TileAnimationFlags.PauseAnimation);
            // If there is a next belt, move it there
            yield return new WaitUntil(() => m_outputBelt || m_outputBuilding != null);
            if (m_outputBuilding != null)
            {
                start = item.transform.position;
                end = m_outputBuilding.Transform.position;

                elapsed = 0f;

                while (elapsed < m_time)
                {
                    if(m_building.BuildingTilemap.GetTileAnimationFlags(Vector3Int.zero) == TileAnimationFlags.PauseAnimation)
                        m_building.BuildingTilemap.SetTileAnimationFlags(Vector3Int.zero, TileAnimationFlags.None);
                    item.transform.position = Vector3.Lerp(start, end, elapsed / m_time);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                item.transform.position = end;
                m_occupied = false;
                m_currentItem = null;
                m_outputBuilding.ReceiveItem(item);
            }
            else
            {
                yield return new WaitUntil(() => m_outputBelt && !m_outputBelt.IsOccupied());
                m_occupied = false;
                m_currentItem = null;
                m_outputBelt.ReceiveItem(item);
            }
        }

        private void SyncTilemapAnimation()
        {
            var visited = new HashSet<Belt>();
            var queue = new Queue<Belt>();
            queue.Enqueue(this);
            visited.Add(this);
            while (queue.Count > 0)
            {
                var currentBelt = queue.Dequeue();
                m_building.BuildingTilemap.SetTileAnimationFlags(Vector3Int.zero, TileAnimationFlags.PauseAnimation);
                currentBelt.m_building.BuildingTilemap.SetAnimationFrame(Vector3Int.zero, 0);

                if (currentBelt.m_inputBelt != null && !visited.Contains(currentBelt.m_inputBelt))
                {
                    queue.Enqueue(currentBelt.m_inputBelt);
                    visited.Add(currentBelt.m_inputBelt);
                }

                if (currentBelt.m_outputBelt != null && !visited.Contains(currentBelt.m_outputBelt))
                {
                    queue.Enqueue(currentBelt.m_outputBelt);
                    visited.Add(currentBelt.m_outputBelt);
                }
            }
        }

        private void DetectBeltType()
        {
            DetectNeighbors();
            var newType = GetBeltType(m_inputDirection, m_outputDirection);
            MakeType(newType);
            SyncTilemapAnimation();

            if ((!m_inputBelt && m_inputBuilding == null && m_outputBelt && !m_outputBelt.m_inputBelt) || (!m_outputBelt && m_inputBelt && !m_inputBelt.OutputBelt && m_inputBelt.m_inputBuilding == null))
            {
                ReverseChain();
            }
        }

        private void DetectNeighbors()
        {
            var gridManager = GridManager.Instance;
            gridManager.WorldToGrid(transform.position, out var center);
            var directionVectors = DirectionVectors().ToList();
            for (int i = 0; i < directionVectors.Count(); i++)
            {
                for(int j = 1; j < directionVectors.Count(); j++)
                {
                    var k = (i + j) % directionVectors.Count();
                    var (dir1, vec1) = directionVectors[i];
                    var (dir2, vec2) = directionVectors[k];

                    var neighborPos1 = center + vec1;
                    var neighborPos2 = center + vec2;

                    if (m_inputBuilding == null && (!m_inputBelt || m_inputDirection == BeltDirection.None) && gridManager.TryGetBelt(neighborPos1, out var neighbor1) && (!neighbor1.m_outputBelt || neighbor1.m_outputBelt == this) && ((!neighbor1.m_outputBelt && neighbor1.m_outputBuilding == null) || neighbor1.m_outputBelt == this) && neighbor1 != m_outputBelt)
                    {
                        m_inputBelt = neighbor1;
                        m_inputDirection = dir1;
                        neighbor1.m_outputDirection = Opposite(dir1);
                        neighbor1.m_outputBelt = this;
                        if(neighbor1.m_inputDirection == BeltDirection.None)
                            neighbor1.m_inputDirection = dir1;

                        neighbor1.MakeType(GetBeltType(neighbor1.m_inputDirection, neighbor1.m_outputDirection));
                    }

                    if (!m_inputBelt && gridManager.TryGetBuilding(neighborPos1, out var buildingNeighbor1) &&
                        buildingNeighbor1.TryGetComponent<IConveyorEndpoint>(out var inputBuilding))
                    {
                        SetInputBuilding(inputBuilding, dir1);
                        inputBuilding.AddOutputBelt(this);
                    }

                    if (m_outputBuilding == null &&  (!m_outputBelt || m_outputDirection == BeltDirection.None) && gridManager.TryGetBelt(neighborPos2, out var neighbor2) && (!neighbor2.m_inputBelt || neighbor2.m_inputBelt == this) && ((!neighbor2.m_inputBelt && neighbor2.m_inputBuilding == null) || neighbor2.m_inputBelt == this) && neighbor2 != m_inputBelt)
                    {
                        m_outputBelt = neighbor2;
                        m_outputDirection = dir2;
                        neighbor2.m_inputDirection = Opposite(dir2);
                        neighbor2.m_inputBelt = this;
                        if(neighbor2.m_outputDirection == BeltDirection.None)
                            neighbor2.m_outputDirection = dir2;

                        neighbor2.MakeType(GetBeltType(neighbor2.m_inputDirection, neighbor2.m_outputDirection));
                    }

                    if (!m_outputBelt && gridManager.TryGetBuilding(neighborPos2, out var buildingNeighbor2) &&
                        buildingNeighbor2.TryGetComponent<IConveyorEndpoint>(out var outputBuilding))
                    {
                        SetOutputBuilding(outputBuilding, dir2);
                        outputBuilding.AddInputBelt(this);
                    }
                }
            }
        }

        public BeltType GetBeltType(BeltDirection input, BeltDirection output)
        {
            return (input, output) switch
            {
                (BeltDirection.North, BeltDirection.South) => BeltType.NorthSouth,
                (BeltDirection.South, BeltDirection.North) => BeltType.SouthNorth,
                (BeltDirection.East, BeltDirection.West) => BeltType.EastWest,
                (BeltDirection.West, BeltDirection.East) => BeltType.WestEast,
                (BeltDirection.East, BeltDirection.North) => BeltType.EastNorth,
                (BeltDirection.East, BeltDirection.South) => BeltType.EastSouth,
                (BeltDirection.North, BeltDirection.East) => BeltType.NorthEast,
                (BeltDirection.North, BeltDirection.West) => BeltType.NorthWest,
                (BeltDirection.South, BeltDirection.East) => BeltType.SouthEast,
                (BeltDirection.South, BeltDirection.West) => BeltType.SouthWest,
                (BeltDirection.West, BeltDirection.North) => BeltType.WestNorth,
                (BeltDirection.West, BeltDirection.South) => BeltType.WestSouth,
                _ => BeltType.Pole,
            };
        }

        private void MakeType(BeltType type)
        {
            m_beltType = type;

            if(m_inputDirection == BeltDirection.None)
                m_inputDirection = Opposite(m_outputDirection);
            if(m_outputDirection == BeltDirection.None)
                m_outputDirection = Opposite(m_inputDirection);

            m_beltType = GetBeltType(m_inputDirection, m_outputDirection);

            if (m_tiles.TryGetValue(m_beltType, out var tile) && this)
            {
                m_building.BuildingTilemap.SetTile(m_building.BuildingTilemap.WorldToCell(transform.position), tile);
                m_building.BuildingTilemap.SetTileAnimationFlags(m_building.BuildingTilemap.WorldToCell(transform.position), TileAnimationFlags.PauseAnimation);
            }
        }

        public static IEnumerable<(BeltDirection, Vector2Int)> DirectionVectors()
        {
            yield return (BeltDirection.North, Vector2Int.up);
            yield return (BeltDirection.East, Vector2Int.right);
            yield return (BeltDirection.South, Vector2Int.down);
            yield return (BeltDirection.West, Vector2Int.left);
        }

        public static BeltDirection Opposite(BeltDirection dir)
        {
            return dir switch
            {
                BeltDirection.North => BeltDirection.South,
                BeltDirection.South => BeltDirection.North,
                BeltDirection.East => BeltDirection.West,
                BeltDirection.West => BeltDirection.East,
                _ => BeltDirection.None
            };
        }
    }
}