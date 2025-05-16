using System.Collections;
using AYellowpaper.SerializedCollections;
using Code.Scripts.Common;
using Code.Scripts.Runtime.GameResources;
using Code.Scripts.Runtime.Grid;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public class Extractor : ConveyorBuilding
    {
        [SerializeField] private SerializedDictionary<Tile, Item> m_resources;
        [SerializeField] private SerializedDictionary<Item, float> m_extractionRates;
        [SerializeField] private int m_capacity;
        [SerializeField] private int m_amount;
        [SerializeField] private int m_beltIndex;

        private (Item item, float rate) m_currentExtraction;

        private void Start()
        {
            DetectResource();
            DetectBelts();
            StartCoroutine(Extract());
            StartCoroutine(Distribute());
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

                if (belt.InputBuilding != null || belt.InputBelt != null) continue;
                if(belt.SetInputBuilding(this, Belt.Opposite(dir.Item1)))
                    AddOutputBelt(belt);
            }
        }

        private void DetectResource()
        {
            var gridManager = GridManager.Instance;
            var worldPos = transform.position;
            gridManager.TrySnapPosition(worldPos.ToVector2(), out var snappedPos);
            gridManager.WorldToGrid(snappedPos, out var gridPos);
            gridManager.TryGetResource(gridPos, out var resource);
            if (resource == null) return;
            if (m_resources.TryGetValue(resource, out var item))
            {
                m_currentExtraction = (item, m_extractionRates[item]);
                Debug.Log($"Detected resource: {resource.name} -> {item.name}");
            }
            else
            {
                Debug.Log($"No matching item for resource: {resource.name}");
            }
        }

        private IEnumerator Extract()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(m_currentExtraction.rate);
                yield return new WaitUntil(() => m_amount < m_capacity);
                m_amount++;
            }
        }

        private IEnumerator Distribute()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(m_currentExtraction.rate);
                yield return new WaitUntil(() => m_amount > 0);
                yield return new WaitUntil(() => m_beltIndex >= 0 && m_beltIndex < m_outputBelts.Count);

                var newItem = Instantiate(m_currentExtraction.item.ItemPrefab, transform.position, Quaternion.identity);
                newItem.Initialize(m_currentExtraction.item);
                var outputBelt = m_outputBelts[m_beltIndex];
                while (outputBelt.IsOccupied())
                {
                    yield return new WaitUntil(() => m_outputBelts.Count < 0);
                    m_beltIndex = (m_beltIndex + 1) % m_outputBelts.Count;
                    outputBelt = m_outputBelts[m_beltIndex];
                    yield return null;
                }
                outputBelt.ReceiveItem(newItem);
                m_amount--;
                m_beltIndex = (m_beltIndex + 1) % m_outputBelts.Count;
            }
        }

        public override void ReceiveItem(MonoItem item)
        {
            // Extractors don't receive items, but we override for interface compatibility
        }
    }
}