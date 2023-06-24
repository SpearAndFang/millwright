namespace Millwright.ModSystem
{
    using Vintagestory.API.Common;
    using Millwright.ModConfig;

    public class MillwrightSystem : ModSystem
    {
        public void RegisterClasses(ICoreAPI api)
        {
            api.RegisterBlockClass("BlockWindmillRotorSingle", typeof(BlockWindmillRotorSingle));
            api.RegisterBlockClass("BlockWindmillRotorDouble", typeof(BlockWindmillRotorDouble));
            api.RegisterBlockEntityBehaviorClass("MPWindmillRotorSingle", typeof(BEBehaviorWindmillRotorSingle));
            api.RegisterBlockEntityBehaviorClass("MPWindmillRotorDouble", typeof(BEBehaviorWindmillRotorDouble));
        }
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
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
            base.StartPre(api);
        }
    }
}
