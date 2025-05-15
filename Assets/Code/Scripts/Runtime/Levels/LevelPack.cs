using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Scripts.Runtime.Levels
{
    [CreateAssetMenu(fileName = "LevelPack", menuName = "Levels/Pack", order = 0)]
    public class LevelPack : ScriptableObject
    {
        [Serializable]
        public struct FloatRange
        {
            [SerializeField] private float m_min;
            [SerializeField] private float m_max;

            public float Min => m_min;
            public float Max => m_max;

            public FloatRange(float min, float max)
            {
                m_min = min;
                m_max = max;
            }
        }

        [SerializeField] private Tile m_primaryFloor;
        [SerializeField] private Tile[] m_secondaryFloors;
        [SerializeField] private Tile m_primaryWall;
        [SerializeField] private Tile[] m_secondaryWalls;
        [SerializeField] private SerializedDictionary<Tile, FloatRange> m_resourceTiles;
        [SerializeField] private float m_secondaryFloorDensity = 0.5f;
        [SerializeField] private float m_secondaryWallDensity = 0.5f;

        public Tile RandomFloorAt(Vector2Int coord)
        {
            var tileSeed = coord.x * 73856093 ^ coord.y * 19349663; // Large primes
            var rng = new System.Random(tileSeed);

            var roll = rng.NextDouble();
            return roll < m_secondaryFloorDensity && m_secondaryFloors.Length > 0
                ? m_secondaryFloors[rng.Next(m_secondaryFloors.Length)]
                : m_primaryFloor;
        }

        public Tile RandomResourceAt(Vector2Int coord)
        {
            var tileSeed = coord.x * 73856093 ^ coord.y * 19349663; // Large primes
            var rng = new System.Random(tileSeed);

            var roll = rng.NextDouble();
            foreach (var (tile, range) in m_resourceTiles)
            {
                if(roll < range.Min)
                    continue;
                if(roll >= range.Max)
                    continue;
                return tile;
            }

            return null;
        }


        public Tile RandomWallAt(Vector2Int coord)
        {
            var tileSeed = coord.x * 73856093 ^ coord.y * 19349663; // Large primes
            var rng = new System.Random(tileSeed);
            var roll = rng.NextDouble();
            return roll < m_secondaryWallDensity && m_secondaryWalls.Length > 0
                ? m_secondaryWalls[rng.Next(m_secondaryWalls.Length)]
                : m_primaryWall;
        }
    }
}