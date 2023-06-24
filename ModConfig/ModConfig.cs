namespace Millwright.ModConfig
{
    public class ModConfig
    {
        public static ModConfig Loaded { get; set; } = new ModConfig();
        public double SailCenteredModifier { get; set; } = 2.0;
    }
}
