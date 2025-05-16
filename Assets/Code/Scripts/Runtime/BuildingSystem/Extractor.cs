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

        private (Item item, float rate) m_currentExtraction;
        private MonoItem m_currentItem;
        public override bool AcceptsInput => false;
        public override bool IsOccupied() => m_amount >= m_capacity;

        protected override void Setup()
        {
            base.Setup();
            DetectResource();
            StartCoroutine(Extract());
            StartCoroutine(Distribute());
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
                yield return new WaitUntil(() => m_output);

                var newItem = Instantiate(m_currentExtraction.item.ItemPrefab, transform.position, Quaternion.identity);
                newItem.Initialize(m_currentExtraction.item);
                m_currentItem = newItem;
                m_output.ReceiveItem(newItem);
                m_currentItem = null;
                m_amount--;
            }
        }

        protected override void CleanUp()
        {
            StopAllCoroutines();

            // Destroy any item that is currently in the extractor
            if (m_currentItem && m_currentItem.gameObject)
            {
                Destroy(m_currentItem.gameObject);
                m_currentItem = null;
            }
        }

        public override void ReceiveItem(MonoItem item)
        {
            // Extractors don't receive items, but we override for interface compatibility
        }
    }
}