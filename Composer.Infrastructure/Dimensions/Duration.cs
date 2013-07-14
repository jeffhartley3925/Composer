namespace Composer.Infrastructure
{
    public class Duration
    {
        public string Caption { get; set; }
        public double Value { get; set; }
        public int Spacing { get; set; }
        public bool? IsDotted { get; set; }
        public Duration(string caption, double value, int spacing, bool? isDotted)
        {
            Caption = caption;
            Spacing = spacing;
            Value = value;
            IsDotted = isDotted;
        }
    }
}
