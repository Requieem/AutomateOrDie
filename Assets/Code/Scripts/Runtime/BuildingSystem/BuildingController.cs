using System;
using System.Collections.Generic;
using Code.Scripts.Runtime.Cursors;
using Code.Scripts.Runtime.Grid;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public class BuildingController : MonoBehaviour
    {
        [SerializeField] private bool m_isBuilding;
        [SerializeField] private int m_buildIndex;
        [SerializeField] private List<Building> m_buildings;
        [SerializeField] private CursorController m_cursorController;
        [SerializeField] private InputActionReference m_changeIndexAction;

        [FormerlySerializedAs("m_buildAction")] [SerializeField]
        private InputActionReference m_startBuildAction;

        [SerializeField] private InputActionReference m_cancelAction;


        private Object m_builder;
        private BuildingPlaceholder m_placeholder;
        private Building m_buildingPrefab;
        private HashSet<Building> m_buildingInstances = new HashSet<Building>();

        public static BuildingController Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
            }
        }

        private void Start()
        {
            m_changeIndexAction.action.performed += OnChangeIndex;
            m_startBuildAction.action.performed += OnBuild;
            m_cancelAction.action.performed += OnCancel;
        }

        private void OnDestroy()
        {
            m_changeIndexAction.action.performed -= OnChangeIndex;
            m_startBuildAction.action.performed -= OnBuild;
            m_cancelAction.action.performed -= OnCancel;
        }

        private void OnChangeIndex(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            var value = context.ReadValue<float>();

            switch (value)
            {
                case > 0:
                    NextIndex();
                    break;
                case < 0:
                    PreviousIndex();
                    break;
            }
        }

        private void OnBuild(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (m_isBuilding && m_builder == this)
            {
                OnConfirm(context);
                return;
            }

            TryBuild(this, out var building);
        }

        private void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (!m_isBuilding) return;
            if (m_builder != this) return;
            if (m_placeholder)
            {
                if (!m_placeholder.IsValid) return;
                Destroy(m_placeholder.gameObject);
                m_placeholder = null;
            }

            var buildingInstance = Instantiate(m_buildings[m_buildIndex], m_cursorController.MarkerTransform.position,
                Quaternion.identity);

            m_buildingInstances.Add(buildingInstance);
            CancelBuild(this);
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            switch (m_isBuilding)
            {
                case true when m_builder != this:
                    return;
                case false:
                    TryDestroy();
                    break;
                default:
                    CancelBuild(this);
                    break;
            }
        }

        public void TryDestroy()
        {
            var gridManager = GridManager.Instance;
            if (gridManager.SelectedCell == null) return;
            var selectedPosition = gridManager.SelectedCell.Value;
            gridManager.TryGetBuilding(selectedPosition, out var building);
            gridManager.TryGetBelt(selectedPosition, out var belt);

            if(building)
                Destroy(building.gameObject);
            if(belt)
                Destroy(belt.gameObject);

            gridManager.TryRemoveBuilding(selectedPosition);
            gridManager.TryRemoveBelt(selectedPosition);
        }

        public bool TryBuild(Object builder, out Building building)
        {
            if (m_isBuilding)
            {
                building = null;
                return false;
            }

            if (m_buildIndex < 0 || m_buildIndex >= m_buildings.Count)
                throw new System.Exception("Building index out of range. This should never happen");

            m_isBuilding = true;
            m_builder = builder;
            building = m_buildings[m_buildIndex];
            m_placeholder = Instantiate(building.PlaceholderPrefab, Vector3.zero, Quaternion.identity,
                m_cursorController.MarkerTransform);
            m_placeholder.transform.localPosition = Vector3.zero;
            m_placeholder.SetValid(true);
            m_buildingPrefab = building;
            return true;
        }

        public bool CancelBuild(Object builder)
        {
            if (!m_isBuilding)
                return false;
            if (builder != m_builder)
                throw new System.Exception(
                    "A builder is trying to cancel a build started by another. this should never happen");

            m_isBuilding = false;
            m_builder = null;
            if (m_placeholder)
            {
                Destroy(m_placeholder.gameObject);
                m_placeholder = null;
            }

            m_buildingPrefab = null;
            return true;
        }

        public bool NextIndex()
        {
            return SetBuildIndex(WrappedIndex(m_buildIndex + 1, m_buildings.Count));
        }

        public bool PreviousIndex()
        {
            return SetBuildIndex(WrappedIndex(m_buildIndex - 1, m_buildings.Count));
        }

        private int WrappedIndex(int index, int count)
        {
            return count == 0 ? 0 : (index % count + count) % count;
        }

        public bool SetBuildIndex(int buildIndex)
        {
            if (buildIndex < 0 || buildIndex >= m_buildings.Count)
                return false;

            m_buildIndex = buildIndex;

            if (m_placeholder)
            {
                Destroy(m_placeholder.gameObject);
            }

            if (!m_isBuilding) return true;

            m_buildingPrefab = m_buildings[m_buildIndex];
            m_placeholder = Instantiate(m_buildingPrefab.PlaceholderPrefab, Vector3.zero, Quaternion.identity,
                m_cursorController.MarkerTransform);
            m_placeholder.transform.localPosition = Vector3.zero;
            m_placeholder.SetValid(true);
            return true;
        }

        private void Update()
        {
            if (!m_placeholder) return;
            m_placeholder.SetValid(true);
            if(m_cursorController)
                m_cursorController.SetValid(m_placeholder.IsValid);
        }
    }
}