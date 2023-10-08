namespace Millwright.ModSystem
{
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.MathTools;
    //using System.Diagnostics;


    public class BlockAxlePassthroughFull : Block
    {
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (!this.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
            { return false; }
            var pos = blockSel.Position.AddCopy(blockSel.Face.Opposite);
            var block = world.BlockAccessor.GetBlock(pos, BlockLayersAccess.Default);
            // make sure it's a relatively basic block
            if (this.api.Side == EnumAppSide.Client)
            {
                if (block?.Class == null || block?.EntityClass != null)
                {
                    failureCode = Lang.Get("millwright:blockdesc-suitable-block-needed");
                    return false;
                }
            }
            // make sure it's a full block - like the first selection box is 0-1
            var selBoxes = block.SelectionBoxes;
            if (selBoxes == null)
            {
                failureCode = Lang.Get("millwright:blockdesc-suitable-block-needed");
                return false;
            } //this shouldn't be possible
            else
            {
                var selBox = selBoxes[0];
                if ((selBox.XSize != 1f) || (selBox.YSize != 1f) || (selBox.ZSize != 1f))
                {
                    failureCode = Lang.Get("millwright:blockdesc-suitable-block-needed");
                    return false;
                }
            }
            // finally we need to ensure there isn't already a passthrough in any direction
            // other than directly across from this one
            foreach (var nface in BlockFacing.ALLFACES)
            {
                if (nface != blockSel.Face && nface != blockSel.Face.Opposite)
                {
                    var npos = pos.AddCopy(nface);
                    var npath = world.BlockAccessor.GetBlock(npos).FirstCodePart();
                    if (npath == "woodenaxlepassthrough")
                    {
                        failureCode = Lang.Get("millwright:blockdesc-suitable-block-needed");
                        return false;
                    }
                }
            }
            var face = blockSel.Face;
            var faceOpposite = blockSel.Face.Opposite;
            var loc = new AssetLocation("millwright:woodenaxlepassthrough-" + faceOpposite.Code[0] + face.Code[0]);
            var toPlaceBlock = world.GetBlock(loc);
            world.BlockAccessor.SetBlock(toPlaceBlock.BlockId, blockSel.Position);
            return true;
        }
    }
}
