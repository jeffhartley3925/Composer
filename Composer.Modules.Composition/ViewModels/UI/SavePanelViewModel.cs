using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Composer.Infrastructure;
using Microsoft.Practices.Composite.Presentation.Commands;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class SavePanelViewModel : BaseViewModel, ISavePanelViewModel
    {
        public SavePanelViewModel()
        {
            SubscribeEvents();
            DefineCommands();
            CanExecuteSave = true;
        }

        private void SubscribeEvents()
        {
        }

        private void DefineCommands()
        {
            SaveButtonClickedCommand = new DelegateCommand<object>(OnSave, OnCanExecuteSave);
        }

        public DelegateCommand<object> SaveButtonClickedCommand { get; private set; }

        private bool _canExecuteSave;
        public bool CanExecuteSave
        {
            get { return _canExecuteSave; }
            set
            {
                _canExecuteSave = value;
                SaveButtonClickedCommand.RaiseCanExecuteChanged();
            }
        }

        public bool OnCanExecuteSave(object obj)
        {
            return CanExecuteSave;
        }

        public void OnSave(object obj)
        {
            EA.GetEvent<Save>().Publish(string.Empty);
            EA.GetEvent<CreateAndUploadImage>().Publish(string.Empty);

            EA.GetEvent<HideSavePanel>().Publish(string.Empty);

            EditorState.Dirty = false;
            Tuple<string, string, string> payload = new Tuple<string, string, string>("Save","#3b5998","#FFFFFF");
            EA.GetEvent<UpdateSaveButtonHyperlink>().Publish(payload);
        }
    }
}
