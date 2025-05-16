using Code.Scripts.Runtime.Grid;
using UnityEngine;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public class BuildingRequirements : MonoBehaviour
    {
        public virtual bool CheckRequirements()
        {
            var gridManager = GridManager.Instance;
            if (!gridManager)
            {
                return false;
            }

            if (!gridManager.SelectedCell.HasValue) return false;
            var canPlace = gridManager.CanPlaceBuilding(gridManager.SelectedCell.Value);
            return canPlace;
        }
    }
}