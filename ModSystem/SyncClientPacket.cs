using ProtoBuf;

namespace millwright.ModSystem
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SyncClientPacket
    {
        public bool AllowSailDeconstruction;
        public double BrakeResistanceModifier;
        public double SailCenteredModifier;
        public double SailAngledModifier;
        public double SailWideModifier;
        public double SailRotationModifier;
    }
}