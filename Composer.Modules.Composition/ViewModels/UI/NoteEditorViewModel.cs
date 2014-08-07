using Composer.Infrastructure;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class NoteEditorViewModel : BaseViewModel, INoteEditorViewModel
    {
        public NoteEditorViewModel()
        {
            DefineCommands();
            SubscribeEvents();
        }

        public void DefineCommands()
        {
            NaturalButtonCommand = new DelegatedCommand<object>(OnNaturalButtonCommand);
            SharpButtonCommand = new DelegatedCommand<object>(OnSharpButtonCommand);
            FlatButtonCommand = new DelegatedCommand<object>(OnFlatButtonCommand);
            ReverseButtonCommand = new DelegatedCommand<object>(OnReverseButtonCommand);
            PlayButtonCommand = new DelegatedCommand<object>(OnPlayButtonCommand);
            CloseButtonCommand = new DelegatedCommand<object>(OnCloseButtonCommand);
            DeleteButtonCommand = new DelegatedCommand<object>(OnDeleteButtonCommand);
            UpIntervalButtonCommand = new DelegatedCommand<object>(OnUpIntervalButtonCommand);
            DownIntervalButtonCommand = new DelegatedCommand<object>(OnDownIntervalButtonCommand);
        }

        private DelegatedCommand<object> _closeButtonCommand;

        public DelegatedCommand<object> CloseButtonCommand
        {
            get { return _closeButtonCommand; }
            set
            {
                _closeButtonCommand = value;
                OnPropertyChanged(() => CloseButtonCommand);
            }
        }

        public void OnCloseButtonCommand(object obj)
        {
            EA.GetEvent<HideNoteEditor>().Publish(string.Empty);
        }

        private DelegatedCommand<object> _playButtonCommand;

        public DelegatedCommand<object> PlayButtonCommand
        {
            get { return _playButtonCommand; }
            set
            {
                _playButtonCommand = value;
                OnPropertyChanged(() => PlayButtonCommand);
            }
        }

        public void OnPlayButtonCommand(object obj)
        {

        }

        private bool _upIntervalButtonEnabled;

        public bool UpIntervalButtonEnabled
        {
            get { return _upIntervalButtonEnabled; }
            set
            {
                _upIntervalButtonEnabled = value;
                OnPropertyChanged(() => UpIntervalButtonEnabled);
            }
        }

        private DelegatedCommand<object> _upIntervalButtonCommand;

        public DelegatedCommand<object> UpIntervalButtonCommand
        {
            get { return _upIntervalButtonCommand; }
            set
            {
                _upIntervalButtonCommand = value;
                OnPropertyChanged(() => UpIntervalButtonCommand);
            }
        }

        public void OnUpIntervalButtonCommand(object obj)
        {

        }

        private bool _downIntervalButtonEnabled;

        public bool DownIntervalButtonEnabled
        {
            get { return _downIntervalButtonEnabled; }
            set
            {
                _downIntervalButtonEnabled = value;
                OnPropertyChanged(() => DownIntervalButtonEnabled);
            }
        }

        private DelegatedCommand<object> _downIntervalButtonCommand;

        public DelegatedCommand<object> DownIntervalButtonCommand
        {
            get { return _downIntervalButtonCommand; }
            set
            {
                _downIntervalButtonCommand = value;
                OnPropertyChanged(() => DownIntervalButtonCommand);
            }
        }

        public void OnDownIntervalButtonCommand(object obj)
        {

        }

        private bool _deleteButtonEnabled;

        public bool DeleteButtonEnabled
        {
            get { return _deleteButtonEnabled; }
            set
            {
                _deleteButtonEnabled = value;
                OnPropertyChanged(() => DeleteButtonEnabled);
            }
        }

        private DelegatedCommand<object> _deleteButtonCommand;

        public DelegatedCommand<object> DeleteButtonCommand
        {
            get { return _deleteButtonCommand; }
            set
            {
                _deleteButtonCommand = value;
                OnPropertyChanged(() => DeleteButtonCommand);
            }
        }

        public void OnDeleteButtonCommand(object obj)
        {
            if (Infrastructure.Support.Selection.Exists())
            {
                Infrastructure.Support.Selection.Delete();
            }
        }

        private bool _reverseButtonEnabled;

        public bool ReverseButtonEnabled
        {
            get { return _reverseButtonEnabled; }
            set
            {
                _reverseButtonEnabled = value;
                OnPropertyChanged(() => ReverseButtonEnabled);
            }
        }

        private DelegatedCommand<object> _reverseButtonCommand;

        public DelegatedCommand<object> ReverseButtonCommand
        {
            get { return _reverseButtonCommand; }
            set
            {
                _reverseButtonCommand = value;
                OnPropertyChanged(() => ReverseButtonCommand);
            }
        }

        public void OnReverseButtonCommand(object obj)
        {
            Infrastructure.Support.Selection.Reverse();
        }

        private bool _naturalButtonEnabled;

        public bool NaturalButtonEnabled
        {
            get { return _naturalButtonEnabled; }
            set
            {
                _naturalButtonEnabled = value;
                OnPropertyChanged(() => NaturalButtonEnabled);
            }

        }
        private bool _sharpButtonEnabled;

        public bool SharpButtonEnabled
        {
            get { return _sharpButtonEnabled; }
            set
            {
                _sharpButtonEnabled = value;
                OnPropertyChanged(() => SharpButtonEnabled);
            }
        }

        private bool _flatButtonEnabled;

        public bool FlatButtonEnabled
        {
            get { return _flatButtonEnabled; }
            set
            {
                _flatButtonEnabled = value;
                OnPropertyChanged(() => FlatButtonEnabled);
            }
        }

        private DelegatedCommand<object> _flatButtonCommand;

        public DelegatedCommand<object> FlatButtonCommand
        {
            get { return _flatButtonCommand; }
            set
            {
                _flatButtonCommand = value;
                OnPropertyChanged(() => FlatButtonCommand);
            }
        }

        public void OnFlatButtonCommand(object obj)
        {
            Infrastructure.Support.Selection.SetAccidental(_Enum.Accidental.Flat);
        }

        private DelegatedCommand<object> _sharpButtonCommand;

        public DelegatedCommand<object> SharpButtonCommand
        {
            get { return _sharpButtonCommand; }
            set
            {
                _sharpButtonCommand = value;
                OnPropertyChanged(() => SharpButtonCommand);
            }
        }

        public void OnSharpButtonCommand(object obj)
        {
            Infrastructure.Support.Selection.SetAccidental(_Enum.Accidental.Sharp);
        }

        private DelegatedCommand<object> _naturalButtonCommand;

        public DelegatedCommand<object> NaturalButtonCommand
        {
            get { return _naturalButtonCommand; }
            set
            {
                if (value != _naturalButtonCommand)
                {
                    _naturalButtonCommand = value;
                    OnPropertyChanged(() => NaturalButtonCommand);
                }
            }
        }

        public void OnNaturalButtonCommand(object obj)
        {
            Infrastructure.Support.Selection.SetAccidental(_Enum.Accidental.Natural);
        }

        public void SubscribeEvents()
        {
        }
    }
}
