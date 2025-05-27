namespace Millwright.ModSystem
{
    //using System.Diagnostics;
    using Vintagestory.API.Common;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.Server;
    using System.Text;
    using Vintagestory.API.Config;
    using Vintagestory.API.Client;
    using Vintagestory.GameContent;
    using System;

    public class ImprovedBEAxlePassThrough : BlockEntityDisplay //BlockEntity
    {
        private ICoreServerAPI sapi;

        public override string InventoryClassName => "improvedaxle";
        protected InventoryGeneric inventory;
        public override InventoryBase Inventory => inventory;

        private readonly int maxSlots = 1;
        //float[] rotations = new float[4];

        public ItemSlot BlockSlot => this.inventory[0];

        public ItemStack BlockStack
        {
            get => this.inventory[0].Itemstack;
            set => this.inventory[0].Itemstack = value;
        }

        public ImprovedBEAxlePassThrough()
        {
            this.inventory = new InventoryGeneric(this.maxSlots, null, null);
            var meshes = new MeshData[this.maxSlots];
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            this.sapi = api as ICoreServerAPI;
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            RedrawAfterReceivingTreeAttributes(worldForResolving);     // Redraw on client after we have completed receiving the update from server
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
        }

        

        protected override float[][] genTransformationMatrices()
        {
            float[][] tfMatrices = new float[1][];
            tfMatrices[0] = new Matrixf().Values;
            return tfMatrices;
        }

        /*
        public override void AddMiningTierInfo(StringBuilder sb)
        {
            if (Code.PathStartsWith("log-grown"))
            {
                // stone axe can cut normal wood (woodtier 3) cannot cut tropical woods except Kapok (which is soft); copper/scrap axe cannot cut ebony
                int woodTier = Attributes?["treeFellingGroupSpreadIndex"].AsInt(0) ?? 0;
                woodTier += RequiredMiningTier - 4;
                if (woodTier < RequiredMiningTier) woodTier = RequiredMiningTier;

                string tierName = "?";
                if (woodTier < miningTierNames.Length)
                {
                    tierName = miningTierNames[woodTier];
                }

                sb.AppendLine(Lang.Get("Requires tool tier {0} ({1}) to break", woodTier, tierName == "?" ? tierName : Lang.Get(tierName)));
            }
            else
            {
                base.AddMiningTierInfo(sb);
            }
        }
        */

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            var block = this.Api.World.BlockAccessor.GetBlock(this.Pos, BlockLayersAccess.Default);

            if (!this.BlockSlot.Empty)
            {
                //sb.AppendLine(Lang.GetMatching("millwright:axle-insideof") + ": " + Lang.GetMatching(inventory[0].Itemstack.GetName()));
                //sb.AppendLine();
                

                /*
                 * oh shit this gets complicated
                 * let's swap the block back in instead and drop just the axle
                 * 
                var tooltier = BlockStack.Collectible.ToolTier;
                if (tooltier == 0)
                {
                    tooltier = BlockStack.Block.RequiredMiningTier;
                }
                string[] miningTierNames = new string[] { "tier_hands", "tier_stone", "tier_copper", "tier_bronze", "tier_iron", "tier_steel", "tier_titanium" };

                if (BlockStack.Collectible.Code.PathStartsWith("log-grown"))
                {
                    // stone axe can cut normal wood (woodtier 3) cannot cut tropical woods except Kapok (which is soft); copper/scrap axe cannot cut ebony
                    int woodTier = BlockStack.Collectible.Attributes?["treeFellingGroupSpreadIndex"].AsInt(0) ?? 0;
                    var requiredMiningTier = tooltier;
                    woodTier += requiredMiningTier - 4;
                    if (woodTier < requiredMiningTier) woodTier = requiredMiningTier;

                    string tierName = "?";
                    if (woodTier < miningTierNames.Length)
                    {
                        tierName = miningTierNames[woodTier];
                    }

                    sb.AppendLine(Lang.Get("Requires tool tier {0} ({1}) to break", woodTier, tierName == "?" ? tierName : Lang.Get(tierName)));
                }
                else
                {
                    string tierName = "?";
                    if (tooltier < miningTierNames.Length)
                    {
                        tierName = miningTierNames[tooltier];
                    }

                    sb.AppendLine(Lang.Get("Requires tool tier {0} ({1}) to break", tooltier, tierName == "?" ? tierName : Lang.Get(tierName)));
                }

                sb.AppendLine(); */

            }
        }
    }
}
