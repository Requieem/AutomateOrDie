using System.Collections.Generic;
using Code.Scripts.Runtime.GameResources;
using UnityEngine;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public abstract class ConveyorBuilding : MonoBehaviour, IConveyorEndpoint
    {
        [SerializeField] protected List<Belt> m_inputBelts;
        [SerializeField] protected List<Belt> m_outputBelts;

        public Transform Transform => transform;

        public virtual void AddInputBelt(Belt belt)
        {
            if (!m_inputBelts.Contains(belt))
                m_inputBelts.Add(belt);
        }

        public virtual void AddOutputBelt(Belt belt)
        {
            if (!m_outputBelts.Contains(belt))
                m_outputBelts.Add(belt);
        }

        public virtual void RemoveInputBelt(Belt belt)
        {
            m_inputBelts.Remove(belt);
        }

        public virtual void RemoveOutputBelt(Belt belt)
        {
            m_outputBelts.Remove(belt);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            foreach (var belt in m_inputBelts)
            {
                belt.RemoveInputBuilding(this);
            }
            foreach (var belt in m_outputBelts)
            {
                belt.RemoveOutputBuilding(this);
            }
        }

        public abstract void ReceiveItem(MonoItem item);
    }
}