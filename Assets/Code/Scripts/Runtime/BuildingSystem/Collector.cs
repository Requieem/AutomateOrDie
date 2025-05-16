using Code.Scripts.Runtime.GameResources;

namespace Code.Scripts.Runtime.BuildingSystem
{
    public class Collector : ConveyorBuilding
    {
        public override bool IsOccupied() => false;
        public override bool AcceptsOutput => false;

        public override void ReceiveItem(MonoItem item)
        {
            var gameState = GameState.Instance;
            gameState.UseResource(item.Item, -1f);

            if(item && item.gameObject)
                Destroy(item.gameObject);
        }
    }
}