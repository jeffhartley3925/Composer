namespace Composer.Modules.Composition.ViewModels
{
    public sealed class ObjectDensity
    {
        public int DensityId { get; set; }

        public int Density { get; set; }

        public ObjectDensity(int density)
        {
            DensityId = density;
            Density = density;
        }
    }
}
