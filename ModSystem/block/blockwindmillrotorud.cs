namespace Millwright.ModSystem

{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent.Mechanics;

    public class BlockWindmillRotorUD : BlockMPBase //, IMPPowered
    {
        private BlockFacing powerOutFacing;
        private string bladeType;

        public override void OnLoaded(ICoreAPI api)
        {
            this.powerOutFacing = BlockFacing.FromCode(this.Variant["side"]).Opposite;
            //this.powerOutFacing = BlockFacing.UP;

            this.bladeType = this.FirstCodePart(1).ToString();
            base.OnLoaded(api);
        }

        public override void DidConnectAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        { }


        public override bool HasMechPowerConnectorAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        {
            return face == this.powerOutFacing;
            //if (face == BlockFacing.UP) return true;
            //return false;
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
                        var rotorAsset = "millwright:" + this.FirstCodePart() + "-" + this.bladeType + "-up";
                        Debug.WriteLine(rotorAsset);
                        var toPlaceBlock = world.GetBlock(rotorAsset);

                        if (toPlaceBlock != null)
                        {
                            world.BlockAccessor.SetBlock(toPlaceBlock.BlockId, blockSel.Position);
                            block.DidConnectAt(world, pos, face.Opposite);
                            this.WasPlaced(world, blockSel.Position, face);
                            return true;
                        }
                        return false;
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
            var be = world.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<BEBehaviorWindmillRotorUD>();
            if (be != null)
            {
                return be.OnInteract(byPlayer);
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            var be = world.BlockAccessor.GetBlockEntity(selection.Position)?.GetBehavior<BEBehaviorWindmillRotorUD>();
            if (be != null)
            {
                if (be.SailLength < 8)
                {
                    return new WorldInteraction[]
                    {
                            new WorldInteraction()
                            {
                                ActionLangCode = "game:heldhelp-addsails",
                                MouseButton = EnumMouseButton.Right,
                                Itemstacks = new ItemStack[] {

                                    new ItemStack(world.GetItem(new AssetLocation("millwright:sailassembly-three-sailwide")), 1)
                                }
                            }
                    };
                }
            }
            return new WorldInteraction[0];
        }
    }
}