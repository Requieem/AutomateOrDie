using System.Collections.Generic;
using Code.Scripts.Common;
using Code.Scripts.Runtime.GameResources;
using Code.Scripts.Runtime.Grid;
using UnityEngine;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public abstract class ConveyorBuilding : MonoBehaviour
    {
        [SerializeField] protected ConveyorBuilding m_input;
        [SerializeField] protected ConveyorBuilding m_output;
        [SerializeField] protected ConveyDirection m_inputDirection;
        [SerializeField] protected ConveyDirection m_outputDirection;

        public Transform Transform => transform;
        public virtual bool AcceptsInput => !m_input;
        public virtual bool AcceptsOutput => !m_output;
        public ConveyDirection InputDirection => m_inputDirection;
        public ConveyDirection OutputDirection => m_outputDirection;
        public ConveyorBuilding Input => m_input;
        public ConveyorBuilding Output => m_output;

        private void Start() => Setup();
        protected virtual void Setup() => DetectNeighbors();

        private void OnDestroy() => CleanUp();
        protected virtual void CleanUp()
        {
            if(m_input)
                m_input.RemoveOutput(this);
            if(m_output)
                m_output.RemoveInput(this);
        }

        public abstract bool IsOccupied();
        public virtual void AddInput(ConveyorBuilding input, ConveyDirection direction)
        {
            if(m_input && input != m_input)
                throw new System.Exception($"GameObject {gameObject.name} already has input to the conveyor endpoint. Check where you are adding the input, as this is not intended.");
            if(input == this)
                throw new System.Exception($"GameObject {gameObject.name} cannot have input to itself");
            if(direction == ConveyDirection.None || direction == m_outputDirection)
                throw new System.Exception($"GameObject {gameObject.name} cannot have input with no direction or the same direction as the output");

            m_input = input;
            m_inputDirection = direction;
        }
        public virtual void AddOutput(ConveyorBuilding output, ConveyDirection direction)
        {
            if(m_output && output != m_output)
                throw new System.Exception($"GameObject {gameObject.name} already has output to the conveyor endpoint. Check where you are adding the output, as this is not intended.");
            if(output == this)
                throw new System.Exception($"GameObject {gameObject.name} cannot have output to itself");
            if(direction == ConveyDirection.None || direction == m_inputDirection)
                throw new System.Exception($"GameObject {gameObject.name} cannot have output with no direction or the same direction as the input");

            m_output = output;
            m_outputDirection = direction;
        }
        public virtual void RemoveInput(ConveyorBuilding input)
        {
            if (m_input != input) return;

            m_input = null;
            m_inputDirection = ConveyDirection.None;
        }
        public virtual void RemoveOutput(ConveyorBuilding output)
        {
            if (m_output != output) return;

            m_output = null;
            m_outputDirection = ConveyDirection.None;
        }
        public virtual void ReverseChain()
        {
            var queue = new Queue<ConveyorBuilding>();
            var visited = new HashSet<ConveyorBuilding>();
            queue.Enqueue(this);
            visited.Add(this);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                (current.m_input, current.m_output) = (current.m_output, current.m_input);
                (current.m_inputDirection, current.m_outputDirection) = (current.m_outputDirection, current.m_inputDirection);
                if (current.m_input != null && !visited.Contains(current.m_input))
                {
                    queue.Enqueue(current.m_input);
                    visited.Add(current.m_input);
                }

                if (current.m_output != null && !visited.Contains(current.m_output))
                {
                    queue.Enqueue(current.m_output);
                    visited.Add(current.m_output);
                }
            }
        }
        public virtual void DetectNeighbors()
        {
            var gridManager = GridManager.Instance;
            var worldPos = transform.position;
            gridManager.TrySnapPosition(worldPos.ToVector2(), out var snappedPos);
            gridManager.WorldToGrid(snappedPos, out var gridPos);
            var directions = ConveyorUtility.DirectionVectors();
            foreach (var dir in directions)
            {
                var neighborPos = gridPos + dir.Item2;
                if (!gridManager.TryGetBuilding(neighborPos, out var neighbor) || !neighbor.TryGetComponent(out ConveyorBuilding neighboringConveyor)) continue;
                if (AcceptsInput && neighboringConveyor.AcceptsOutput)
                {
                    neighboringConveyor.AddOutput(this, ConveyorUtility.Opposite(dir.Item1));
                    AddInput(neighboringConveyor, dir.Item1);
                }
                else if (AcceptsOutput && neighboringConveyor.AcceptsInput)
                {
                    neighboringConveyor.AddInput(this, ConveyorUtility.Opposite(dir.Item1));
                    AddOutput(neighboringConveyor, dir.Item1);
                }
            }
        }
        public abstract void ReceiveItem(MonoItem item);
    }
}