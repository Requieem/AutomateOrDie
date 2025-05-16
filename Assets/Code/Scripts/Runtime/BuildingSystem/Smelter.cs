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
        private readonly Queue<Item> m_inputQueue = new();

        private MonoItem m_currentItem;

        protected override void Setup()
        {
            base.Setup();
            StartCoroutine(RefineAndDistribute());
        }

        public override bool IsOccupied()
        {
            return false;
        }

        public override void ReceiveItem(MonoItem item)
        {
            m_inputQueue.Enqueue(item.Item);

            if(item.gameObject)
                Destroy(item.gameObject); // Smelter consumes the item
        }

        protected override void CleanUp()
        {
            if(m_currentItem && m_currentItem.gameObject)
                Destroy(m_currentItem.gameObject);
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
                m_currentItem = newItem;
                newItem.Initialize(refinedItem);

                yield return new WaitUntil(() => m_output && !m_output.IsOccupied());
                m_output.ReceiveItem(newItem);
                m_currentItem = null;
            }
        }
    }
}