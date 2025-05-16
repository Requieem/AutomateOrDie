using Code.Scripts.Runtime.GameResources;
using UnityEngine;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public interface IConveyorEndpoint
    {
        Transform Transform { get; }
        void ReceiveItem(MonoItem item);
        void AddInputBelt(Belt belt);
        void AddOutputBelt(Belt belt);
        void RemoveInputBelt(Belt belt);
        void RemoveOutputBelt(Belt belt);
    }
}