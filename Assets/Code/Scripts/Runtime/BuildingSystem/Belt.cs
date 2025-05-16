using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Code.Scripts.Runtime.GameResources;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public class Belt : ConveyorBuilding
    {
        [SerializeField] private Building m_building;
        [SerializeField] private BeltType m_beltType;
        [SerializeField] private SerializedDictionary<BeltType, TileBase> m_tiles;
        [SerializeField] private float m_time = 0.15f;

        private bool m_occupied;

        private readonly Queue<MonoItem> m_itemQueue = new();
        private MonoItem m_currentItem;

        public BeltType Type => m_beltType;
        public Building Building => m_building;

        public override bool IsOccupied() => m_occupied;
        public override void ReceiveItem(MonoItem item)
        {
            m_itemQueue.Enqueue(item);
            if (!m_occupied)
            {
                StartCoroutine(ProcessQueue());
            }
        }
        public override void AddInput(ConveyorBuilding input, ConveyDirection direction)
        {
            base.AddInput(input, direction);
            MakeType(GetBeltType(m_inputDirection, m_outputDirection));
        }
        public override void AddOutput(ConveyorBuilding output, ConveyDirection direction)
        {
            base.AddOutput(output, direction);
            MakeType(GetBeltType(m_inputDirection, m_outputDirection));
        }
        public override void RemoveInput(ConveyorBuilding input)
        {
            base.RemoveInput(input);
            MakeType(GetBeltType(m_inputDirection, m_outputDirection));
        }
        public override void RemoveOutput(ConveyorBuilding output)
        {
            base.RemoveOutput(output);
            MakeType(GetBeltType(m_inputDirection, m_outputDirection));
        }

        protected override void CleanUp()
        {
            StopAllCoroutines();
            // Destroy any item currently on the belt or in the queue
            while (m_itemQueue.Count > 0)
            {
                var item = m_itemQueue.Dequeue();
                Destroy(item.gameObject);
            }

            if (!m_currentItem || !m_currentItem.gameObject) return;
            Destroy(m_currentItem.gameObject);
            m_currentItem = null;
        }
        private void SyncTilemapAnimation()
        {
            var visited = new HashSet<ConveyorBuilding>();
            var queue = new Queue<ConveyorBuilding>();
            queue.Enqueue(this);
            visited.Add(this);
            while (queue.Count > 0)
            {
                var currentBelt = queue.Dequeue();
                if (currentBelt is not Belt belt) continue;
                belt.m_building.BuildingTilemap.SetTileAnimationFlags(Vector3Int.zero, TileAnimationFlags.SyncAnimation);
                belt.m_building.BuildingTilemap.SetAnimationFrame(Vector3Int.zero, 0);

                if (currentBelt.Input != null && !visited.Contains(currentBelt.Input))
                {
                    queue.Enqueue(currentBelt.Input);
                    visited.Add(currentBelt.Input);
                }

                if (currentBelt.Output != null && !visited.Contains(currentBelt.Output))
                {
                    queue.Enqueue(currentBelt.Output);
                    visited.Add(currentBelt.Output);
                }
            }
        }
        private void MakeType(BeltType type)
        {
            m_beltType = type;

            if(m_inputDirection == ConveyDirection.None)
                m_inputDirection = ConveyorUtility.Opposite(m_outputDirection);
            if(m_outputDirection == ConveyDirection.None)
                m_outputDirection = ConveyorUtility.Opposite(m_inputDirection);

            m_beltType = GetBeltType(m_inputDirection, m_outputDirection);

            if (!m_tiles.TryGetValue(m_beltType, out var tile) || !this) return;
            m_building.BuildingTilemap.SetTile(m_building.BuildingTilemap.WorldToCell(transform.position), tile);
            SyncTilemapAnimation();
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
                item.transform.position = Vector3.Lerp(start, end, elapsed / m_time);
                elapsed += Time.deltaTime;
                yield return null;
            }

            item.transform.position = end;
            yield return new WaitUntil(() => m_output && !m_output.IsOccupied());
            m_occupied = false;
            m_currentItem = null;
            m_output.ReceiveItem(item);
        }
        private static BeltType GetBeltType(ConveyDirection input, ConveyDirection output)
        {
            return (input, output) switch
            {
                (ConveyDirection.North, ConveyDirection.South) => BeltType.NorthSouth,
                (ConveyDirection.South, ConveyDirection.North) => BeltType.SouthNorth,
                (ConveyDirection.East, ConveyDirection.West) => BeltType.EastWest,
                (ConveyDirection.West, ConveyDirection.East) => BeltType.WestEast,
                (ConveyDirection.East, ConveyDirection.North) => BeltType.EastNorth,
                (ConveyDirection.East, ConveyDirection.South) => BeltType.EastSouth,
                (ConveyDirection.North, ConveyDirection.East) => BeltType.NorthEast,
                (ConveyDirection.North, ConveyDirection.West) => BeltType.NorthWest,
                (ConveyDirection.South, ConveyDirection.East) => BeltType.SouthEast,
                (ConveyDirection.South, ConveyDirection.West) => BeltType.SouthWest,
                (ConveyDirection.West, ConveyDirection.North) => BeltType.WestNorth,
                (ConveyDirection.West, ConveyDirection.South) => BeltType.WestSouth,
                _ => BeltType.Pole,
            };
        }

    }
}