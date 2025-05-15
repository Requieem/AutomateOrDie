using UnityEngine;

namespace Code.Scripts.Runtime.Structures
{
    [CreateAssetMenu(fileName = "StructureData", menuName = "Gameplay/StructureData")]
    public class StructureData : ScriptableObject
    {
        [SerializeField] private AnimationClip m_activeClip;
        [SerializeField] private AnimationClip m_inactiveClip;
        [SerializeField] private Structure m_structurePrefab;
    }
}
