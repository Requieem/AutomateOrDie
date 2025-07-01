using System.Collections;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Code.Scripts.Common;
using Code.Scripts.Common.MyGame.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Code.Scripts.Runtime.GameResources
{
    public class GameState : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField] private SerializedDictionary<Item, float> m_startingItems;
        [SerializeField] private SerializedDictionary<Item, float> m_maxItems;
        [SerializeField] private SerializedDictionary<Item, float> m_usageRates;
        [SerializeField] private SerializedDictionary<Item, float> m_appearanceTimes;
        [SerializeField] private SerializedDictionary<Item, float> m_abundanceThresholds;
        [SerializeField] private SerializedDictionary<Item, float> m_scarcityThresholds;
        [SerializeField] private SerializedDictionary<Item, float> m_acquiredItems;
        [SerializeField] private ResourceState m_abundantState;
        [SerializeField] private ResourceState m_scarceState;
        [SerializeField] private ResourceState m_normalState;
        [SerializeField] private TextMeshProUGUI m_alertText;
        [SerializeField] private TextMeshProUGUI m_timerText;
        [SerializeField] private TextMeshProUGUI m_endTimerText;
        [SerializeField] private AudioClip m_alertSound;
        [SerializeField] private AudioClip m_lossSound;
        [SerializeField] private GameObject m_endMenu;
        [SerializeField] private float m_alertSmoothTime = 0.25f;
        [SerializeField] private float m_alertDuration = 5f;
        [SerializeField] private bool m_lost = false;

        [Header("Debugging")] [SerializeField] private SerializedDictionary<Item, float> m_currentItems;
        [SerializeField] private SerializedDictionary<Item, ResourceState> m_currentStates;
        [SerializeField] private SerializedDictionary<Item, bool> m_appearanceMap;
        [SerializeField] private SerializedDictionary<Item, UnityEvent<float>> m_valueEvents;
        [SerializeField] private SerializedDictionary<Item, UnityEvent<float>> m_percentageEvents;
        [SerializeField] private SerializedDictionary<Item, UnityEvent<float>> m_acquiredEvents;
        [SerializeField] private UnityEvent<Item> m_onItemAppeared = new UnityEvent<Item>();
        [SerializeField] private bool m_performFastForward = false;
        private Camera m_camera;

        public SerializedDictionary<Item, float> CurrentItems => m_currentItems;
        public SerializedDictionary<Item, float> MaxItems => m_maxItems;
        public SerializedDictionary<Item, ResourceState> CurrentStates => m_currentStates;
        public SerializedDictionary<Item, float> UsageRates => m_usageRates;
        public SerializedDictionary<Item, float> AbundanceThresholds => m_abundanceThresholds;
        public SerializedDictionary<Item, float> ScarcityThresholds => m_scarcityThresholds;
        public SerializedDictionary<Item, float> AcquiredItems => m_acquiredItems;
        public SerializedDictionary<Item, float> AppearanceTimes => m_appearanceTimes;
        public SerializedDictionary<Item, UnityEvent<float>> ValueEvents => m_valueEvents;
        public SerializedDictionary<Item, UnityEvent<float>> AcquiredEvents => m_acquiredEvents;
        public SerializedDictionary<Item, UnityEvent<float>> PercentageEvents => m_percentageEvents;
        public UnityEvent<Item> OnItemAppeared => m_onItemAppeared;
        public SerializedDictionary<Item, float> StartingItems => m_startingItems;
        public bool Lost => m_lost;

        public static GameState Instance { get; private set; }

        private void OnEnable()
        {
            m_valueEvents ??= new SerializedDictionary<Item, UnityEvent<float>>();
            m_percentageEvents ??= new SerializedDictionary<Item, UnityEvent<float>>();
            m_acquiredEvents ??= new SerializedDictionary<Item, UnityEvent<float>>();
            m_startingItems ??= new SerializedDictionary<Item, float>();

            foreach (var item in m_startingItems)
            {
                if (!m_valueEvents.TryAdd(item.Key, new UnityEvent<float>()))
                {
                    m_valueEvents[item.Key]?.RemoveAllListeners();
                }

                if (!m_percentageEvents.TryAdd(item.Key, new UnityEvent<float>()))
                {
                    m_percentageEvents[item.Key]?.RemoveAllListeners();
                }

                if (!m_acquiredEvents.TryAdd(item.Key, new UnityEvent<float>()))
                {
                    m_acquiredEvents[item.Key]?.RemoveAllListeners();
                }
            }

            m_usageRates ??= new SerializedDictionary<Item, float>();
            m_abundanceThresholds ??= new SerializedDictionary<Item, float>();
            m_scarcityThresholds ??= new SerializedDictionary<Item, float>();

            m_currentItems ??= new SerializedDictionary<Item, float>();
            m_currentItems.Clear();

            foreach (var item in m_startingItems)
            {
                m_currentItems.Add(item.Key, item.Value);
            }

            m_currentStates ??= new SerializedDictionary<Item, ResourceState>();
            m_currentStates.Clear();

            foreach (var item in m_startingItems)
            {
                if (m_abundanceThresholds.TryGetValue(item.Key, out var abundanceThreshold) &&
                    m_scarcityThresholds.TryGetValue(item.Key, out var scarcityThreshold))
                {
                    if (item.Value >= abundanceThreshold)
                    {
                        m_currentStates.Add(item.Key, m_abundantState);
                    }
                    else if (item.Value <= scarcityThreshold)
                    {
                        m_currentStates.Add(item.Key, m_scarceState);
                    }
                    else
                    {
                        m_currentStates.Add(item.Key, m_normalState);
                    }
                }
                else
                {
                    Debug.LogError($"[GameState] Missing thresholds for item: {item.Key.name}");
                }
            }

            m_appearanceMap ??= new SerializedDictionary<Item, bool>();
            m_appearanceMap.Clear();

            foreach (var item in m_appearanceTimes)
            {
                m_appearanceMap.Add(item.Key, item.Value <= 0f);
            }

            m_onItemAppeared.AddListener(SetAppeared);
        }
        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        private void Start()
        {
            m_camera = Camera.main;
            StartGame();
        }
        private void Update()
        {
            if (Lost) return;

            // if any three items are below 0, show end menu
            var scarceItems = m_currentItems.Where(x => x.Value <= 0f).ToList();
            if (scarceItems.Count < 3) return;
            m_lost = true;
            m_endMenu.SetActive(true);
            AudioSource.PlayClipAtPoint(m_lossSound, m_camera.transform.position, 0.25f);
            StopAllCoroutines();
            Time.timeScale = 0f;
            StartCoroutine(UpdateResults());
        }
        private void OnDisable()
        {
            StopAllCoroutines();
            m_onItemAppeared.RemoveListener(SetAppeared);
        }

        private IEnumerator UpdateResults()
        {
            yield return new WaitForSecondsRealtime(0.1f + Time.unscaledDeltaTime);
            foreach (var acquiredItem in m_acquiredItems)
            {
                var item = acquiredItem.Key;
                var acquired = acquiredItem.Value;
                if (!m_acquiredEvents.TryGetValue(item, out var gameEvent)) continue;
                gameEvent.Invoke(acquired);
            }
        }
        public void SetAppeared(Item item)
        {
            m_appearanceMap[item] = true;
        }
        public void StartGame()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            foreach (var (item, gameEvent) in m_valueEvents)
            {
                if (gameEvent == null || !item) continue;
                if (!m_currentItems.TryGetValue(item, out var currentAmount)) continue;
                gameEvent.Invoke(currentAmount);
            }

            foreach (var (item, gameEvent) in m_percentageEvents)
            {
                if (gameEvent == null || !item) continue;
                if (!m_currentItems.TryGetValue(item, out var currentAmount)) continue;
                if (!m_maxItems.TryGetValue(item, out var maxAmount)) continue;
                gameEvent.Invoke(currentAmount / maxAmount);
            }

            foreach (var (item, gameEvent) in m_acquiredEvents)
            {
                if (gameEvent == null || !item) continue;
                if (!m_acquiredItems.TryGetValue(item, out var acquiredAmount)) continue;
                gameEvent.Invoke(acquiredAmount);
            }

            StartCoroutine(Tick());
            StartCoroutine(ConsumeResources());
        }
        private IEnumerator Tick()
        {
            var lastTime = 0f;
            var elapsedTime = 0f;
            while (enabled)
            {
                var target = m_appearanceTimes.Count;
                var count = 0;
                if (count != target)
                {
                    foreach (var appearance in m_appearanceTimes)
                    {
                        if (!(appearance.Value <= elapsedTime)) continue;
                        count++;

                        if (!(appearance.Value > lastTime)) continue;
                        m_onItemAppeared.Invoke(appearance.Key);
                        StartCoroutine(ShowAlert(appearance.Key));
                    }
                }

                lastTime = elapsedTime;
                elapsedTime += ElapseTime();
                // show timer text in format MM:SS
                var timeString = $"{(int)elapsedTime / 60:00}:{elapsedTime % 60:00}";
                m_timerText.text = timeString;
                m_endTimerText.text = $"<color=red>SHIFT</color> DURATION\n{timeString}";
                yield return null;
            }
        }
        private IEnumerator ScaleText(bool show)
        {
            var elapsedTime = 0f;
            var targetScale = show ? Vector3.one : Vector3.zero;
            var startScale = m_alertText.transform.localScale;
            while (elapsedTime < m_alertSmoothTime)
            {
                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / m_alertSmoothTime);
                m_alertText.transform.localScale = Vector3.Lerp(startScale, targetScale, t.EaseOutBounce());
                yield return null;
            }
        }
        private IEnumerator ShowAlert(Item item)
        {
            m_alertText.text = $"Automate {item.name} or <color=\"red\">DIE</color>!";
            m_alertText.transform.localScale = Vector3.zero;
            yield return ScaleText(true);

            if (m_camera)
                AudioSource.PlayClipAtPoint(m_alertSound, m_camera.transform.position, 0.75f);

            yield return new WaitForSeconds(m_alertDuration);
            yield return ScaleText(false);
            m_alertText.text = string.Empty;
            m_alertText.transform.localScale = Vector3.zero;
        }
        private IEnumerator ConsumeResources()
        {
            while (enabled)
            {
                var keys = m_currentItems.Keys.ToList();
                for (var i = 0; i < keys.Count; i++)
                {
                    var item = keys[i];
                    if (!m_currentItems.TryGetValue(item, out var currentAmount)) continue;
                    if (!m_appearanceMap.TryGetValue(item, out var hasAppeared) || !hasAppeared) continue;
                    if (currentAmount <= 0f) continue;
                    UseResource(item, m_usageRates[item] * ElapseTime());
                }

                yield return null;
            }
        }
        private float ElapseTime()
        {
            if (m_performFastForward) return Time.deltaTime * 10f;
            return Time.deltaTime;
        }
        public void UseResource(Item item, float amount)
        {
            if (!m_currentItems.TryGetValue(item, out var currentAmount)) return;
            if (!m_maxItems.TryGetValue(item, out var maxAmount)) return;

            var newAmount = currentAmount - amount;
            newAmount = Mathf.Max(newAmount, 0f);
            newAmount = Mathf.Min(newAmount, maxAmount);
            if (Mathf.Approximately(newAmount, currentAmount)) return;
            m_currentItems[item] = newAmount;
            m_valueEvents[item].Invoke(newAmount);
            m_percentageEvents[item].Invoke(newAmount / maxAmount);
            if (!(amount < 0f)) return;
            m_acquiredItems.TryAdd(item, 0f);
            m_acquiredItems[item] -= amount;
            m_acquiredEvents[item].Invoke(m_acquiredItems[item]);
        }
        public void OnBeforeSerialize()
        {
            //throw new System.NotImplementedException();
        }
        public void OnAfterDeserialize()
        {
            foreach (var startingItem in m_startingItems)
            {
                m_maxItems.TryAdd(startingItem.Key, 0f);
                m_acquiredItems.TryAdd(startingItem.Key, 0f);
                m_usageRates.TryAdd(startingItem.Key, 0f);
                m_abundanceThresholds.TryAdd(startingItem.Key, 0f);
                m_scarcityThresholds.TryAdd(startingItem.Key, 0f);
                m_appearanceTimes.TryAdd(startingItem.Key, 0f);
            }

            var maxKeys = m_maxItems.Keys.ToList();
            for (var i = maxKeys.Count - 1; i >= 0; i--)
            {
                if (!m_startingItems.ContainsKey(maxKeys[i]))
                {
                    m_maxItems.Remove(maxKeys[i]);
                }
            }

            var usageRateKeys = m_usageRates.Keys.ToList();
            for (var i = usageRateKeys.Count - 1; i >= 0; i--)
            {
                if (!m_startingItems.ContainsKey(usageRateKeys[i]))
                {
                    m_usageRates.Remove(usageRateKeys[i]);
                }
            }

            var abundanceThresholdKeys = m_abundanceThresholds.Keys.ToList();
            for (var i = abundanceThresholdKeys.Count - 1; i >= 0; i--)
            {
                if (!m_startingItems.ContainsKey(abundanceThresholdKeys[i]))
                {
                    m_abundanceThresholds.Remove(abundanceThresholdKeys[i]);
                }
            }

            var scarcityThresholdKeys = m_scarcityThresholds.Keys.ToList();
            for (var i = scarcityThresholdKeys.Count - 1; i >= 0; i--)
            {
                if (!m_startingItems.ContainsKey(scarcityThresholdKeys[i]))
                {
                    m_scarcityThresholds.Remove(scarcityThresholdKeys[i]);
                }
            }

            var appearanceTimeKeys = m_appearanceTimes.Keys.ToList();
            for (var i = appearanceTimeKeys.Count - 1; i >= 0; i--)
            {
                if (!m_startingItems.ContainsKey(appearanceTimeKeys[i]))
                {
                    m_appearanceTimes.Remove(appearanceTimeKeys[i]);
                }
            }

            var acquiredItemsKeys = m_acquiredItems.Keys.ToList();
            for (var i = acquiredItemsKeys.Count - 1; i >= 0; i--)
            {
                if (!m_startingItems.ContainsKey(acquiredItemsKeys[i]))
                {
                    m_appearanceTimes.Remove(acquiredItemsKeys[i]);
                }
            }
        }
    }
}