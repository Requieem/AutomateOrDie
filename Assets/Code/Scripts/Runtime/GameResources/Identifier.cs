using UnityEngine;

namespace Code.Scripts.Runtime.GameResources
{
    public abstract class Identifier : ScriptableObject
    {
        [SerializeField] private Sprite m_sprite;
        [SerializeField] private Color m_hintColor;

        public Sprite Sprite => m_sprite;
        public Color HintColor => m_hintColor;
    }
}