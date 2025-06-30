namespace Millwright.ModSystem
{
    using millwright.ModSystem;
    using Millwright.ModConfig;
    using ProtoBuf;
    using System.Collections.Generic;
    using Vintagestory.API.Client;
    using Vintagestory.API.Common;
    using Vintagestory.API.MathTools;
    using Vintagestory.API.Server;
    using Vintagestory.API.Util;

    public class MillwrightSystem : ModSystem
    {

        private IServerNetworkChannel serverChannel;
        private ICoreAPI api;

        public void RegisterClasses(ICoreAPI api)
        {
            api.RegisterBlockClass("BlockWindmillRotorEnhanced", typeof(BlockWindmillRotorEnhanced));
            api.RegisterBlockEntityBehaviorClass("BEBehaviorWindmillRotorEnhanced", typeof(BEBehaviorWindmillRotorEnhanced));

            //horizontal windmill
            api.RegisterBlockClass("BlockWindmillRotorUD", typeof(BlockWindmillRotorUD));
            api.RegisterBlockEntityBehaviorClass("BEBehaviorWindmillRotorUD", typeof(BEBehaviorWindmillRotorUD));

            api.RegisterBlockClass("BlockBrakeEnhanced", typeof(BlockBrakeEnhanced));
            api.RegisterBlockEntityClass("BEBrakeEnhanced", typeof(BEBrakeEnhanced));
            api.RegisterBlockEntityBehaviorClass("BEBehaviorBrakeEnhanced", typeof(BEBehaviorBrakeEnhanced));

            api.RegisterBlockClass("BlockAxlePassthrough", typeof(BlockAxlePassthrough));
            api.RegisterBlockClass("BlockAxlePassthroughfull", typeof(BlockAxlePassthroughFull));
            api.RegisterBlockEntityClass("BEAxlePassThrough", typeof(BEAxlePassThrough));
            api.RegisterBlockEntityBehaviorClass("BEBehaviorAxlePassthrough", typeof(BEBehaviorAxlePassthrough));

            api.RegisterBlockClass("ImprovedBlockAxlePassthrough", typeof(ImprovedBlockAxlePassthrough));
            api.RegisterBlockClass("ImprovedBlockAxlePassthroughfull", typeof(ImprovedBlockAxlePassthroughFull));
            api.RegisterBlockEntityClass("ImprovedBEAxlePassThrough", typeof(ImprovedBEAxlePassThrough));
            api.RegisterBlockEntityBehaviorClass("BEBehaviorImprovedAxlePassthrough", typeof(BEBehaviorImprovedAxlePassthrough));
        }


        public override void Start(ICoreAPI api)
        {
            this.api = api;
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


        public override void StartClientSide(ICoreClientAPI capi)
        {
            capi.Network.RegisterChannel("millwright")
                .RegisterMessageType<SyncClientPacket>()
                .SetMessageHandler<SyncClientPacket>(packet =>
                {

                    ModConfig.Loaded.AllowSailDeconstruction = packet.AllowSailDeconstruction;
                    this.Mod.Logger.Event($"Received AllowSailDeconstruction of {packet.AllowSailDeconstruction} from server");

                    ModConfig.Loaded.BrakeResistanceModifier = packet.BrakeResistanceModifier;
                    this.Mod.Logger.Event($"Received BrakeResistanceModifier of {packet.BrakeResistanceModifier} from server");

                    ModConfig.Loaded.SailCenteredModifier = packet.SailCenteredModifier;
                    this.Mod.Logger.Event($"Received SailCenteredModifier of {packet.SailCenteredModifier} from server");

                    ModConfig.Loaded.SailAngledModifier = packet.SailAngledModifier;
                    this.Mod.Logger.Event($"Received SailAngledModifier of {packet.SailAngledModifier} from server");

                    ModConfig.Loaded.SailWideModifier = packet.SailWideModifier;
                    this.Mod.Logger.Event($"Received SailWideModifier of {packet.SailWideModifier} from server");

                    ModConfig.Loaded.SailRotationModifier = packet.SailRotationModifier;
                    this.Mod.Logger.Event($"Received SailRotationModifier of {packet.SailRotationModifier} from server");

                });
        }


        public override void StartServerSide(ICoreServerAPI sapi)
        {
            // send connecting players the config settings
            sapi.Event.PlayerJoin += this.OnPlayerJoin; // add method so we can remove it in dispose to prevent memory leaks
            // register network channel to send data to clients
            this.serverChannel = sapi.Network.RegisterChannel("millwright")
                .RegisterMessageType<SyncClientPacket>()
                .SetMessageHandler<SyncClientPacket>((player, packet) => { /* do nothing. idk why this handler is even needed, but it is */ });
        }


        private void OnPlayerJoin(IServerPlayer player)
        {
            // send the connecting player the settings it needs to be synced
            this.serverChannel.SendPacket(new SyncClientPacket
            {
                AllowSailDeconstruction = ModConfig.Loaded.AllowSailDeconstruction,
                BrakeResistanceModifier = ModConfig.Loaded.BrakeResistanceModifier,
                SailCenteredModifier = ModConfig.Loaded.SailCenteredModifier,
                SailAngledModifier = ModConfig.Loaded.SailAngledModifier,
                SailWideModifier = ModConfig.Loaded.SailWideModifier,
                SailRotationModifier = ModConfig.Loaded.SailRotationModifier
            }, player);
        }

        public override void Dispose()
        {
            // remove our player join listener so we dont create memory leaks
            if (this.api is ICoreServerAPI sapi)
            {
                sapi.Event.PlayerJoin -= this.OnPlayerJoin;
            }
        }
    }
}
