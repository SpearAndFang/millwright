namespace Millwright.ModSystem
{
    //using System.Diagnostics;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent.Mechanics;

    public class BlockWindmillRotorEnhanced : BlockMPBase //, IMPPowered
    {
        private BlockFacing powerOutFacing;
        private string bladeType;
        private int bladeCount;
        public override void OnLoaded(ICoreAPI api)
        {
            this.powerOutFacing = BlockFacing.FromCode(this.Variant["side"]).Opposite;

            this.bladeType = this.FirstCodePart(1).ToString();
            if (this.bladeType == "double")
            { this.bladeCount = 8; }
            else if (this.bladeType == "three")
            { this.bladeCount = 3; }
            else if (this.bladeType == "six")
            { this.bladeCount = 6; }
            else //single
            { this.bladeCount = 4; }

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
                        if (block is BlockWindmillRotor || block is BlockWindmillRotorEnhanced)
                        { return false; }

                        var toPlaceBlock = world.GetBlock(new AssetLocation("millwright:" + this.FirstCodePart() + "-" + this.bladeType + "-" + face.Opposite.Code));
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
            var be = world.BlockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<BEBehaviorWindmillRotorEnhanced>();
            if (be != null)
            {
                return be.OnInteract(byPlayer);
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }


        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            var be = world.BlockAccessor.GetBlockEntity(selection.Position)?.GetBehavior<BEBehaviorWindmillRotorEnhanced>();
            if (be != null)
            {
                if (this.bladeType == "double")
                {
                    if (be.SailLength >= 5)
                    { return new WorldInteraction[0]; }
                }
                else if (this.bladeType == "three")
                {
                    if (be.SailLength >= 8)
                    { return new WorldInteraction[0]; }
                }
                else if (this.bladeType == "six")
                {
                    if (be.SailLength >= 6)
                    { return new WorldInteraction[0]; }
                }
                else //single
                {
                    if (be.SailLength >= 7)
                    { return new WorldInteraction[0]; }
                }
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
                                new ItemStack(world.GetItem(new AssetLocation("millwright:"+ be.SailType)), this.bladeCount)
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
                        new ItemStack(world.GetItem(new AssetLocation("millwright:sailcentered")), this.bladeCount),
                        new ItemStack(world.GetItem(new AssetLocation("millwright:sailangled")), this.bladeCount),
                        new ItemStack(world.GetItem(new AssetLocation("millwright:sailwide")), this.bladeCount)
                    }
                }
            };
        }
    }
}
