namespace Composer.Infrastructure.Dimensions
{
    public class DimensionBase
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string Vector { get; set; }
        public string Formatter { get; set; }
    }
}
