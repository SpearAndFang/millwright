namespace Millwright.ModSystem
{
    using System;
    using System.Text;
    //using System.Diagnostics;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent;
    using Vintagestory.GameContent.Mechanics;
    using Millwright.ModConfig;

    public class BEBehaviorWindmillRotorEnhanced : BEBehaviorMPRotor
    {
        private WeatherSystemBase weatherSystem;
        private double windSpeed;

        public int SailLength { get; private set; } = 0;
        public string SailType { get; private set; } = "";

        private string bladeType;
        private int bladeCount;
        private AssetLocation sound;
        protected override AssetLocation Sound => this.sound;
        protected override float GetSoundVolume()
        {
            return (0.5f + (0.5f * (float)this.windSpeed)) * this.SailLength / 3f;
        }

        private readonly float centeredbladeModifier = (float)ModConfig.Loaded.SailCenteredModifier;
        private readonly float angledbladeModifier = (float)ModConfig.Loaded.SailCenteredModifier;

        public float bladeModifier = 1.0f;

        protected override float Resistance => 0.003f;
        protected override double AccelerationFactor => 0.05d + (this.bladeModifier / 4);
        protected override float TargetSpeed => (float)Math.Min(0.6f * this.bladeModifier, this.windSpeed * this.bladeModifier);

        protected override float TorqueFactor => this.SailLength * this.bladeModifier / 4f;    // Should stay at /4f (5 sails are supposed to have "125% power output")

        public BEBehaviorWindmillRotorEnhanced(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            if (this.SailType == "sailangled")
            { this.bladeModifier = this.angledbladeModifier; }
            else
            { this.bladeModifier = this.centeredbladeModifier; }

            this.bladeType = this.Block.FirstCodePart(1).ToString();
            if (this.bladeType == "double")
            {
                this.bladeCount = 8;
                this.bladeModifier *= 2f;
            }
            else if (this.bladeType == "three")
            {
                this.bladeCount = 3;
                this.bladeModifier *= 0.75f;
            }
            else if (this.bladeType == "six")
            {
                this.bladeCount = 6;
                this.bladeModifier *= 1.5f;
            }
            else //single
            { this.bladeCount = 4; }

            base.Initialize(api, properties);
            this.sound = new AssetLocation("game:sounds/effect/swoosh");
            this.weatherSystem = this.Api.ModLoader.GetModSystem<WeatherSystemBase>();
            this.Blockentity.RegisterGameTickListener(this.CheckWindSpeed, 1000);
        }

        private void CheckWindSpeed(float dt)
        {
            this.windSpeed = this.weatherSystem.WeatherDataSlowAccess.GetWindSpeed(this.Blockentity.Pos.ToVec3d());
            if (this.Api.World.BlockAccessor.GetLightLevel(this.Blockentity.Pos, EnumLightLevelType.OnlySunLight) < 5 && this.Api.World.Config.GetString("undergroundWindmills", "false") != "true")
            {
                this.windSpeed = 0;
            }

            if (this.Api.Side == EnumAppSide.Server && this.SailLength > 0 && this.Api.World.Rand.NextDouble() < 0.2)
            {
                if (this.Obstructed(this.SailLength + 1))
                {
                    this.Api.World.PlaySoundAt(new AssetLocation("game:sounds/effect/toolbreak"), this.Position.X + 0.5, this.Position.Y + 0.5, this.Position.Z + 0.5, null, false, 20, 1f);
                    while (this.SailLength-- > 0)
                    {
                        //for downwards compatibility
                        var sail = "";
                        if (this.SailType == null || this.SailType == "")
                        { sail = "sailcentered"; }
                        else
                        { sail = this.SailType; }

                        var assetLoc = new AssetLocation("millwright:" + sail);
                        var stacks = new ItemStack(this.Api.World.GetItem(assetLoc), this.bladeCount);
                        this.Api.World.SpawnItemEntity(stacks, this.Blockentity.Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                    }
                    this.SailLength = 0;
                    this.SailType = "";
                    this.Blockentity.MarkDirty(true);
                    this.network.updateNetwork(this.manager.getTickNumber());
                }
            }
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            //for downwards compatibility
            var sail = "";
            if (this.SailType == null || this.SailType == "")
            { sail = "sailcentered"; }
            else
            { sail = this.SailType; }

            while (this.SailLength-- > 0)
            {
                var assetLoc = new AssetLocation("millwright:" + sail);
                var stacks = new ItemStack(this.Api.World.GetItem(assetLoc), this.bladeCount);
                this.Api.World.SpawnItemEntity(stacks, this.Blockentity.Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            base.OnBlockBroken(byPlayer);
        }

        internal bool OnInteract(IPlayer byPlayer)
        {
            if (this.SailLength >= 5)
            {
                return false;
            }

            var slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Empty || slot.StackSize < this.bladeCount)
            {
                return false;
            }

            //for downwards compatibility
            var sail = "";
            if (this.SailType == null || this.SailType == "")
            { sail = "sailcentered"; }
            else
            { sail = this.SailType; }

            sail = slot.Itemstack.Collectible.Code.Path;
            if (!sail.StartsWith("sail") || slot.Itemstack.Collectible.Code.Domain != "millwright")
            { return false; }

            if (this.SailLength > 0 && this.SailType != sail)
            { return false; }

            var assetLoc = new AssetLocation("millwright:" + sail);
            var sailStack = new ItemStack(this.Api.World.GetItem(assetLoc));
            if (!slot.Itemstack.Equals(this.Api.World, sailStack, GlobalConstants.IgnoredStackAttributes))
            {
                return false;
            }

            var len = this.SailLength + 2;

            if (this.Obstructed(len))
            {
                if (this.Api.Side == EnumAppSide.Client)
                {
                    (this.Api as ICoreClientAPI).TriggerIngameError(this, "notenoughspace", Lang.Get("Cannot add more sails. Make sure there's space for the sails to rotate freely"));
                }
                return false;
            }

            if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
            {
                slot.TakeOut(this.bladeCount);
                slot.MarkDirty();
            }
            this.SailLength++;
            this.SailType = sail;

            this.updateShape(this.Api.World);

            this.Blockentity.MarkDirty(true);
            return true;
        }

        private bool Obstructed(int len)
        {
            var tmpPos = new BlockPos();

            for (var dxz = -len; dxz <= len; dxz++)
            {
                for (var dy = -len; dy <= len; dy++)
                {
                    if (dxz == 0 && dy == 0)
                    {
                        continue;
                    }

                    if (len > 1 && Math.Abs(dxz) == len && Math.Abs(dy) == len)
                    {
                        continue;
                    }

                    var dx = this.ownFacing.Axis == EnumAxis.Z ? dxz : 0;
                    var dz = this.ownFacing.Axis == EnumAxis.X ? dxz : 0;
                    tmpPos.Set(this.Position.X + dx, this.Position.Y + dy, this.Position.Z + dz);

                    var block = this.Api.World.BlockAccessor.GetBlock(tmpPos);
                    var collBoxes = block.GetCollisionBoxes(this.Api.World.BlockAccessor, tmpPos);
                    if (collBoxes != null && collBoxes.Length > 0 && !(block is BlockSnowLayer) && !(block is BlockSnow))
                    {

                        return true;
                    }
                }
            }

            return false;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            this.SailLength = tree.GetInt("sailLength");
            this.SailType = tree.GetString("sailType");
            base.FromTreeAttributes(tree, worldAccessForResolve);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetInt("sailLength", this.SailLength);
            tree.SetString("sailType", this.SailType);
            base.ToTreeAttributes(tree);
        }

        protected override void updateShape(IWorldAccessor worldForResolve)
        {
            if (worldForResolve.Side != EnumAppSide.Client || this.Block == null)
            {
                return;
            }

            if (this.SailLength == 0)
            {
                this.Shape = new CompositeShape()
                {
                    Base = new AssetLocation("millwright:block/wood/mechanics/" + this.bladeType + "/windmillrotor"),
                    rotateY = this.Block.Shape.rotateY
                };
            }
            else
            {
                //for downwards compatibility
                var sail = "";
                if (this.SailType == null || this.SailType == "")
                { sail = "sailcentered"; }
                else
                { sail = this.SailType; }

                this.Shape = new CompositeShape()
                {
                    Base = new AssetLocation("millwright:block/wood/mechanics/" + this.bladeType + "/" + sail + "/windmill-" + this.SailLength + "blade"),
                    rotateY = this.Block.Shape.rotateY
                };
            }
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            base.GetBlockInfo(forPlayer, sb);

            sb.AppendLine(string.Format(Lang.Get("Wind speed: {0}%", (int)(100 * this.windSpeed))));
            sb.AppendLine(Lang.Get("Sails power output: {0} kN", (int)(this.SailLength * this.bladeModifier / 5f * 100f)));
            sb.AppendLine();
            sb.AppendLine("<font color=\"#edca98\"><i>Millwright</i></font>");
        }
    }
}
