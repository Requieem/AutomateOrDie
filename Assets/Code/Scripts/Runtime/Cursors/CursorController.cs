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

        private void OnEnable()
        {
            m_positionAction.action.Enable();
            m_deltaAction.action.Enable();

            m_positionAction.action.performed += OnPosition;
            m_deltaAction.action.performed += OnDelta;

            if (m_hideCursor)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.Confined;
                UnityEngine.Cursor.visible = false;
            }
        }

        private void OnDisable()
        {
            m_positionAction.action.performed -= OnPosition;
            m_deltaAction.action.performed -= OnDelta;

            m_positionAction.action.Disable();
            m_deltaAction.action.Disable();

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        private void OnPosition(InputAction.CallbackContext context)
        {
            // Mouse movement sets the absolute position
            var screenPosition = context.ReadValue<Vector2>();
            if (Camera.main != null) m_cursorWorldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        }

        private void OnDelta(InputAction.CallbackContext context)
        {
            // Controller right stick moves the cursor relatively
            var delta = context.ReadValue<Vector2>();
            var worldDelta = delta * m_movementSpeed * Time.deltaTime;
            m_cursorWorldPosition += worldDelta;
        }

        private void Update()
        {
            // Update the cursor's actual sprite position
            if (m_cursorSpriteRenderer)
                m_cursorSpriteRenderer.transform.position = m_cursorWorldPosition;

            // Try to snap marker to grid
            if (m_markerSpriteRenderer && m_gridManager)
            {
                if (m_gridManager.TrySnapPosition(m_cursorWorldPosition, out var snappedPosition))
                {
                    if (Vector3.Distance(m_characterTransform.position, snappedPosition.ToVector3()) / m_gridManager.CellSize.magnitude <
                        m_maxCellDistance)
                    {
                        m_markerSpriteRenderer.enabled = true;
                        m_markerSpriteRenderer.transform.position = snappedPosition;
                        m_gridManager.WorldToGrid(snappedPosition.ToVector3(), out var gridPos);
                        m_gridManager.SetSelectedCell(gridPos);
                    }
                }
                else
                {
                    m_gridManager.SetSelectedCell(null);
                    m_markerSpriteRenderer.enabled = false;
                }
            }
        }

        public void SetValid(bool valid)
        {
            m_markerSpriteRenderer.color = valid ? m_validColor : m_invalidColor;
        }
    }
}