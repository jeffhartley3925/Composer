using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Events;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace Composer.Modules.Palettes.ViewModels
{
    public class PaletteButtonViewModel : BaseViewModel, IPaletteButtonViewModel
    {
        private string _target;
        public string Target
        {
            get { return _target; }
            set
            {
                _target = value;
                OnPropertyChanged(() => Target);
            }
        }

        private string _foreground = Preferences.PaletteButtonForeground;
        public string Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                OnPropertyChanged(() => Foreground);
            }

        }
        private string _background = Preferences.PaletteButtonBackground;
        public string Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged(() => Background);
            }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged(() => IsChecked);
            }
        }

        private int _strokeWidth = 1;
        public int StrokeWidth
        {
            get { return _strokeWidth; }
            set
            {
                _strokeWidth = value;
                OnPropertyChanged(() => StrokeWidth);
            }
        }

        private string _tooltip;
        public string Tooltip
        {
            get { return _tooltip; }
            set
            {
                if (value != _tooltip)
                {
                    _tooltip = value;
                    OnPropertyChanged(() => Tooltip);
                }
            }
        }

        private string _paletteId;
        public string PaletteId
        {
            get { return _paletteId; }
            set
            {
                if (value != _paletteId)
                {
                    _paletteId = value;
                    OnPropertyChanged(() => PaletteId);
                }
            }
        }

        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set
            {
                if (value != _caption)
                {
                    _caption = value;
                    OnPropertyChanged(() => Caption);
                }
            }
        }

        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set
            {
                if (value != _groupName)
                {
                    _groupName = value;
                    OnPropertyChanged(() => GroupName);
                }
            }
        }

        private bool _enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    OnPropertyChanged(() => Enabled);
                }
            }
        }

        public PaletteButtonViewModel(string enabled, string target, string groupName, string caption, string tooltip, string paletteId)
        {
            Enabled = (enabled == "") || bool.Parse(enabled);
            Target = target;
            GroupName = groupName;
            Caption = caption;
            Tooltip = tooltip;
            PaletteId = paletteId;
            PaletteCache.PaletteButtonViewModels.Add(this);
            DefineCommands();
            SubscribeEvents();
        }

        public void DefineCommands()
        {
            PaletteButtonClickedCommand = new DelegateCommand<IPaletteButtonViewModel>(OnPaletteButtonClickedCommand, OnCanExecuteCommand);
        }

        public void SubscribeEvents()
        {
        }

        public DelegateCommand<IPaletteButtonViewModel> PaletteButtonClickedCommand { get; private set; }

        public bool OnCanExecuteCommand(IPaletteButtonViewModel obj)
        {
            return true;
        }

        public void OnPaletteButtonClickedCommand(IPaletteButtonViewModel obj)
        {
            EA.GetEvent<HideEditPopup>().Publish(string.Empty);
            EA.GetEvent<DeSelectAll>().Publish(string.Empty);
            if (Enabled)
            {
                switch (PaletteId)
                {
                    case Palette.DurationPaletteId:
                        //EditorState.SetTool(null);
                        EA.GetEvent<DurationPaletteClicked>().Publish(Target);
                        break;
                    case Palette.PlaybackPaletteId:
                        EditorState.SetTool(null);
                        EA.GetEvent<PlayComposition>().Publish(_Enum.PlaybackInitiatedFrom.Palette);
                        break;
                    case Palette.ToolPaletteId:
                        EA.GetEvent<ToolPaletteClicked>().Publish(Target);
                        break;
                }
                EA.GetEvent<EditorStateChanged>().Publish(this);
            }
            else
            {
                IsChecked = false;
            }
        }
    }
}