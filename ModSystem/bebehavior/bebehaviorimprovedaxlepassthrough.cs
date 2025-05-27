namespace Millwright.ModSystem
{
    //using System.Diagnostics;
    using System.Text;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.Config;
    using Vintagestory.API.Datastructures;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Util;
    using Vintagestory.GameContent.Mechanics;
    using Newtonsoft;
    

    public class BEBehaviorImprovedAxlePassthrough : BEBehaviorMPBase // BEBehaviorMPAxle
    {
        private Vec3f center = new Vec3f(0.5f, 0.5f, 0.5f);
        BlockFacing[] orients = new BlockFacing[2];
        ICoreClientAPI capi;
        string orientations;
        AssetLocation axlePlate;


        public BEBehaviorImprovedAxlePassthrough(BlockEntity ImprovedBEAxlePassThrough) : base(ImprovedBEAxlePassThrough)
        { }
        protected virtual bool AddPlate => true;

        public override float GetResistance()
        { return 0.0005f; }


        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            this.axlePlate = AssetLocation.Create("block/wood/mechanics/axle-improved-plate", this.Block.Code?.Domain);
            if (this.Block.Attributes?["axlePlate"].Exists == true)
            {
                this.axlePlate = this.Block.Attributes["axlePlate"].AsObject<AssetLocation>();
            }
            this.axlePlate.WithPathAppendixOnce(".json").WithPathPrefixOnce("shapes/");
            if (api.Side == EnumAppSide.Client)
            {
                this.capi = api as ICoreClientAPI;
            }
            this.orientations = this.Block.Variant["rotation"];
            switch (this.orientations)
            {
                case "ns":
                    this.AxisSign = new int[] { 0, 0, -1 };
                    this.orients[0] = BlockFacing.NORTH;
                    this.orients[1] = BlockFacing.SOUTH;
                    break;

                case "we":
                    this.AxisSign = new int[] { -1, 0, 0 };
                    this.orients[0] = BlockFacing.WEST;
                    this.orients[1] = BlockFacing.EAST;
                    break;

                default: //du, ud
                    this.AxisSign = new int[] { 0, 1, 0 };
                    this.orients[0] = BlockFacing.DOWN;
                    this.orients[1] = BlockFacing.UP;
                    break;
            }
        }


        protected virtual MeshData GetPlateMesh()
        {
            return ObjectCacheUtil.GetOrCreate(this.Api, this.Block.Code + "-plate", () =>
            {
                var shape = this.Api.Assets.TryGet(this.axlePlate).ToObject<Shape>();
                this.capi.Tesselator.TesselateShape(this.Block, shape, out var mesh);
                return mesh;
            });
        }



        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (this.AddPlate)
            {
                var mesh = this.GetPlateMesh();
                mesh = this.RotateMesh(mesh);
                if (mesh != null)
                { mesher.AddMeshData(mesh); }
            }
            return base.OnTesselation(mesher, tesselator);
        }


        private MeshData RotateMesh(MeshData mesh)
        {
            mesh = mesh.Clone();
            if (this.orientations == "ud")
            { mesh = mesh.Rotate(this.center, 0, 0, -GameMath.PIHALF); }
            if (this.orientations == "we")
            { mesh = mesh.Rotate(this.center, -GameMath.PI, 0, 0); }
            if (this.orientations == "ns")
            { mesh = mesh.Rotate(this.center, 0, -GameMath.PIHALF, 0); }
            return mesh;
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            base.GetBlockInfo(forPlayer, sb);
            if (this.Api.World.EntityDebugMode)
            {
                var rotation = this.Block.Variant["rotation"];
                sb.AppendLine(string.Format(Lang.Get("Rotation: {0}", rotation)));
            }
        }
    }
}
