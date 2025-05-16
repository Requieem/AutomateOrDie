// VectorExtensions.cs
using UnityEngine;

namespace Code.Scripts.Common
{
    public static class VectorExtensions
    {
        // ─────────────────────────────────────────────
        // Vector2
        // ─────────────────────────────────────────────
        public static Vector2 WithX(this Vector2 v, float x) => new Vector2(x, v.y);
        public static Vector2 WithY(this Vector2 v, float y) => new Vector2(v.x, y);
        public static Vector2Int ToVector2Int(this Vector2 v) => new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        public static Vector3 ToVector3(this Vector2 v) => new Vector3(v.x, v.y, 0f);
        public static Vector3 ToVector3(this Vector2 v, float z) => new Vector3(v.x, v.y, z);
        public static Vector4 ToVector4(this Vector2 v) => new Vector4(v.x, v.y, 0f, 0f);
        public static Vector4 ToVector4(this Vector2 v, float z, float w) => new Vector4(v.x, v.y, z, w);

        // ─────────────────────────────────────────────
        // Vector2Int
        // ─────────────────────────────────────────────
        public static Vector2Int WithX(this Vector2Int v, int x) => new Vector2Int(x, v.y);
        public static Vector2Int WithY(this Vector2Int v, int y) => new Vector2Int(v.x, y);
        public static Vector3Int ToVector3Int(this Vector2Int v) => new Vector3Int(v.x, v.y, 0);
        public static Vector3Int ToVector3Int(this Vector2Int v, int z) => new Vector3Int(v.x, v.y, z);
        public static Vector3 ToVector3(this Vector2Int v) => new Vector3(v.x, v.y, 0f);
        public static Vector3 ToVector3(this Vector2Int v, float z) => new Vector3(v.x, v.y, z);
        public static Vector2 ToVector2(this Vector2Int v) => new Vector2(v.x, v.y);

        // ─────────────────────────────────────────────
        // Vector3
        // ─────────────────────────────────────────────
        public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
        public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);
        public static Vector2 ToVector2(this Vector3 v) => new Vector2(v.x, v.y);
        public static Vector2 ToVector2XY(this Vector3 v) => new Vector2(v.x, v.y);
        public static Vector2 ToVector2XZ(this Vector3 v) => new Vector2(v.x, v.z);
        public static Vector2 ToVector2YZ(this Vector3 v) => new Vector2(v.y, v.z);
        public static Vector3Int ToVector3Int(this Vector3 v) => new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        public static Vector2Int ToVector2Int(this Vector3 v) => new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        public static Vector4 ToVector4(this Vector3 v) => new Vector4(v.x, v.y, v.z, 0f);
        public static Vector4 ToVector4(this Vector3 v, float w) => new Vector4(v.x, v.y, v.z, w);

        // ─────────────────────────────────────────────
        // Vector3Int
        // ─────────────────────────────────────────────
        public static Vector3Int WithX(this Vector3Int v, int x) => new Vector3Int(x, v.y, v.z);
        public static Vector3Int WithY(this Vector3Int v, int y) => new Vector3Int(v.x, y, v.z);
        public static Vector3Int WithZ(this Vector3Int v, int z) => new Vector3Int(v.x, v.y, z);
        public static Vector2Int ToVector2Int(this Vector3Int v) => new Vector2Int(v.x, v.y);
        public static Vector2Int ToVector2IntXZ(this Vector3Int v) => new Vector2Int(v.x, v.z);
        public static Vector2Int ToVector2IntYZ(this Vector3Int v) => new Vector2Int(v.y, v.z);
        public static Vector3 ToVector3(this Vector3Int v) => new Vector3(v.x, v.y, v.z);
        public static Vector4 ToVector4(this Vector3Int v) => new Vector4(v.x, v.y, v.z, 0f);
        public static Vector4 ToVector4(this Vector3Int v, float w) => new Vector4(v.x, v.y, v.z, w);

        // ─────────────────────────────────────────────
        // Vector4
        // ─────────────────────────────────────────────
        public static Vector4 WithX(this Vector4 v, float x) => new Vector4(x, v.y, v.z, v.w);
        public static Vector4 WithY(this Vector4 v, float y) => new Vector4(v.x, y, v.z, v.w);
        public static Vector4 WithZ(this Vector4 v, float z) => new Vector4(v.x, v.y, z, v.w);
        public static Vector4 WithW(this Vector4 v, float w) => new Vector4(v.x, v.y, v.z, w);
        public static Vector2 ToVector2(this Vector4 v) => new Vector2(v.x, v.y);
        public static Vector3 ToVector3(this Vector4 v) => new Vector3(v.x, v.y, v.z);
    }
}