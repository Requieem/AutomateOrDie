using System;
using Code.Scripts.Common;

namespace Code.Scripts.Runtime
{

using Levels;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GridManager))]
public class FactoryFloorGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private int m_width = 64;
    [SerializeField] private int m_height = 64;
    [SerializeField] private int m_seed = 12345;
    [SerializeField] [Range(0f, 1f)] private float m_initialFillPercent = 0.45f;
    [SerializeField] private int m_smoothIterations = 5;
    [SerializeField] private LevelPack m_levelPack;

    [Header("Tilemap")]
    [SerializeField] private Tilemap m_environmentTilemap;
    [SerializeField] private Tilemap m_wallTilemap;

    private GridManager m_gridManager;
    private int[,] m_map;

    private void Awake()
    {
        m_gridManager = GetComponent<GridManager>();
        m_gridManager.SetBounds(new Vector2(m_width, m_height));
        m_environmentTilemap.size = new Vector3Int(m_width, m_height, 1);
        m_environmentTilemap.origin = new Vector3Int(0, -m_height, 0);
        m_environmentTilemap.tileAnchor = Vector3.zero;// Match grid top-left origin
        m_wallTilemap.size = new Vector3Int(m_width, m_height, 1);
        m_wallTilemap.origin = new Vector3Int(0, -m_height, 0);
        m_wallTilemap.tileAnchor = Vector3.zero;// Match grid top-left origin
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
        if (m_environmentTilemap == null) return;

        m_environmentTilemap.ClearAllTiles();

        for (var x = 0; x < m_width; x++)
        for (var y = 0; y < m_height; y++)
        {
            if (m_map[x, y] != 1) continue;
            var cellPos = new Vector3Int(x, -y, 0); // Match grid top-left origin
            var isWall = IsWall(x, y);
            var randomTile = !isWall ? m_levelPack.RandomFloorAt(cellPos.ToVector2Int()) : m_levelPack.RandomWallAt(cellPos.ToVector2Int());
            if(!m_gridManager.TryAddEnvironment(cellPos.ToVector2Int(), randomTile))
                continue;
            if(randomTile == null) continue;
            if(!isWall)
                m_environmentTilemap.SetTile(cellPos, randomTile);
            else
                m_wallTilemap.SetTile(cellPos, randomTile);
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
}

}