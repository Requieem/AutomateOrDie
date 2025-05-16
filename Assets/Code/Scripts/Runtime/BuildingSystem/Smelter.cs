using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Code.Scripts.Common;
using Code.Scripts.Runtime.GameResources;
using Code.Scripts.Runtime.Grid;
using UnityEngine;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public class Smelter : ConveyorBuilding
    {
        [SerializeField] private SerializedDictionary<Item, Item> m_refinedItems;
        [SerializeField] private float m_refineTime = 1f;

        private Queue<Item> m_inputQueue = new();
        private int m_beltIndex;

        private void Start()
        {
            DetectBelts();
            StartCoroutine(RefineAndDistribute());
        }

        private void DetectBelts()
        {
            var gridManager = GridManager.Instance;
            var worldPos = transform.position;
            gridManager.TrySnapPosition(worldPos.ToVector2(), out var snappedPos);
            gridManager.WorldToGrid(snappedPos, out var gridPos);
            foreach (var dir in Belt.DirectionVectors())
            {
                var neighborPos = gridPos + dir.Item2;
                if (!gridManager.TryGetBelt(neighborPos, out var belt)) continue;

                if (belt.InputBuilding == null && belt.InputBelt == null)
                {
                    m_outputBelts.Add(belt);
                    belt.SetInputBuilding(this, Belt.Opposite(dir.Item1));
                }
                else if (belt.OutputBelt == null && belt.OutputBuilding == null)
                {
                    m_inputBelts.Add(belt);
                    belt.SetOutputBuilding(this, Belt.Opposite(dir.Item1));
                }
            }
        }

        public override void ReceiveItem(MonoItem item)
        {
            m_inputQueue.Enqueue(item.Item);
            Destroy(item.gameObject); // Smelter consumes the item
        }

        private IEnumerator RefineAndDistribute()
        {
            while (enabled)
            {
                yield return new WaitUntil(() => m_inputQueue.Count > 0);

                var rawItem = m_inputQueue.Dequeue();
                if (!m_refinedItems.TryGetValue(rawItem, out var refinedItem)) continue;

                yield return new WaitForSeconds(m_refineTime);

                var newItem = Instantiate(refinedItem.ItemPrefab, transform.position, Quaternion.identity);
                newItem.Initialize(refinedItem);

                yield return new WaitUntil(() => m_outputBelts.Count > 0);

                var outputBelt = m_outputBelts[m_beltIndex];
                while (outputBelt.IsOccupied())
                {
                    m_beltIndex = (m_beltIndex + 1) % m_outputBelts.Count;
                    outputBelt = m_outputBelts[m_beltIndex];
                    yield return null;
                }

                outputBelt.ReceiveItem(newItem);
                m_beltIndex = (m_beltIndex + 1) % m_outputBelts.Count;
            }
        }
    }
}