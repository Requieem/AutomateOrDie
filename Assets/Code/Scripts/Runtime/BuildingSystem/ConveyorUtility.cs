using System.Collections.Generic;
using UnityEngine;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public static class ConveyorUtility
    {
        public static IEnumerable<(ConveyDirection, Vector2Int)> DirectionVectors()
        {
            yield return (ConveyDirection.North, Vector2Int.up);
            yield return (ConveyDirection.East, Vector2Int.right);
            yield return (ConveyDirection.South, Vector2Int.down);
            yield return (ConveyDirection.West, Vector2Int.left);
        }

        public static ConveyDirection Opposite(ConveyDirection dir)
        {
            return dir switch
            {
                ConveyDirection.North => ConveyDirection.South,
                ConveyDirection.South => ConveyDirection.North,
                ConveyDirection.East => ConveyDirection.West,
                ConveyDirection.West => ConveyDirection.East,
                _ => ConveyDirection.None
            };
        }
    }
}