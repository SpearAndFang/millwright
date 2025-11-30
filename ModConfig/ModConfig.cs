namespace Millwright.ModConfig
{
    public class ModConfig
    {
        public static ModConfig Loaded { get; set; } = new ModConfig();

        public bool AllowSailDeconstruction { get; set; } = true;
        public bool UseIncreasedWindSpeed { get; set; } = true;
        public double BrakeResistanceModifier { get; set; } = 2.0;
        public double SailCenteredModifier { get; set; } = 2.0;
        public double SailAngledModifier { get; set; } = 2.0;
        public double SailWideModifier { get; set; } = 2.0;
        public double SailRotationModifier { get; set; } = 1.0;
    }
}
