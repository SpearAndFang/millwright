namespace Millwright.ModSystem
{
    using Vintagestory.API.Common;
    using Millwright.ModConfig;

    public class MillwrightSystem : ModSystem
    {
        public void RegisterClasses(ICoreAPI api)
        {
            api.RegisterBlockClass("BlockWindmillRotorEnhanced", typeof(BlockWindmillRotorEnhanced));
            api.RegisterBlockEntityBehaviorClass("BEBehaviorWindmillRotorEnhanced", typeof(BEBehaviorWindmillRotorEnhanced));

            //horizontal windmill demo only
            //api.RegisterBlockClass("BlockWindmillRotorUD", typeof(BlockWindmillRotorUD));
            //api.RegisterBlockEntityBehaviorClass("BEBehaviorWindmillRotorUD", typeof(BEBehaviorWindmillRotorUD));

            api.RegisterBlockClass("BlockBrakeEnhanced", typeof(BlockBrakeEnhanced));
            api.RegisterBlockEntityClass("BEBrakeEnhanced", typeof(BEBrakeEnhanced));
            api.RegisterBlockEntityBehaviorClass("BEBehaviorBrakeEnhanced", typeof(BEBehaviorBrakeEnhanced));

            api.RegisterBlockClass("BlockAxlePassthrough", typeof(BlockAxlePassthrough));
            api.RegisterBlockClass("BlockAxlePassthroughfull", typeof(BlockAxlePassthroughFull));
            api.RegisterBlockEntityClass("BEAxlePassThrough", typeof(BEAxlePassThrough));
            api.RegisterBlockEntityBehaviorClass("BEBehaviorAxlePassthrough", typeof(BEBehaviorAxlePassthrough));

            // colored sails attempt 1
            //api.RegisterItemClass("ItemSailCustom", typeof(ItemSailCustom));
        }


        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.World.Logger.Event("started 'Millwright' mod");
            this.RegisterClasses(api);
        }

        public override void StartPre(ICoreAPI api)
        {
            var cfgFileName = "millwright.json";
            try
            {
                ModConfig fromDisk;
                if ((fromDisk = api.LoadModConfig<ModConfig>(cfgFileName)) == null)
                { api.StoreModConfig(ModConfig.Loaded, cfgFileName); }
                else
                { ModConfig.Loaded = fromDisk; }
            }
            catch
            { api.StoreModConfig(ModConfig.Loaded, cfgFileName); }

            api.World.Config.SetBool("AllowSailDeconstruction", ModConfig.Loaded.AllowSailDeconstruction);

            base.StartPre(api);
        }
    }
}
