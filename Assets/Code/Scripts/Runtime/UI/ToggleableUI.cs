using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Code.Scripts.Common;
using Code.Scripts.Common.MyGame.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.Runtime.UI
{
    public class ToggleableUI : MonoBehaviour
    {
        public enum LoopType { None, Loop, PingPong }

        [SerializeField] private RectTransform m_rectTransform;
        [SerializeField] private Vector2[] m_offsets;
        [SerializeField] private LoopType m_loopType;
        [SerializeField] private SerializedDictionary<int, EaseType> m_easeTypes;
        [SerializeField] private SerializedDictionary<int, float> m_durations;
        [SerializeField] private float m_fallbackDuration = 0.5f;
        [SerializeField] private EaseType m_fallbackEaseType = EaseType.Linear;
        [SerializeField] private InputActionReference m_startAction;
        [SerializeField] private InputActionReference m_stopAction;
        [SerializeField] private bool m_toggleMode = true;

        [Header("Debugging")]
        [SerializeField] private float m_elapsedTime;
        [SerializeField] private Vector2 m_initialPosition;
        [SerializeField] private float m_lastDuration;
        [SerializeField] private int m_state;
        [SerializeField] private int m_direction;

        public Vector2[] Offsets => m_offsets;

        private void Awake()
        {
            m_initialPosition = m_rectTransform.anchoredPosition;
            m_direction = 1;
        }

        private void OnEnable()
        {
            m_startAction.action.performed += OnStart;
            m_stopAction.action.performed += OnStop;
        }

        private void OnDisable()
        {
            m_stopAction.action.performed -= OnStart;
            m_startAction.action.performed -= OnStop;
        }

        private void OnStart(InputAction.CallbackContext context)
        {
            if(!context.performed || !this || !gameObject) return;
            Move();
        }

        private void OnStop(InputAction.CallbackContext context)
        {
            if(!context.performed || !this || !gameObject || m_toggleMode) return;
            OnStart(context);
        }

        private Vector3 GetPosition(int state)
        {
            if (state == 0) return m_initialPosition;
            if (state < 0 || state > m_offsets.Length)
                return m_rectTransform.anchoredPosition;

            var totalOffset = Vector2.zero;
            for (var i = 0; i < state; i++)
                totalOffset += m_offsets[i];

            return m_initialPosition + totalOffset;
        }

        private (int state, int direction) TryMove(int currentState, int currentDirection)
        {
            if (currentState == 0 && currentDirection < 0 && m_loopType == LoopType.Loop)
                return (m_offsets.Length, currentDirection);
            if (currentState == m_offsets.Length && currentDirection > 0 && m_loopType == LoopType.Loop)
                return (0, currentDirection);
            if (currentState == 0 && currentDirection < 0 && m_loopType == LoopType.PingPong)
                return (1, -currentDirection);
            if (currentState == m_offsets.Length && currentDirection > 0 && m_loopType == LoopType.PingPong)
                return (m_offsets.Length - 1, -currentDirection);

            return (currentState + currentDirection, currentDirection);
        }

        private float GetAdditionalTime((int state, int direction) oldState, (int state, int direction) newState)
        {
            if (oldState == newState || newState.direction == 0)
                return m_fallbackDuration;

            var clampedElapsed = Mathf.Clamp01(m_elapsedTime / m_lastDuration);
            var remaining = 1f - clampedElapsed;

            return oldState.direction == newState.direction
                ? m_lastDuration * remaining
                : m_lastDuration * clampedElapsed;
        }

        public void Move() => Move(m_direction <= 0);
        public void Move(int target)
        {
            StopAllCoroutines();
            StartCoroutine(MultiOffset(target));
        }
        public void Move(bool forward)
        {
            StopAllCoroutines();
            StartCoroutine(Offset(forward ? 1 : -1));
        }

        private IEnumerator MultiOffset(int target)
        {
            if (target < 0 || target > m_offsets.Length) yield break;

            var step = target < m_state ? -1 : 1;
            while (m_state != target)
            {
                yield return StartCoroutine(Offset(step));
            }
        }

        private IEnumerator Offset(int direction)
        {
            var prevState = m_state;
            var prevDirection = m_direction;

            m_direction = direction;
            var (nextState, nextDirection) = TryMove(m_state, m_direction);

            if (nextState == m_state || nextDirection == 0)
                yield break;

            m_state = nextState;
            m_lastDuration = m_durations.GetValueOrDefault(nextState, m_fallbackDuration);
            var additionalTime = GetAdditionalTime((prevState, prevDirection), (nextState, nextDirection));
            m_lastDuration += additionalTime;
            var easeType = m_easeTypes.GetValueOrDefault(nextState, m_fallbackEaseType);
            var start = m_rectTransform.anchoredPosition;
            var target = GetPosition(nextState);

            m_elapsedTime = 0f;

            while (m_elapsedTime < m_lastDuration)
            {
                var t = Mathf.Clamp01(m_elapsedTime / m_lastDuration);
                m_rectTransform.anchoredPosition = Vector3.Lerp(start, target, t.Ease(easeType));
                m_elapsedTime += Time.deltaTime;
                yield return null;
            }

            m_rectTransform.anchoredPosition = target;
        }

        private void OnDrawGizmosSelected()
        {
            if (!m_rectTransform || m_offsets == null || m_offsets.Length < 1) return;
            var current = m_rectTransform.position.ToVector2();
            if(Application.isPlaying)
                current = m_initialPosition;

            var color = Color.HSVToRGB(0f, .75f, 0.65f);
            for (var i = 0; i < m_offsets.Length; i++)
            {
                var next = current + m_offsets[i];
                Gizmos.color = color;
                color = Color.HSVToRGB(0.05f * i, .5f, .65f);
                Gizmos.DrawLine(current, next);
                Gizmos.DrawWireCube(next, m_rectTransform.sizeDelta);
                current = next;
            }
        }
    }
}