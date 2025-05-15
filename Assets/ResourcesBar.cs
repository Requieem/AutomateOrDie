using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Code.Scripts.Runtime.GameResources;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesBar : MonoBehaviour
{
    [SerializeField] private List<Item> m_trackedItems;
    [SerializeField] private SerializedDictionary<Item, bool> m_activeAtStart;
    [SerializeField] private ResourceUI m_itemPrefab;
    [SerializeField] private GameState m_gameState;

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
    }
}
