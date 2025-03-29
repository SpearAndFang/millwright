namespace Millwright.ModSystem

{
    using System.Collections.Generic;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent.Mechanics;

    public class BlockWindmillRotorUD : BlockMPBase
    {
        private BlockFacing powerOutFacing;
        public override void OnLoaded(ICoreAPI api)
        {
            this.powerOutFacing = BlockFacing.UP;
            base.OnLoaded(api);
        }

        public override void DidConnectAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        {
        }

        public override bool HasMechPowerConnectorAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        {
            if (face == BlockFacing.UP) return true;
            return false;
        }


        
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            var be = world.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<BEBehaviorWindmillRotorUD>();
            if (be != null)
            {
                return be.OnInteract(byPlayer);
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
        

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (!this.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
            {
                return false;
            }

            foreach (var face in BlockFacing.ALLFACES)
            {
                var pos = blockSel.Position.AddCopy(face);
                if (world.BlockAccessor.GetBlock(pos) is IMechanicalPowerBlock block)
                {
                    if (block.HasMechPowerConnectorAt(world, pos, face.Opposite))
                    {
                        //We can use a different approach to this

                        //Prevent rotor back-to-back placement
                        // if (block is IMPPowered)s
                        //    return false;
                        //if (block is BlockWindmillRotor || block is BlockWindmillRotorEnhanced)
                        //{ return false; }

                        var toPlaceBlock = world.GetBlock(new AssetLocation("millwright:windmillrotorud-three-up"));
                        world.BlockAccessor.SetBlock(toPlaceBlock.BlockId, blockSel.Position);

                        block.DidConnectAt(world, pos, face.Opposite);
                        this.WasPlaced(world, blockSel.Position, face);

                        return true;
                    }
                }
            }
            var ok = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            if (ok)
            {
                this.WasPlaced(world, blockSel.Position, null);
            }
            return ok;

        }
    }
}