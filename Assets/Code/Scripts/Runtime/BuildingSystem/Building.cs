using Code.Scripts.Runtime.Grid;
using Code.Scripts.Runtime.Structures;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public class Building : MonoBehaviour
    {
        [SerializeField] protected Tilemap m_floorTilemap;
        [SerializeField] protected Tilemap m_wallTilemap;
        [SerializeField] protected Tilemap m_resourcesTilemap;
        [SerializeField] protected Tilemap m_buildingTilemap;
        [SerializeField] protected BuildingPlaceholder m_placeholderPrefab;
        [SerializeField] protected StructureKey m_key;

        public BuildingPlaceholder PlaceholderPrefab => m_placeholderPrefab;
        public Tilemap BuildingTilemap => m_buildingTilemap;
        public StructureKey Key => m_key;

        private void Start()
        {
            Setup();
        }

        protected virtual void Setup()
        {
            ApplyBuilding();
        }

        private void ApplyTilemap(Tilemap source)
        {
            source.CompressBounds();
            var bounds = source.cellBounds;
            var gridManager = GridManager.Instance;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    var tile = source.GetTile(cell);
                    if (!tile) continue;

                    // Local to world conversion (safe for nested tilemaps)
                    var worldPos = source.GetCellCenterWorld(cell);

                    Debug.Log($"Tile: {tile.name} at {cell} -> {worldPos}");

                    gridManager.WorldToGrid(worldPos, out var gridPos);
                    gridManager.TryAddBuilding(gridPos, this);
                }
            }
        }

        /// <summary>
        /// Applies all tilemaps using <see cref= 'ApplyTilemap(Tilemap)' />
        /// </summary>
        public void ApplyBuilding()
        {
            ApplyTilemap(m_floorTilemap);
            ApplyTilemap(m_wallTilemap);
            ApplyTilemap(m_resourcesTilemap);
            ApplyTilemap(m_buildingTilemap);
        }
    }
}