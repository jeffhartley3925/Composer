namespace Composer.Modules.Palettes
{
    public sealed class PaletteItem
    {
        public string Enabled { get; set; }
        public string Target { get; set; }
        public string GroupName { get; set; }
        public string Caption { get; set; }
        public string Tooltip { get; set; }
        public string PaletteId { get; set; }
        public string PaletteCaption { get; set; }

        public PaletteItem(string enabled, string target, string groupName, string caption, string tooltip, string paletteId, string paletteCaption)
        {
            Enabled = enabled;
            Target = target;
            GroupName = groupName;
            Caption = caption;
            Tooltip = tooltip;
            PaletteId = paletteId;
            PaletteCaption = paletteCaption;
        }
    }
}
