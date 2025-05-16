using System.Collections.Generic;
using Code.Scripts.Common;
using Code.Scripts.Runtime.Levels;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Code.Scripts.Runtime.Grid
{
    [RequireComponent(typeof(GridManager))]
public class FactoryFloorGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private int m_width = 64;
    [SerializeField] private int m_height = 64;
    [SerializeField] private int m_seed = 12345;
    [SerializeField] [Range(0f, 1f)] private float m_initialFillPercent = 0.45f;
    [SerializeField] private int m_smoothIterations = 5;
    [SerializeField] private float m_resourceDensity = 0.25f;
    [SerializeField] private LevelPack m_levelPack;

    [Header("Tilemap")]
    [FormerlySerializedAs("m_environmentTilemap")]
    [SerializeField] private Tilemap m_floorTilemap;
    [SerializeField] private Tilemap m_wallTilemap;
    [SerializeField] private Tilemap m_resourcesTilemap;

    private GridManager m_gridManager;
    private int[,] m_map;

    private void Awake()
    {
        m_gridManager = GetComponent<GridManager>();
        m_gridManager.SetBounds(new Vector2(m_width, m_height));
        m_floorTilemap.size = new Vector3Int(m_width, m_height, 1);
        m_floorTilemap.origin = new Vector3Int(0, -m_height, 0);
        m_floorTilemap.tileAnchor = Vector3.zero;// Match grid top-left origin
        m_wallTilemap.size = new Vector3Int(m_width, m_height, 1);
        m_wallTilemap.origin = new Vector3Int(0, -m_height, 0);
        m_wallTilemap.tileAnchor = Vector3.zero;// Match grid top-left origin
        m_resourcesTilemap.size = new Vector3Int(m_width, m_height, 1);
        m_resourcesTilemap.origin = new Vector3Int(0, -m_height, 0);
        m_resourcesTilemap.tileAnchor = Vector3.zero;// Match grid top-left origin
        Generate();
    }

    [ContextMenu("Regenerate")]
    public void Generate()
    {
        Random.InitState(m_seed);
        GenerateMap();
        SmoothMap();
        ApplyToTilemap();
    }

    private void GenerateMap()
    {
        m_map = new int[m_width, m_height];

        for (var x = 0; x < m_width; x++)
        for (var y = 0; y < m_height; y++)
        {
            if (x == 0 || y == 0 || x == m_width - 1 || y == m_height - 1)
                m_map[x, y] = 1; // Wall borders
            else
                m_map[x, y] = Random.value < m_initialFillPercent ? 1 : 0;
        }
    }

    private void SmoothMap()
    {
        for (var i = 0; i < m_smoothIterations; i++)
        {
            var newMap = new int[m_width, m_height];

            for (var x = 1; x < m_width - 1; x++)
            for (var y = 1; y < m_height - 1; y++)
            {
                var neighborWalls = GetSurroundingWallCount(x, y);

                newMap[x, y] = neighborWalls > 4 ? 1 : 0;
            }

            m_map = newMap;
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        var wallCount = 0;

        for (var x = gridX - 1; x <= gridX + 1; x++)
        for (var y = gridY - 1; y <= gridY + 1; y++)
        {
            if (x == gridX && y == gridY) continue;
            if (x < 0 || y < 0 || x >= m_width || y >= m_height || m_map[x, y] == 1) wallCount++;
        }

        return wallCount;
    }

    private void ApplyToTilemap()
    {
        if (m_floorTilemap == null) return;

        m_floorTilemap.ClearAllTiles();
        m_wallTilemap.ClearAllTiles();
        m_resourcesTilemap.ClearAllTiles();

        for (var x = 0; x < m_width; x++)
        for (var y = 0; y < m_height; y++)
        {
            if (m_map[x, y] != 1) continue;
            var cellPos = new Vector3Int(x, -y, 0); // Match grid top-left origin
            var isWall = IsWall(x, y);
            var randomTile = !isWall ? m_levelPack.RandomFloorAt(cellPos.ToVector2Int()) : m_levelPack.RandomWallAt(cellPos.ToVector2Int());
            if(randomTile == null) continue;
            if (!isWall)
            {
                if(!m_gridManager.TryAddFloor(cellPos.ToVector2Int(), randomTile))
                    continue;
                m_floorTilemap.SetTile(cellPos, randomTile);
            }
            else
            {
                if(!m_gridManager.TryAddWall(cellPos.ToVector2Int(), randomTile))
                    continue;
                m_wallTilemap.SetTile(cellPos, randomTile);
            }
        }

        var resourcePositions = GetResourcePositions(m_resourceDensity);
        foreach (var pos in resourcePositions)
        {
            var cellPos = new Vector3Int(pos.x, -pos.y, 0); // Match grid top-left origin
            var randomTile = m_levelPack.RandomResourceAt(pos);
            if (randomTile == null) continue;
            if (!m_gridManager.TryAddResource(cellPos.ToVector2Int(), randomTile))
                continue;
            m_gridManager.TryRemoveFloor(cellPos.ToVector2Int());
            m_gridManager.TryRemoveWall(cellPos.ToVector2Int());
            m_gridManager.TryRemoveBuilding(cellPos.ToVector2Int());
            m_gridManager.TryRemoveCharacter(cellPos.ToVector2Int());
            m_floorTilemap.SetTile(cellPos, null);
            m_wallTilemap.SetTile(cellPos, null);
            m_resourcesTilemap.SetTile(cellPos, randomTile);
        }
    }

    private bool IsWall(int x, int y)
    {
        var hasLeftWall = x > 0 && m_map[x - 1, y] == 1;
        var hasRightWall = x < m_width - 1 && m_map[x + 1, y] == 1;
        var hasTopWall = y < m_height - 1 && m_map[x, y + 1] == 1;
        var hasBottomWall = y > 0 && m_map[x, y - 1] == 1;
        return !(hasLeftWall && hasRightWall && hasTopWall && hasBottomWall);
    }

    private List<Vector2Int> GetResourcePositions(float maxDensity)
    {
        var allFloorPositions = new List<Vector2Int>();

        for (var x = 0; x < m_width; x++)
        {
            for (var y = 0; y < m_height; y++)
            {
                var pos = new Vector3Int(x, -y, 0); // grid convention
                if (m_floorTilemap.HasTile(pos) &&
                    !m_wallTilemap.HasTile(pos) &&
                    (m_resourcesTilemap == null || !m_resourcesTilemap.HasTile(pos)))
                {
                    allFloorPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        Shuffle(allFloorPositions);

        var chosen = new List<Vector2Int>();
        var taken = new HashSet<Vector2Int>();
        var maxCount = Mathf.FloorToInt(allFloorPositions.Count * maxDensity);

        foreach (var pos in allFloorPositions)
        {
            if (IsClearAround(pos, taken))
            {
                chosen.Add(pos);
                MarkTaken(pos, taken);
                if (chosen.Count >= maxCount)
                    break;
            }
        }

        return chosen;
    }

    private bool IsClearAround(Vector2Int center, HashSet<Vector2Int> taken)
    {
        const float radius = 2.5f;
        float radiusSqr = radius * radius;

        for (int dx = -3; dx <= 3; dx++) // radius 2.5 fits within 3 tiles
        {
            for (int dy = -3; dy <= 3; dy++)
            {
                if (dx * dx + dy * dy > radiusSqr)
                    continue;

                var check = new Vector2Int(center.x + dx, center.y + dy);
                if (taken.Contains(check) || IsWall(check.x, check.y))
                    return false;
            }
        }

        return true;
    }

    private void MarkTaken(Vector2Int center, HashSet<Vector2Int> taken)
    {
        const float radius = 2.5f;
        float radiusSqr = radius * radius;

        for (int dx = -3; dx <= 3; dx++)
        {
            for (int dy = -3; dy <= 3; dy++)
            {
                if (dx * dx + dy * dy > radiusSqr)
                    continue;

                taken.Add(new Vector2Int(center.x + dx, center.y + dy));
            }
        }
    }

    private void Shuffle<T>(IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

}

}