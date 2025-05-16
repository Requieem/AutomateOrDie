using AYellowpaper.SerializedCollections;
using Code.Scripts.Common;
using Code.Scripts.Runtime.BuildingSystem;
using Code.Scripts.Runtime.Characters;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Scripts.Runtime.Grid
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private Vector2 m_center;
        [SerializeField] private Vector2 m_cellSize;
        [SerializeField] private Vector2 m_bounds;
        [SerializeField] private SerializedDictionary<Vector2Int, Building> m_buildings;
        [SerializeField] private SerializedDictionary<Vector2Int, Belt> m_belts;
        [SerializeField] private SerializedDictionary<Vector2Int, Tile> m_resources;
        [SerializeField] private SerializedDictionary<Vector2Int, Tile> m_floor;
        [SerializeField] private SerializedDictionary<Vector2Int, Tile> m_walls;
        [SerializeField] private SerializedDictionary<Vector2Int, Character> m_characters;
        [SerializeField] private Tilemap m_buildingsTilemap;
        [SerializeField] private Tilemap m_resourcesTilemap;
        [SerializeField] private Tilemap m_floorTilemap;
        [SerializeField] private Tilemap m_wallsTilemap;
        [SerializeField] private Tilemap m_charactersTilemap;

        public static GridManager Instance { get; private set; }
        public Tilemap BuildingsTilemap => m_buildingsTilemap;
        public Tilemap ResourcesTilemap => m_resourcesTilemap;
        public Tilemap FloorTilemap => m_floorTilemap;
        public Tilemap WallsTilemap => m_wallsTilemap;
        public Tilemap CharactersTilemap => m_charactersTilemap;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if(Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
            }
        }

        public Vector2 Center => m_center;
        public Vector2 CellSize => m_cellSize;
        public Vector2 Bounds => m_bounds;

        public void SetSelectedCell(Vector2Int? position)
        {
            SelectedCell = position;
        }
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
            if(TryGetBuilding(position, out var building) && building && building.gameObject)
            {
                obj = building.gameObject;
                return true;
            }
            if (TryGetCharacter(position, out var character) && character && character.gameObject)
            {
                obj = character.gameObject;
                return true;
            }
            if (TryGetFloor(position, out var tile) && tile && tile.gameObject)
            {
                obj = tile.gameObject;
                return true;
            }
            if (TryGetWall(position, out tile) && tile && tile.gameObject)
            {
                obj = tile.gameObject;
                return true;
            }
            if (TryGetResource(position, out tile) && tile && tile.gameObject)
            {
                obj = tile.gameObject;
                return true;
            }
            if (TryGetBelt(position, out var belt) && belt && belt.gameObject)
            {
                obj = belt.gameObject;
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

        public void WorldToGrid(Vector3 worldPosition, out Vector2Int gridPosition)
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
            if (worldPosition.x < gridOrigin.x || worldPosition.x > gridMax.x ||
                worldPosition.y > gridOrigin.y || worldPosition.y < gridMin.y)
            {
                gridPosition = Vector2Int.zero;
                return;
            }

            // Local offset from top-left
            var offset = worldPosition.ToVector2() - gridOrigin;
            var x = Mathf.RoundToInt(offset.x / cellSize.x);
            var y = Mathf.RoundToInt(-offset.y / cellSize.y); // Flip Y since the grid is top-down
            // Reconstruct the snapped position (center of cell)
            gridPosition = new Vector2Int(x, -y);
        }

        public bool TryAddBuilding(Vector2Int position, Building building)
        {
            return !TryGetCell(position, out _) && m_buildings.TryAdd(position, building);
        }

        public bool TryAddBelt(Vector2Int position, Belt belt)
        {
            return m_belts.TryAdd(position, belt);
        }

        public bool TryAddCharacter(Vector2Int position, Character character)
        {
            return !TryGetCell(position, out _) && m_characters.TryAdd(position, character);
        }

        public bool TryAddFloor(Vector2Int position, Tile tile)
        {
            return !TryGetCell(position, out _) && m_floor.TryAdd(position, tile);
        }

        public bool TryAddWall(Vector2Int position, Tile tile)
        {
            return !TryGetCell(position, out _) && m_walls.TryAdd(position, tile);
        }

        public bool TryAddResource(Vector2Int position, Tile tile)
        {
            return !TryGetCell(position, out _) && m_resources.TryAdd(position, tile);
        }

        public bool TryRemoveBuilding(Vector2Int position)
        {
            return m_buildings.Remove(position);
        }

        public bool TryRemoveBelt(Vector2Int position)
        {
            return m_belts.Remove(position);
        }

        public bool TryRemoveCharacter(Vector2Int position)
        {
            return m_characters.Remove(position);
        }

        public bool TryRemoveFloor(Vector2Int position)
        {
            return m_floor.Remove(position);
        }

        public bool TryRemoveWall(Vector2Int position)
        {
            return m_walls.Remove(position);
        }

        public bool TryRemoveResource(Vector2Int position)
        {
            return m_resources.Remove(position);
        }

        public bool TryGetBuilding(Vector2Int position, out Building building)
        {
            return m_buildings.TryGetValue(position, out building);
        }

        public bool TryGetBelt(Vector2Int position, out Belt belt)
        {
            return m_belts.TryGetValue(position, out belt);
        }

        public bool TryGetCharacter(Vector2Int position, out Character character)
        {
            return m_characters.TryGetValue(position, out character);
        }

        public bool TryGetFloor(Vector2Int position, out Tile tile)
        {
            return m_floor.TryGetValue(position, out tile);
        }

        public bool TryGetWall(Vector2Int position, out Tile tile)
        {
            return m_walls.TryGetValue(position, out tile);
        }

        public bool TryGetResource(Vector2Int position, out Tile tile)
        {
            return m_resources.TryGetValue(position, out tile);
        }

        public bool CanPlaceBuilding(Vector2Int position, bool needsResource = false)
        {
            var hasResource = m_resources.ContainsKey(position);
            var hasBuilding = m_buildings.ContainsKey(position);
            var hasWall = m_walls.ContainsKey(position);
            var hasFloor = m_floor.ContainsKey(position);

            return needsResource switch
            {
                true when !hasResource => false,
                true => true,
                _ => hasFloor && !(hasBuilding || hasWall || hasResource)
            };
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
            foreach (var (pos, obj) in m_buildings)
            {
                var cellPosition = pos * m_cellSize + m_center;
                Gizmos.DrawWireCube(cellPosition + transform.position.ToVector2(), m_cellSize);

                if (obj != null)
                    DrawString(obj.name, cellPosition + transform.position.ToVector2(), Color.yellow, Vector2.zero);
            }

            Gizmos.color = Color.red;
            foreach (var (pos, _) in m_floor)
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