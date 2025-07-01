using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Code.Scripts.Common.MyGame.Extensions;
using Unity.Cinemachine;

namespace Code.Scripts.Runtime.Camera
{
    public class ToggleableZoom : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera m_virtualCamera;
        [SerializeField] private float m_minZoom = 2f;
        [SerializeField] private float m_maxZoom = 10f;
        [SerializeField] private float m_duration = 0.5f;
        [SerializeField] private EaseType m_easeType = EaseType.Linear;
        [SerializeField] private InputActionReference m_startAction;
        [SerializeField] private InputActionReference m_stopAction;
        [SerializeField] private bool m_toggleMode = true;

        [Header("Debugging")]
        [SerializeField] private float m_elapsedTime;
        [SerializeField] private float m_initialZoom;
        [SerializeField] private float m_targetZoom;
        [SerializeField] private bool m_zoomedIn;

        private Coroutine m_currentZoomRoutine;

        private void Awake()
        {
            if (m_virtualCamera == null)
                m_virtualCamera = GetComponent<CinemachineCamera>();

            m_initialZoom = m_virtualCamera.Lens.OrthographicSize;
            m_zoomedIn = false;
        }

        private void OnEnable()
        {
            m_startAction.action.performed += OnStart;
            m_stopAction.action.performed += OnStop;
        }

        private void OnDisable()
        {
            m_startAction.action.performed -= OnStart;
            m_stopAction.action.performed -= OnStop;
        }

        private void OnStart(InputAction.CallbackContext context)
        {
            if (!context.performed || !this || !gameObject) return;
            ToggleZoom();
        }

        private void OnStop(InputAction.CallbackContext context)
        {
            if (!context.performed || !this || !gameObject || m_toggleMode) return;
            OnStart(context);
        }

        public void ToggleZoom()
        {
            var target = !m_zoomedIn ? m_initialZoom : m_minZoom;
            StartZoom(target);
            m_zoomedIn = !m_zoomedIn;
        }

        public void StartZoom(float targetZoom)
        {
            if (m_currentZoomRoutine != null)
                StopCoroutine(m_currentZoomRoutine);

            m_currentZoomRoutine = StartCoroutine(ZoomTo(targetZoom));
        }

        private IEnumerator ZoomTo(float targetZoom)
        {
            var startZoom = m_virtualCamera.Lens.OrthographicSize;
            m_targetZoom = Mathf.Clamp(targetZoom, m_minZoom, m_maxZoom);
            m_elapsedTime = 0f;

            while (m_elapsedTime < m_duration)
            {
                float t = Mathf.Clamp01(m_elapsedTime / m_duration);
                float eased = t.Ease(m_easeType);
                m_virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startZoom, m_targetZoom, eased);
                m_elapsedTime += Time.deltaTime;
                yield return null;
            }

            m_virtualCamera.Lens.OrthographicSize = m_targetZoom;
        }
    }
}