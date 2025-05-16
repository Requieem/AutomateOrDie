using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Code.Scripts.Runtime.GameResources;
using UnityEngine;

namespace Code.Scripts.Runtime.UI
{
    public class ResourcesBar : MonoBehaviour
    {
        [SerializeField] private List<Item> m_trackedItems;
        [SerializeField] private SerializedDictionary<Item, bool> m_activeAtStart;
        [SerializeField] private ResourceUI m_itemPrefab;
        [SerializeField] private GameState m_gameState;
        [SerializeField] private List<Sprite> m_backgrounds;
        [SerializeField] private Sprite m_defaultBackground;

        [Header("Debugging")]
        [SerializeField] private SerializedDictionary<Item, ResourceUI> m_instances;

        private void Start()
        {
            m_instances ??= new SerializedDictionary<Item, ResourceUI>();
            m_instances.Clear();
            foreach (var trackedItem in m_trackedItems)
            {
                if (!m_gameState.PercentageEvents.TryGetValue(trackedItem, out var unityEvent)) continue;
                var item = Instantiate(m_itemPrefab, transform);
                item.SetIcon(trackedItem.Sprite);
                item.SetFill(m_gameState.CurrentItems[trackedItem] / m_gameState.MaxItems[trackedItem]);
                var activeAtStart = m_activeAtStart.TryGetValue(trackedItem, out var isActive) && isActive;
                unityEvent.AddListener(item.SetFill);
                item.gameObject.SetActive(activeAtStart);
                m_instances.Add(trackedItem, item);
            }

            m_gameState.OnItemAppeared.AddListener(Show);
            AdjustBackgrounds();
        }

        private void AdjustBackgrounds()
        {
            var keys = m_instances.Keys.ToList();
            var keyCount = keys.Count;
            var activeCount = -1;
            ResourceUI firstInstance = null;
            ResourceUI lastInstance = null;
            for (int i = 0; i < keyCount; i++)
            {
                if(m_instances[keys[i]].gameObject.activeInHierarchy) activeCount++;
                else continue;

                var instance = m_instances[keys[i]];
                var background = m_defaultBackground;

                if (activeCount == 0)
                {
                    firstInstance = instance;
                }
                else if (activeCount < m_backgrounds.Count )
                {
                    background = m_backgrounds[activeCount];
                }
                lastInstance = instance;
                instance.SetBackground(background);
            }

            if (activeCount <= 0) return;
            firstInstance?.SetBackground(m_backgrounds[0]);
            lastInstance?.SetBackground(m_backgrounds[^1]);
        }

        private void OnDestroy()
        {
            m_gameState.OnItemAppeared.RemoveListener(Show);
            foreach (var trackedItem in m_trackedItems)
            {
                if (!m_gameState.PercentageEvents.TryGetValue(trackedItem, out var unityEvent)) continue;
                unityEvent.RemoveListener(m_instances[trackedItem].SetFill);
            }
        }

        public void Show(Item item) => Show(item, true);

        public void Show(Item item, bool show)
        {
            if (!m_instances.TryGetValue(item, out var instance)) return;
            instance.gameObject.SetActive(show);
            AdjustBackgrounds();
        }
    }
}
