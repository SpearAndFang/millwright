namespace Millwright.ModSystem
{
    //using System.Diagnostics;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Server;


    public class BEAxlePassThrough : BlockEntity
    {
        private ICoreServerAPI sapi;
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            this.sapi = api as ICoreServerAPI;
        }


        public void ReconnectGap(ICoreAPI api)
        {
            if (api is ICoreServerAPI)
            {
                foreach (var face in BlockFacing.ALLFACES)
                {
                    var blockPos = this.Pos;
                    var faceOpposite = face.Opposite;
                    var pos = blockPos.AddCopy(faceOpposite).AddCopy(faceOpposite);
                    var beb = api.World.BlockAccessor.GetBlockEntity(pos)?.GetBehavior<BEBehaviorAxlePassthrough>();
                    beb?.TryConnectGap(pos, blockPos, face);
                }
            }
        }


        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            this.ReconnectGap(this.sapi);
        }
    }
}
