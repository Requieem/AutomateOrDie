using Code.Scripts.Common;
using UnityEngine;

namespace Code.Scripts.Runtime
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private Vector2 m_center;
        [SerializeField] private Vector2 m_cellSize;
        [SerializeField] private Vector2 m_bounds;
        [SerializeField] private SerializableDictionary<Vector2, GameObject> m_cells;
        [SerializeField] private Vector2 m_selectedCell;


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(m_center, m_bounds);

            for(int x = Mathf.RoundToInt(-m_bounds.x/2); x < Mathf.CeilToInt(m_bounds.x/2); x++)
            {
                for(int y = Mathf.RoundToInt(-m_bounds.y/2); y < Mathf.CeilToInt(m_bounds.y/2); y++)
                {
                    Vector2 cellPosition = new Vector2(x, y) * m_cellSize + m_center;
                    Gizmos.DrawWireCube(cellPosition, m_cellSize);
                }
            }
        }
    }
}