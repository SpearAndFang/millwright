namespace Millwright.ModSystem
{
    //using System.Diagnostics;
    using System.Linq;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent.Mechanics;

    public class BlockAxlePassthrough : BlockMPBase
    {
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
                //Test for connection on opposite side as well (the gap)
                pos = blockPos.AddCopy(faceOpposite).AddCopy(faceOpposite);
                nblock = world.BlockAccessor.GetBlock(pos);
                block = nblock as IMechanicalPowerBlock;
                if (block != null && nblock.FirstCodePart() == "woodenaxlepassthrough")
                {
                    block.DidConnectAt(world, pos, face);
                    this.WasPlaced(world, blockPos, faceOpposite);
                    var beb = world.BlockAccessor.GetBlockEntity(pos)?.GetBehavior<BEBehaviorAxlePassthrough>();
                    beb?.TryConnectGap(pos, blockPos, faceOpposite);
                }
            }
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
                case "sn":
                    return BlockFacing.SOUTH;
                case "ew":
                    return BlockFacing.EAST;
                default:
                    break;
            }
            return BlockFacing.DOWN;
        }



        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            //if the gap block breaks we need to break this block too
            var bblock = world.BlockAccessor.GetBlock(pos);
            var facing = IsFacing(bblock);
            var inBlock = world.BlockAccessor.GetBlock(pos.AddCopy(facing));
            if (inBlock?.BlockId == 0) //air
            {
                //break the axle
                world.BlockAccessor.BreakBlock(pos, null);
            }


            //everything below I haven't looked at yet
            var bempaxle = world.BlockAccessor.GetBlockEntity(pos)?.GetBehavior<BEBehaviorAxlePassthrough>();
            if (bempaxle != null) // && !BEBehaviorAxlePassthrough.IsAttachedToBlock(world.BlockAccessor, bempaxle.Block, pos))
            {
                var connected = false;
                foreach (var face in BlockFacing.ALLFACES)
                {

                    var npos = pos.AddCopy(face);
                    var block = world.BlockAccessor.GetBlock(npos) as IMechanicalPowerBlock;
                    var prevConnected = connected;
                    if (block != null && block.HasMechPowerConnectorAt(world, pos, face.Opposite) && world.BlockAccessor.GetBlockEntity(pos)?.GetBehavior<BEBehaviorAxlePassthrough>()?.disconnected == false)
                    { connected = true; }

                    if (!(block is BlockAngledGears blockagears))
                    { continue; }

                    if (blockagears.Facings.Contains(face.Opposite) && blockagears.Facings.Length == 1)
                    {
                        world.BlockAccessor.BreakBlock(npos, null);
                        connected = prevConnected;  //undo connected = true in this situation
                    }
                }
                if (!connected)
                {
                    //world.BlockAccessor.BreakBlock(pos, null);
                }
            }
            base.OnNeighbourBlockChange(world, pos, neibpos);
        }


        public override void DidConnectAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        { }
    }
}
