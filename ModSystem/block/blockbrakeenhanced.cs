namespace Millwright.ModSystem
{
    using System.Linq;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent.Mechanics;

    public class BlockBrakeEnhanced : BlockMPBase
    {
        public bool IsOrientedTo(BlockFacing facing)
        {
            return facing.Code == this.Variant["side"];
        }


        public override bool HasMechPowerConnectorAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        {
            var ownFacing = BlockFacing.FromCode(this.Variant["side"]);
            var leftFacing = BlockFacing.HORIZONTALS_ANGLEORDER[GameMath.Mod(ownFacing.HorizontalAngleIndex - 1, 4)];
            var rightFacing = BlockFacing.HORIZONTALS_ANGLEORDER[GameMath.Mod(ownFacing.HorizontalAngleIndex + 1, 4)];
            return face == leftFacing || face == rightFacing;
        }


        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (!this.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
            { return false; }

            var horVer = SuggestedHVOrientation(byPlayer, blockSel);
            var blockCode = this.CodeWithParts(horVer[0].Code);
            var orientedBlock = world.BlockAccessor.GetBlock(blockCode);

            var ownFacing = BlockFacing.FromCode(orientedBlock.Variant["side"]);
            var leftFacing = BlockFacing.HORIZONTALS_ANGLEORDER[GameMath.Mod(ownFacing.HorizontalAngleIndex - 1, 4)];
            var rightFacing = BlockFacing.HORIZONTALS_ANGLEORDER[GameMath.Mod(ownFacing.HorizontalAngleIndex + 1, 4)];

            if (world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(leftFacing)) is IMechanicalPowerBlock leftBlock)
            { return this.DoPlaceMechBlock(world, byPlayer, itemstack, blockSel, orientedBlock, leftBlock, leftFacing); }

            if (world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(rightFacing)) is IMechanicalPowerBlock rightBlock)
            { return this.DoPlaceMechBlock(world, byPlayer, itemstack, blockSel, orientedBlock, rightBlock, rightFacing); }


            var frontFacing = ownFacing;
            var backFacing = ownFacing.Opposite;
            var rotBlock = world.GetBlock(orientedBlock.CodeWithVariant("side", leftFacing.Code));

            if (world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(frontFacing)) is IMechanicalPowerBlock frontBlock)
            { return this.DoPlaceMechBlock(world, byPlayer, itemstack, blockSel, rotBlock, frontBlock, frontFacing); }

            if (world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(backFacing)) is IMechanicalPowerBlock backBlock)
            { return this.DoPlaceMechBlock(world, byPlayer, itemstack, blockSel, rotBlock, backBlock, backFacing); }


            if (base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode))
            {
                this.WasPlaced(world, blockSel.Position, null);
                return true;
            }
            return false;
        }


        private bool DoPlaceMechBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, Block block, IMechanicalPowerBlock connectingBlock, BlockFacing connectingFace)
        {
            if (block.DoPlaceBlock(world, byPlayer, blockSel, itemstack))
            {
                connectingBlock.DidConnectAt(world, blockSel.Position.AddCopy(connectingFace), connectingFace.Opposite);
                this.WasPlaced(world, blockSel.Position, connectingFace);
                return true;
            }

            return false;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            var bempaxle = world.BlockAccessor.GetBlockEntity(pos)?.GetBehavior<BEBehaviorMPAxle>();
            if (bempaxle != null && !BEBehaviorMPAxle.IsAttachedToBlock(world.BlockAccessor, bempaxle.Block, pos))
            {
                foreach (var face in BlockFacing.HORIZONTALS)
                {
                    var npos = pos.AddCopy(face);
                    if (!(world.BlockAccessor.GetBlock(npos) is BlockAngledGears blockagears))
                    { continue; }
                    if (blockagears.Facings.Contains(face.Opposite) && blockagears.Facings.Length == 1)
                    {
                        world.BlockAccessor.BreakBlock(npos, null);
                    }
                }
            }
            base.OnNeighbourBlockChange(world, pos, neibpos);
        }


        public override void DidConnectAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        { }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            var bebrakeenhanced = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEBrakeEnhanced;
            return bebrakeenhanced?.OnInteract(byPlayer) == true;
        }
    }
}
