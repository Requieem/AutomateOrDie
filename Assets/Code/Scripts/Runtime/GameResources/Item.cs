using UnityEngine;

namespace Code.Scripts.Runtime.GameResources
{
    [CreateAssetMenu(fileName = "Item", menuName = "Gameplay/Item", order = 0)]
    public class Item : Identifier
    {
        [SerializeField] private SpriteRenderer m_itemPrefab;
        public SpriteRenderer ItemPrefab => m_itemPrefab;
    }
}