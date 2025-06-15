namespace Millwright.ModSystem
{
    //using System.Diagnostics;
    using System.Linq;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent.Mechanics;

    public class ImprovedBlockAxlePassthrough : BlockMPBase
    {
        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            // https://github.com/issues/assigned?issue=SpearAndFang%7Cmillwright%7C8
            // prevent middle click from putting that weird variant in the inventory
            return null;
        }

        public override int GetRetention(BlockPos pos, BlockFacing facing, EnumRetentionType type)
        {
         return 3;
        }

        public bool IsOrientedTo(BlockFacing facing)
        {
            var dirs = this.LastCodePart();
            return dirs[0] == facing.Code[0] || (dirs.Length > 1 && dirs[1] == facing.Code[0]);
        }


        public override bool HasMechPowerConnectorAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        { return this.IsOrientedTo(face); }


        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);
            foreach (var face in BlockFacing.ALLFACES)
            {
                var pos = blockPos.AddCopy(face);
                var nblock = world.BlockAccessor.GetBlock(pos);
                var faceOpposite = face.Opposite;
                if (nblock is IMechanicalPowerBlock block)
                {

                    if (block != null && block.HasMechPowerConnectorAt(world, pos, faceOpposite))
                    {
                        //make sure neib and passthrough are oriented the same
                        if (IsFacing(world.BlockAccessor.GetBlock(blockPos)) == faceOpposite)
                        {
                            block.DidConnectAt(world, pos, faceOpposite);
                            this.WasPlaced(world, blockPos, face);
                        }
                    }
                }
            }
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            //base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
            var newbe = world.BlockAccessor.GetBlockEntity(pos);
            ItemStack tempStack;
            if (newbe is ImprovedBEAxlePassThrough be)
            {
                tempStack = be.Inventory[0].Itemstack;
                Block toPlaceBlock = world.GetBlock(tempStack.Collectible.Code);
                world.BlockAccessor.SetBlock(toPlaceBlock.BlockId, pos);
                world.BlockAccessor.MarkBlockDirty(pos);
            }
            var assetLoc = new AssetLocation("millwright:improvedaxlepassthroughfull");
            tempStack = new ItemStack(world.GetBlock(assetLoc), 1);
            world.SpawnItemEntity(tempStack, pos.ToVec3d().Add(0.5, 1.1, 0.5));

            world.BlockAccessor.MarkBlockDirty(pos);
        }


        public static BlockFacing IsFacing(Block block)
        {
            switch (block.Variant["rotation"])
            {
                case "ns":
                    return BlockFacing.NORTH;
                case "we":
                    return BlockFacing.WEST;
                case "ud":
                    return BlockFacing.UP;
                
                default:
                    break;
            }
            return BlockFacing.DOWN;
        }



        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            BEBehaviorMPAxle bempaxle = world.BlockAccessor.GetBlockEntity(pos)?.GetBehavior<BEBehaviorMPAxle>();
            if (bempaxle != null && !BEBehaviorMPAxle.IsAttachedToBlock(world.BlockAccessor, bempaxle.Block, pos))
            {
                bool connected = false;
                foreach (BlockFacing face in BlockFacing.ALLFACES)
                {
                    BlockPos npos = pos.AddCopy(face);
                    IMechanicalPowerBlock block = world.BlockAccessor.GetBlock(npos) as IMechanicalPowerBlock;
                    bool prevConnected = connected;
                    if (block != null && block.HasMechPowerConnectorAt(world, pos, face.Opposite) && world.BlockAccessor.GetBlockEntity(pos)?.GetBehavior<BEBehaviorMPBase>()?.disconnected == false) connected = true;
                    BlockAngledGears blockagears = block as BlockAngledGears;
                    if (blockagears == null) continue;
                    if (blockagears.Facings.Contains(face.Opposite) && blockagears.Facings.Length == 1)
                    {
                        world.BlockAccessor.BreakBlock(npos, null);
                        connected = prevConnected;  //undo connected = true in this situation
                    }
                }
                if (!connected)
                {
                    world.BlockAccessor.BreakBlock(pos, null);
                }
            }

            base.OnNeighbourBlockChange(world, pos, neibpos);
        }

        public override void DidConnectAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        { }
    }
}
