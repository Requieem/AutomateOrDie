using Code.Scripts.Runtime.Grid;
using UnityEngine;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public class ExtractorRequirements : BuildingRequirements
    {
        public override bool CheckRequirements()
        {
            var gridManager = GridManager.Instance;
            if (!gridManager)
            {
                return false;
            }

            if (!gridManager.SelectedCell.HasValue) return false;
            var canPlace = gridManager.CanPlaceBuilding(gridManager.SelectedCell.Value, true);
            return canPlace;
        }
    }
}