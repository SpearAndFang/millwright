namespace Millwright.ModSystem
{
    //using System.Diagnostics;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent;
    using Vintagestory.GameContent.Mechanics;

    public class ItemSailCustom : Item
    {

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.PreventDefaultAction;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel == null || byEntity == null)
            { return; }
            var worldAcc = byEntity.World.BlockAccessor;
            if (worldAcc == null)
            { return; }

            var block = worldAcc.GetBlock(blockSel.Position, BlockLayersAccess.Default);

            


            if (block.FirstCodePart() == "windmillrotor")
            {
                //still need to verify no sails

                int bladeCount;
                var bladeType = block.FirstCodePart(1);
                if (bladeType == "double")
                { bladeCount = 8; }
                else if (bladeType == "three")
                { bladeCount = 3; }
                else if (bladeType == "six")
                { bladeCount = 6; }
                else //single
                { bladeCount = 4; }


                var stack = slot.Itemstack;

                if (stack.StackSize < bladeCount)
                {
                    if (api.Side == EnumAppSide.Client)
                    {
                        (api as ICoreClientAPI).TriggerIngameError(this, "needmoresails", Lang.Get("game:placefailure-more-sails-needed"));
                    }
                    return;
                }

                //swap the block
                string newRotorAsset = "millwright:windmillrotorcustom-" + block.FirstCodePart(1) + "-" + stack.Collectible.FirstCodePart(1) + "-" + stack.Collectible.FirstCodePart(2) + "-" + block.LastCodePart();
                var newblock = worldAcc.GetBlock(new AssetLocation(newRotorAsset));
                worldAcc.SetBlock(newblock.BlockId, blockSel.Position);
                
                worldAcc.MarkBlockDirty(blockSel.Position);

                block = (worldAcc.GetBlock(blockSel.Position)) as BlockWindmillRotorEnhanced;

                if (block != null)
                {
                    IPlayer byPlayer = (byEntity as EntityPlayer).Player;
                    block.OnBlockInteractStart(byEntity.World, byPlayer, blockSel);
                }

                /*
                if ((api as ICoreClientAPI)?.World.Player.WorldData.CurrentGameMode != EnumGameMode.Creative)
                {
                    slot.TakeOut(bladeCount);
                    slot.MarkDirty();
                }
                */
            }
        }
    }
}
