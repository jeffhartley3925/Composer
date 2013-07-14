using System.Collections.Generic;

namespace Composer.Modules.Palettes.Services
{
    public class BasePaletteService
    {
        public List<PaletteItem> PaletteItems;
        private string _paletteCaption;
        public string PaletteCaption 
        {
            get
            {
                return _paletteCaption;
            }
            set
            {
                _paletteCaption = value;
                GetPaletteItems();
            }
        }
        public List<PaletteItem> GetPaletteItems()
        {
            PaletteItems = new PaletteItems(PaletteCaption).Parse();
            return PaletteItems;
        }
    }
}
