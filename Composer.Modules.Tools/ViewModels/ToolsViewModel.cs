using Composer.Infrastructure;
using Composer.Modules.Palettes.Services;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Palettes.ViewModels
{
    public class ToolsViewModel : BasePaletteViewModel, IToolsViewModel
    {
        private readonly ToolsService _service;
        public ToolsViewModel()
        {
            _service = (ToolsService)Unity.Container.Resolve(typeof(IToolsService), "ToolsService");
            Items = _service.PaletteItems;
            Caption = Items[0].PaletteCaption;
            Id = Items[0].PaletteId;
            Hide(new object());
            SubscribeEvents();
        }

        public void SubscribeEvents()
        {
            Ea.GetEvent<ToolPaletteClicked>().Unsubscribe(OnButtonClicked);
            Ea.GetEvent<ToolPaletteClicked>().Subscribe(OnButtonClicked, true);
        }

        public void OnButtonClicked(string target)
        {
            string[] paramaters = target.Split(',');
            string tool = paramaters[1];
            EditorState.SetTool((EditorState.Tool == tool) ? string.Empty : tool);
            EditorState.PaletteId = Id;
            switch (tool)
            {
                case "Erase":
                    break;
                case "Reverse":
                    break;
                case "Color": // >= 1
                    break;
                case "Clone":
                    break;
                case "Select":
                    break;
                case "AreaSelect":
                    break;
                case "Slur": // 2
                    break;
                case "Tie": // 2
                    break;
                case "Span": // >= 2
                    break;
                case "Text": // >= 1
                    break;
            }
        }
    }
}
