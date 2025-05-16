using UnityEngine;

namespace Code.Scripts.Runtime.GameResources
{
    [CreateAssetMenu(fileName = "Item", menuName = "Gameplay/Item", order = 0)]
    public class Item : Identifier
    {
        [SerializeField] private MonoItem m_itemPrefab;
        public MonoItem ItemPrefab => m_itemPrefab;
    }
}