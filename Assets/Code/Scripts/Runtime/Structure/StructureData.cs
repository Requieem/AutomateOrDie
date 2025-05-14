using UnityEngine;

namespace Code.Scripts.Runtime.Structure
{
    [CreateAssetMenu(fileName = "StructureData", menuName = "Gameplay/StructureData")]
    public class StructureData : ScriptableObject
    {
        [SerializeField] private AnimationClip m_activeClip;
        [SerializeField] private AnimationClip m_inactiveClip;
    }
}
