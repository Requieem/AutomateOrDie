using Code.Scripts.Common;
using Code.Scripts.Runtime.Grid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.Runtime.Cursors
{
    public class CursorController : MonoBehaviour
    {
        [SerializeField] private bool m_hideCursor = true;
        [SerializeField] private GridManager m_gridManager;
        [SerializeField] private SpriteRenderer m_cursorSpriteRenderer;
        [SerializeField] private SpriteRenderer m_markerSpriteRenderer;
        [SerializeField] private InputActionReference m_positionAction; // e.g. mouse position
        [SerializeField] private InputActionReference m_deltaAction;    // e.g. right stick delta
        [SerializeField] private float m_movementSpeed = 500f; // pixels per second
        [SerializeField] private Color m_validColor = Color.green;
        [SerializeField] private Color m_invalidColor = Color.red;
        [SerializeField] private Transform m_characterTransform;
        [SerializeField] private int m_maxCellDistance = 2;

        public Transform MarkerTransform => m_markerSpriteRenderer.transform;
        private Vector2 m_cursorWorldPosition;
        private Camera m_camera;

        private bool m_useDelta;

        private void OnEnable()
        {
            m_positionAction.action.Enable();
            m_deltaAction.action.Enable();

            m_positionAction.action.performed += OnPosition;
            m_deltaAction.action.performed += OnDelta;

            m_camera = Camera.main;

            if (!m_hideCursor) return;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            m_positionAction.action.performed -= OnPosition;
            m_deltaAction.action.performed -= OnDelta;

            m_positionAction.action.Disable();
            m_deltaAction.action.Disable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnPosition(InputAction.CallbackContext context)
        {
            // Mouse movement sets the absolute position
            var screenPosition = context.ReadValue<Vector2>();
            m_useDelta = false;
            if (m_camera) m_cursorWorldPosition = m_camera.ScreenToWorldPoint(screenPosition);
        }

        private void OnDelta(InputAction.CallbackContext context)
        {
            m_useDelta = true;
        }

        private void Update()
        {
            if(m_useDelta)
                m_cursorWorldPosition += m_deltaAction.action.ReadValue<Vector2>() * (m_movementSpeed * Time.deltaTime);

            // Update the cursor's actual sprite position
            if (m_cursorSpriteRenderer)
                m_cursorSpriteRenderer.transform.position = m_cursorWorldPosition;

            // Try to snap marker to grid
            if (!m_markerSpriteRenderer || !m_gridManager) return;
            if (m_gridManager.TrySnapPosition(m_cursorWorldPosition, out var snappedPosition))
            {
                if (!(Vector3.Distance(m_characterTransform.position, snappedPosition.ToVector3()) /
                      m_gridManager.CellSize.magnitude <
                      m_maxCellDistance)) return;
                m_markerSpriteRenderer.enabled = true;
                m_markerSpriteRenderer.transform.position = snappedPosition;
                m_gridManager.WorldToGrid(snappedPosition.ToVector3(), out var gridPos);
                m_gridManager.SetSelectedCell(gridPos);
            }
            else
            {
                m_gridManager.SetSelectedCell(null);
                m_markerSpriteRenderer.enabled = false;
            }
        }

        public void SetValid(bool valid)
        {
            m_markerSpriteRenderer.color = valid ? m_validColor : m_invalidColor;
        }
    }
}