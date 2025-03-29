namespace Millwright.ModSystem
{
    using System;
    using System.Text;
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.Datastructures;
    using Vintagestory.GameContent.Mechanics;

    internal class BEBehaviorWindmillRotorUD : BEBehaviorMPRotor
    {
        double windSpeed;
        protected override float Resistance => 0.0003f;
        protected override float TargetSpeed => (float)Math.Min(0.6f, this.windSpeed);

        protected override double AccelerationFactor => 0.05d + 2.0f / 4;
        
        protected override float TorqueFactor => 10f / 4f; 


        public BEBehaviorWindmillRotorUD(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            this.Blockentity.RegisterGameTickListener(this.CheckAnimalSpeed, 1000);
            AxisSign = new int[] { 0, 1, 0 };
        }

        private void CheckAnimalSpeed(float dt)
        {
            this.windSpeed = 1.0f; // this.Api.World.Rand.NextDouble() + 0.2;
            //this.network.updateNetwork(this.manager.getTickNumber());
        }

        internal bool OnInteract(IPlayer byPlayer)
        {
            this.updateShape(this.Api.World);

            this.Blockentity.MarkDirty(true);
            return true;
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
                    return this.lastKnownAngleRad = 6.2831855f - this.network.AngleRad * this.GearedRatio * 2.0f % 6.2831855f;
                }
                return this.lastKnownAngleRad = this.network.AngleRad * this.GearedRatio * 2.0f % 6.2831855f;
            }
        }

        protected override void updateShape(IWorldAccessor worldForResolve)
        {
            if (worldForResolve.Side != EnumAppSide.Client || this.Block == null)
            {
                return;
            }
            this.Shape = new CompositeShape()
            {
                Base = new AssetLocation("millwright:block/wood/mechanics/three/windmillrotorud"),
                //rotateX = this.Block.Shape.rotateX
            };
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            base.GetBlockInfo(forPlayer, sb);

            sb.AppendLine(string.Format(Lang.Get("Wind speed: {0}%", (int)(100 * this.windSpeed))));
            sb.AppendLine(Lang.Get("Sails power output: {0} kN", (int)(100f)));
        }
    }
}
