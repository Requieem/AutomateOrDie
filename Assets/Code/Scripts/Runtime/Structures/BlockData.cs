using UnityEngine;

namespace Code.Scripts.Runtime.Structures
{
    [CreateAssetMenu(fileName = "BlockData", menuName = "Environment/BlockData", order = 0)]
    public class BlockData : ScriptableObject
    {
        [SerializeField] private Sprite m_sprite;
        [SerializeField] private SpriteRenderer m_blockPrefab;
    }
}