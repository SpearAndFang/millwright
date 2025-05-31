namespace Millwright.ModSystem
{
    using System;
    using System.Text;
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.Datastructures;
    using Vintagestory.GameContent.Mechanics;
    using Vintagestory.API.Client;
    using Vintagestory.API.MathTools;
    using Vintagestory.GameContent;
    using Millwright.ModConfig;


    internal class BEBehaviorWindmillRotorUD : BEBehaviorMPRotor
    {
        private WeatherSystemBase weatherSystem;
        private double windSpeed;

        public int SailLength { get; private set; } = 0;
        public string SailType { get; private set; } = "";
        private string bladeType;
        private AssetLocation sound;
        protected override AssetLocation Sound => this.sound;

        protected override float GetSoundVolume()
        {
            return (0.5f + (0.5f * (float)this.windSpeed)) * this.SailLength / 3f;
        }

        private readonly float widebladeModifier = (float)ModConfig.Loaded.SailWideModifier;
        private readonly float sailRotationModifier = (float)ModConfig.Loaded.SailRotationModifier;

        public float bladeModifier = 1.0f;

        protected override float Resistance => 0.003f;
        protected override double AccelerationFactor => 0.05d + (this.bladeModifier / 4);

        protected override float TargetSpeed => (float)Math.Min(0.6f * this.bladeModifier, this.windSpeed * this.bladeModifier);

        protected override float TorqueFactor => this.SailLength * this.bladeModifier / 4f;    // Should stay at /4f (5 sails are supposed to have "125% power output")



        public BEBehaviorWindmillRotorUD(BlockEntity blockentity) : base(blockentity)
        { }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            this.bladeModifier = this.widebladeModifier;
            this.bladeType = this.Block.FirstCodePart(1).ToString();

            if (this.bladeType == "two")
                this.bladeModifier *= 0.667f;
            else //three
            {
                this.bladeModifier *= 1f;
            }
            base.Initialize(api, properties);
            this.sound = new AssetLocation("game:sounds/effect/swoosh");
            this.weatherSystem = this.Api.ModLoader.GetModSystem<WeatherSystemBase>();
            this.Blockentity.RegisterGameTickListener(this.CheckWindSpeed, 1000);
            
            AxisSign = new int[] { 0, 1, 0 };
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
                if (this.Obstructed(this.SailLength))
                {
                    this.Api.World.PlaySoundAt(new AssetLocation("game:sounds/effect/toolbreak"), this.Position.X + 0.5, this.Position.Y + 0.5, this.Position.Z + 0.5, null, false, 20, 1f);
                    var assetLoc = new AssetLocation("millwright:" + "sailassembly-" + this.bladeType + "-" + this.SailType);

                    while (this.SailLength-- > 0)
                    {
                        
                        var stacks = new ItemStack(this.Api.World.GetItem(assetLoc), 1);
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
            while (this.SailLength-- > 0)
            {
                var assetLoc = new AssetLocation("millwright:" + "sailassembly-" + this.bladeType + "-" + this.SailType);
                var stacks = new ItemStack(this.Api.World.GetItem(assetLoc), 1);
                this.Api.World.SpawnItemEntity(stacks, this.Blockentity.Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            base.OnBlockBroken(byPlayer);
        }


        internal bool OnInteract(IPlayer byPlayer)
        {

            var slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Empty)
            {
                return false;
            }

            if (this.SailLength >= 8)
            { return false; }

            var sail = slot.Itemstack.Collectible.Code.Path;
            var sailtype = slot.Itemstack.Collectible.LastCodePart();
            if (!sail.StartsWith("sailassembly") || slot.Itemstack.Collectible.Code.Domain != "millwright")
            { return false; }

            var sailcount = slot.Itemstack.Collectible.FirstCodePart(1);
            if (this.bladeType != sailcount)
            {
                return false;
            }


            var assetLoc = new AssetLocation("millwright:" + sail);
            var sailStack = new ItemStack(this.Api.World.GetItem(assetLoc));
            if (!slot.Itemstack.Equals(this.Api.World, sailStack, GlobalConstants.IgnoredStackAttributes))
            {
                return false;
            }
             

            var len = this.SailLength + 1; 

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
                slot.TakeOut(1);
                slot.MarkDirty();
            }
            this.SailLength++;
            this.SailType = sailtype;

            this.updateShape(this.Api.World);

            this.Blockentity.MarkDirty(true);
            return true;
        }

        private bool Obstructed(int len)
        {
            var tmpPos = new BlockPos(0, 0, 0, 0);

            for (var dx = -2; dx <= 2; dx++)
            {
                for (var dz = -2; dz <= 2; dz++)
                {
                    for (var dy = 0; dy <= len; dy++)
                    {
                        // Debug.WriteLine("dx: " + dx + " dy: " + dy + " dz: " + dz);
                        
                        // ignore the rotor itself
                        if (dx == 0 && dy == 0 && dz == 0)
                        {
                            continue;
                        }

                        //ignore the four corners 
                        if (Math.Abs(dx) == 2 && Math.Abs(dz) == 2)
                        {
                             continue;
                        }

                        tmpPos.Set(this.Position.X + dx, this.Position.Y + dy, this.Position.Z + dz);
                        var block = this.Api.World.BlockAccessor.GetBlock(tmpPos);
                        var collBoxes = block.GetCollisionBoxes(this.Api.World.BlockAccessor, tmpPos);
                        if (collBoxes != null && collBoxes.Length > 0 && !(block is BlockSnowLayer) && !(block is BlockSnow))
                        {

                            return true;
                        }
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

        public override float AngleRad
        {
            get
            {
                if (this.network == null)
                {
                    return this.lastKnownAngleRad;
                }
                if (this.isRotationReversed())
                {
                    return this.lastKnownAngleRad = 6.2831855f - this.network.AngleRad * this.GearedRatio * this.sailRotationModifier % 6.2831855f;
                }
                return this.lastKnownAngleRad = this.network.AngleRad * this.GearedRatio * this.sailRotationModifier % 6.2831855f;
            }
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
                    Base = new AssetLocation("millwright:block/wood/mechanics/ud/" + this.bladeType + "/windmillrotorud"),
                    rotateY = this.Block.Shape.rotateY
                };
            }
            else
            {
                try
                {
                    this.Shape = new CompositeShape()
                    {
                        Base = new AssetLocation("millwright:block/wood/mechanics/ud/" + this.bladeType + "/" + this.SailType + "/windmillud-" + this.SailLength + "blade"),
                        rotateY = this.Block.Shape.rotateY
                    };
                }
                catch
                {
                    //shouldn't need the try catch as long as all the assets exist
                }
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
