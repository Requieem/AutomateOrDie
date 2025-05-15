using AYellowpaper.SerializedCollections;
using Code.Scripts.Common;
using Code.Scripts.Runtime.Characters;
using Code.Scripts.Runtime.Structures;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Scripts.Runtime
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private Vector2 m_center;
        [SerializeField] private Vector2 m_cellSize;
        [SerializeField] private Vector2 m_bounds;
        [SerializeField] private SerializedDictionary<Vector2Int, Structure> m_structures;
        [SerializeField] private SerializedDictionary<Vector2Int, Tile> m_resources;
        [SerializeField] private SerializedDictionary<Vector2Int, Tile> m_environment;
        [SerializeField] private SerializedDictionary<Vector2Int, Character> m_characters;

        public Vector2 Center => m_center;
        public Vector2 CellSize => m_cellSize;
        public Vector2 Bounds => m_bounds;

        public void SetCenter(Vector2 center)
        {
            m_center = center;
        }
        public void SetCellSize(Vector2 cellSize)
        {
            m_cellSize = cellSize;
        }
        public void SetBounds(Vector2 bounds)
        {
            m_bounds = bounds;
        }

        public bool TryGetCell(Vector2Int position, out GameObject obj)
        {
            if(TryGetStructure(position, out var structure) && structure && structure.gameObject)
            {
                obj = structure.gameObject;
                return true;
            }
            if (TryGetCharacter(position, out var character) && character && character.gameObject)
            {
                obj = character.gameObject;
                return true;
            }
            if (TryGetEnvironment(position, out var tile) && tile && tile.gameObject)
            {
                obj = tile.gameObject;
                return true;
            }
            obj = null;
            return false;
        }

        public bool TrySnapPosition(Vector2 position, out Vector2 snappedPosition)
        {
            var cellSize = m_cellSize;
            var bounds = m_bounds;

            // Top-left of grid in world space
            var gridOrigin = transform.position.ToVector2() + m_center;

            // Compute total grid size
            var gridSize = new Vector2(bounds.x * cellSize.x, bounds.y * cellSize.y);

            // Bottom-right corner
            var gridMin = gridOrigin + new Vector2(0, -gridSize.y);
            var gridMax = gridOrigin + new Vector2(gridSize.x, 0);

            // Bounds check
            if (position.x < gridOrigin.x || position.x > gridMax.x ||
                position.y > gridOrigin.y || position.y < gridMin.y)
            {
                snappedPosition = Vector2.zero;
                return false;
            }

            // Local offset from top-left
            var offset = position - gridOrigin;

            var x = Mathf.RoundToInt(offset.x / cellSize.x);
            var y = Mathf.RoundToInt(-offset.y / cellSize.y); // Flip Y since the grid is top-down

            // Reconstruct the snapped position (center of cell)
            snappedPosition = gridOrigin + new Vector2(x, -y) * cellSize;
            return true;
        }

        public bool TryAddStructure(Vector2Int position, Structure structure)
        {
            return !TryGetCell(position, out _) && m_structures.TryAdd(position, structure);
        }

        public bool TryAddCharacter(Vector2Int position, Character character)
        {
            return !TryGetCell(position, out _) && m_characters.TryAdd(position, character);
        }

        public bool TryAddEnvironment(Vector2Int position, Tile tile)
        {
            return !TryGetCell(position, out _) && m_environment.TryAdd(position, tile);
        }

        public bool TryAddResource(Vector2Int position, Tile tile)
        {
            return !TryGetCell(position, out _) && m_resources.TryAdd(position, tile);
        }

        public bool TryRemoveStructure(Vector2Int position)
        {
            return m_structures.Remove(position);
        }

        public bool TryRemoveCharacter(Vector2Int position)
        {
            return m_characters.Remove(position);
        }

        public bool TryRemoveEnvironment(Vector2Int position)
        {
            return m_environment.Remove(position);
        }

        public bool TryRemoveResource(Vector2Int position)
        {
            return m_resources.Remove(position);
        }

        public bool TryGetStructure(Vector2Int position, out Structure structure)
        {
            return m_structures.TryGetValue(position, out structure);
        }

        public bool TryGetCharacter(Vector2Int position, out Character character)
        {
            return m_characters.TryGetValue(position, out character);
        }

        public bool TryGetEnvironment(Vector2Int position, out Tile tile)
        {
            return m_environment.TryGetValue(position, out tile);
        }

        public bool TryGetResource(Vector2Int position, out Tile tile)
        {
            return m_resources.TryGetValue(position, out tile);
        }

        public Vector2Int? SelectedCell { get; set; }

        #region Visual Debugging

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            for (var x = 0; x < m_bounds.x; x++)
            {
                for (var y = 0; y > -m_bounds.y; y--)
                {
                    var pos = new Vector2Int(x, y);
                    if (TryGetCell(pos, out _)) continue;
                    var cellPosition = pos * m_cellSize + m_center;
                    Gizmos.DrawWireCube(cellPosition + transform.position.ToVector2(), m_cellSize);
                }
            }

            Gizmos.color = Color.magenta;
            foreach (var (pos, obj) in m_structures)
            {
                var cellPosition = pos * m_cellSize + m_center;
                Gizmos.DrawWireCube(cellPosition + transform.position.ToVector2(), m_cellSize);

                if (obj != null)
                    DrawString(obj.name, cellPosition + transform.position.ToVector2(), Color.yellow, Vector2.zero);
            }

            Gizmos.color = Color.red;
            foreach (var (pos, obj) in m_environment)
            {
                var cellPosition = pos * m_cellSize + m_center;
                Gizmos.DrawWireCube(cellPosition + transform.position.ToVector2(), m_cellSize);

                // if (obj != null)
                //     DrawString(obj.name, cellPosition + transform.position.ToVector2(), Color.yellow, Vector2.zero);
            }

            Gizmos.color = Color.yellow;
            foreach (var (pos, obj) in m_resources)
            {
                var cellPosition = pos * m_cellSize + m_center;
                Gizmos.DrawWireCube(cellPosition + transform.position.ToVector2(), m_cellSize);

                if (obj != null)
                    DrawString(obj.name, cellPosition + transform.position.ToVector2(), Color.yellow, Vector2.zero);
            }

            Gizmos.color = Color.cyan;
            foreach (var (pos, obj) in m_characters)
            {
                var cellPosition = pos * m_cellSize + m_center;
                Gizmos.DrawWireCube(cellPosition + transform.position.ToVector2(), m_cellSize);

                if (obj != null)
                    DrawString(obj.name, cellPosition + transform.position.ToVector2(), Color.yellow, Vector2.zero);
            }

            if (SelectedCell.HasValue)
            {
                Gizmos.color = Color.green;
                var cellPosition = SelectedCell.Value * m_cellSize + m_center;
                Gizmos.DrawWireCube(cellPosition + transform.position.ToVector2(), m_cellSize);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                transform.position.ToVector2() + m_center + new Vector2(m_bounds.x, -m_bounds.y) * m_cellSize / 2f -
                new Vector2(m_cellSize.x, -m_cellSize.y) / 2f, m_bounds * m_cellSize);
        }

        private static void DrawString(string text, Vector3 worldPosition, Color textColor, Vector2 anchor,
            float textSize = 15f)
        {
#if UNITY_EDITOR
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            if (!view)
                return;
            var screenPosition = view.camera.WorldToScreenPoint(worldPosition);
            if (screenPosition.y < 0 || screenPosition.y > view.camera.pixelHeight || screenPosition.x < 0 ||
                screenPosition.x > view.camera.pixelWidth || screenPosition.z < 0)
                return;
            var pixelRatio = UnityEditor.HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.right).x -
                             UnityEditor.HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.zero).x;
            UnityEditor.Handles.BeginGUI();
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = (int)textSize,
                normal = new GUIStyleState() { textColor = textColor }
            };
            var size = style.CalcSize(new GUIContent(text)) * pixelRatio;
            var alignedPosition =
                ((Vector2)screenPosition +
                 size * ((anchor + Vector2.left + Vector2.up) / 2f)) * (Vector2.right + Vector2.down) +
                Vector2.up * view.camera.pixelHeight;
            GUI.Label(new Rect(alignedPosition / pixelRatio, size / pixelRatio), text, style);
            UnityEditor.Handles.EndGUI();
#endif
        }

        #endregion
    }
}