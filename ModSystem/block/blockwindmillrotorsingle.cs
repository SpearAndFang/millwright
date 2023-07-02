namespace Millwright.ModSystem
{
    //using System.Diagnostics;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent.Mechanics;

    public class BlockWindmillRotorSingle : BlockMPBase //, IMPPowered
    {
        private BlockFacing powerOutFacing;

        public override void OnLoaded(ICoreAPI api)
        {
            this.powerOutFacing = BlockFacing.FromCode(this.Variant["side"]).Opposite;
            base.OnLoaded(api);
        }

        public override void DidConnectAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        { }

        public override bool HasMechPowerConnectorAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        {
            return face == this.powerOutFacing;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (!this.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
            {
                return false;
            }
            foreach (var face in BlockFacing.HORIZONTALS)
            {
                var pos = blockSel.Position.AddCopy(face);
                if (world.BlockAccessor.GetBlock(pos) is IMechanicalPowerBlock block)
                {
                    if (block.HasMechPowerConnectorAt(world, pos, face.Opposite))
                    {
                        //We can use a different approach to this

                        //Prevent rotor back-to-back placement
                        // if (block is IMPPowered)
                        //    return false;
                        if (block is BlockWindmillRotor || block is BlockWindmillRotorSingle || block is BlockWindmillRotorDouble)
                        { return false; }

                        var toPlaceBlock = world.GetBlock(new AssetLocation("millwright:" + this.FirstCodePart() + "-" + this.FirstCodePart(1) + "-" + face.Opposite.Code));
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

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            var be = world.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<BEBehaviorWindmillRotorSingle>();
            if (be != null)
            {
                return be.OnInteract(byPlayer);
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            var be = world.BlockAccessor.GetBlockEntity(selection.Position)?.GetBehavior<BEBehaviorWindmillRotorSingle>();
            if (be != null && be.SailLength >= 5)
            {
                return new WorldInteraction[0];
            }

            if (be != null && be.SailType != null && be.SailType != "")
            {
                return new WorldInteraction[]
                 {
                        new WorldInteraction()
                        {
                            ActionLangCode = "game:heldhelp-addsails",
                            MouseButton = EnumMouseButton.Right,
                            Itemstacks = new ItemStack[] {
                                new ItemStack(world.GetItem(new AssetLocation("millwright:"+ be.SailType)), 4)
                            }
                        }
                 };
            };
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = "game:heldhelp-addsails",
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = new ItemStack[] {
                        new ItemStack(world.GetItem(new AssetLocation("millwright:sailcentered")), 4),
                        new ItemStack(world.GetItem(new AssetLocation("millwright:sailangled")), 4)
                    }
                }
            };
        }
    }
}
