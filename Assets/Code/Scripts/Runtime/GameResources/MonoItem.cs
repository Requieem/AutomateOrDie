using UnityEngine;

namespace Code.Scripts.Runtime.GameResources
{
    public class MonoItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_spriteRenderer;
        [SerializeField] private Item m_item;

        public Item Item => m_item;
        public SpriteRenderer SpriteRenderer => m_spriteRenderer;

        public void Initialize(Item item)
        {
            m_item = item;
            if (m_spriteRenderer != null)
            {
                m_spriteRenderer.sprite = item.Sprite;
            }
        }
    }
}