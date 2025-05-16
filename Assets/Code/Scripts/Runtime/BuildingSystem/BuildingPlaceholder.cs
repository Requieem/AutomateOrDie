using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public class BuildingPlaceholder : MonoBehaviour
    {
        [SerializeField] private Tilemap m_tilemap;
        [SerializeField] private Color m_validColor = Color.green;
        [SerializeField] private Color m_invalidColor = Color.red;
        [SerializeField] private BuildingRequirements m_requirements;

        private bool m_isValid;
        public bool IsValid => m_isValid;

        public void SetValid(bool isValid)
        {
            isValid = isValid && m_requirements.CheckRequirements();
            m_tilemap.color = isValid ? m_validColor : m_invalidColor;
            m_isValid = isValid;
        }
    }
}