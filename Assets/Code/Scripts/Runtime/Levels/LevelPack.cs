using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Scripts.Runtime.Levels
{
    [CreateAssetMenu(fileName = "LevelPack", menuName = "Levels/Pack", order = 0)]
    public class LevelPack : ScriptableObject
    {
        [SerializeField] private Tile m_primaryFloor;
        [SerializeField] private Tile[] m_secondaryFloors;
        [SerializeField] private Tile m_primaryWall;
        [SerializeField] private Tile[] m_secondaryWalls;
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