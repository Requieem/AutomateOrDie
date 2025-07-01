using UnityEngine;

namespace Code.Scripts.Runtime.Characters
{
    [CreateAssetMenu(fileName = "MovementModel", menuName = "Gameplay/MovementModel", order = 0)]
    public class MovementModel : ScriptableObject
    {
        [SerializeField] private float m_maxSpeed;
        [SerializeField] private float m_acceleration;
        [SerializeField] private float m_deceleration;
        [SerializeField] private float m_bounciness;
        [SerializeField] private float m_sprintMultiplier = 1.5f;

        public float MaxSpeed => m_maxSpeed;
        public float Acceleration => m_acceleration;
        public float Deceleration => m_deceleration;
        public float Bounciness => m_bounciness;
        public float SprintMultiplier => m_sprintMultiplier;
    }
}