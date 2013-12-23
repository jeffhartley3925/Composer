
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using System.Collections.Generic;
using Composer.Infrastructure.Behavior;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class CollaborationPanelViewModel : BaseViewModel, ICollaborationPanelViewModel
    {
        private ListBox listBox;

        private List<Collaborator> _collaborators;
        public List<Collaborator> Collaborators
        {
            get { return _collaborators; }
            set
            {
                _collaborators = value;
                OnPropertyChanged(() => Collaborators);
            }
        }

        public CollaborationPanelViewModel()
        {
            DefineCommands();
            SubscribeEvents();
            if (Collaborators == null)
            {
                EA.GetEvent<UpdateCollaborators>().Publish(Collaborations.Collaborators);
            }
            CanExecuteSave = false;
        }

        private void SubscribeEvents()
        {
            EA.GetEvent<UpdateCollaborators>().Subscribe(OnCollaborationsUpdate);
            EA.GetEvent<UpdateCollaborationPanelSaveButtonEnableState>().Subscribe(OnUpdateCollaborationPanelSaveButtonEnabled);
        }

        public void OnUpdateCollaborationPanelSaveButtonEnabled(bool b)
        {
            CanExecuteSave = b;
        }

        public void OnCollaborationsUpdate(object obj)
        {
            var collaborators = (List<Collaborator>)obj;
            Collaborators = EditorState.IsAuthor ?
                (from b in collaborators where b.AuthorId != Current.User.Id select b).ToList() :
                (from b in collaborators where b.Index == 0 select b).ToList();
        }

        #region Close Button Support

        private bool _canExecuteClose;
        public bool CanExecuteClose
        {
            get { return _canExecuteClose; }
            set
            {
                _canExecuteClose = value;
                CloseButtonClickedCommand.RaiseCanExecuteChanged();
            }
        }
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
        public bool OnCanExecuteClose(object obj)
        {
            return CanExecuteClose;
        }
        public DelegateCommand<object> CloseButtonClickedCommand { get; private set; }

        public void OnClose(object obj)
        {
            if (listBox != null)
            {
                listBox.SelectedItem = null;
            }

            EA.GetEvent<HideCollaborationPanel>().Publish(string.Empty);
            EA.GetEvent<UpdateCollaboratorName>().Publish(string.Empty);
            EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Editing);
            EA.GetEvent<HideDispositionButtons>().Publish(string.Empty);
            Collaborations.CurrentCollaborator = null;
        }

        public bool OnCanExecuteSave(object obj)
        {
            return CanExecuteSave;
        }

        public DelegateCommand<object> SaveButtonClickedCommand { get; private set; }

        public void OnSave(object obj)
        {
            if (Collaborations.DispositionChanges != null)
            {
                for (var i = Collaborations.DispositionChanges.Count() - 1; i >= 0; i--)
                {
                    DispositionChangeItem item = Collaborations.DispositionChanges[i];
                    switch (item.Disposition)
                    {
                        case _Enum.Disposition.Accept:
                            EA.GetEvent<AcceptChange>().Publish(item.SourceId);
                            break;
                        case _Enum.Disposition.Reject:
                            EA.GetEvent<RejectChange>().Publish(item.SourceId);
                            break;
                    }
                    Collaborations.DispositionChanges.Remove(item);
                }
                EA.GetEvent<UpdateProvenancePanel>().Publish(CompositionManager.Composition);
                var c = from d in Collaborations.DispositionChanges where d.Disposition != _Enum.Disposition.Na select d;
                EA.GetEvent<UpdateCollaborationPanelSaveButtonEnableState>().Publish(c.Any());
            }
        }
        #endregion

        #region Clear Button Support

        private bool _canExecuteClear;
        public bool CanExecuteClear
        {
            get { return _canExecuteClear; }
            set
            {
                _canExecuteClear = value;
                ClearButtonClickedCommand.RaiseCanExecuteChanged();
            }
        }
        public bool OnCanExecuteClear(object obj)
        {
            return CanExecuteClear;
        }
        public DelegateCommand<object> ClearButtonClickedCommand { get; private set; }

        public void OnClear(object obj)
        {
            if (listBox != null)
            {
                listBox.SelectedItem = null; //causes SelectionChanged event. see OnSelectionChanged
            }
            EA.GetEvent<UpdateCollaboratorName>().Publish(string.Empty);
            EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Editing);
            EA.GetEvent<HideDispositionButtons>().Publish(string.Empty);
        }

        #endregion

        private void DefineCommands()
        {
            SaveButtonClickedCommand = new DelegateCommand<object>(OnSave, OnCanExecuteSave);
            CloseButtonClickedCommand = new DelegateCommand<object>(OnClose, OnCanExecuteClose);
            ClearButtonClickedCommand = new DelegateCommand<object>(OnClear, OnCanExecuteClear);
            SelectionChangedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnSelectionChanged, null);

            CanExecuteClear = false;
            CanExecuteClose = true;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _selectionChangedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> SelectionChangedCommand
        {
            get
            {
                return _selectionChangedCommand;
            }
            set
            {
                _selectionChangedCommand = value;
                OnPropertyChanged(() => SelectionChangedCommand);
            }
        }

        public void OnSelectionChanged(ExtendedCommandParameter param)
        {
            listBox = (ListBox)param.Sender;
            var collaborator = (Collaborator)(listBox.SelectedItem);
            EA.GetEvent<ResetNoteActivationState>().Publish(string.Empty);
            ResetCollaborationContext(collaborator);
            if (collaborator != null)
            {
                CanExecuteClear = true;
                var id = (collaborator.AuthorId == CompositionManager.Composition.Audit.Author_Id) ? Current.User.Id : collaborator.AuthorId;
                Collaborations.Index = (from b in Collaborations.CurrentCollaborations where b.CollaboratorId == id select b.Index).First();
                Collaborations.CurrentCollaborator = collaborator;
                EA.GetEvent<UpdateCollaboratorName>().Publish(string.Format("{0} {1}", collaborator.Name, string.Empty));
                foreach (var note in Cache.Notes)
                {
                    var status = Collaborations.GetStatus(note, Collaborations.Index);
                    bool updateNote = false;
                    if (EditorState.IsAuthor)
                    {
                        updateNote = ShowPendingCollaboratorContributions(note, collaborator, status);
                    }
                    else
                    {
                        updateNote = ShowPendingAuthorAdditions(status, note);
                    }
                    if (updateNote) EA.GetEvent<UpdateNote>().Publish(note);
                }
                EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Collaboration);
            }
            UpdateCompositionAfterCollaboratorChange();
        }

        private static bool ShowPendingAuthorAdditions(int? status, Note note)
        {
            // if we arrive here, the current user is a contributor.
            // There can be many contributors but only one author, so unlike 
            // above, we only need to check the status of the note instead of 
            // both status and ownership.
            var result = false;
            switch (status)
            {
                case (int)_Enum.Status.AuthorAdded:
                    // has the disposition of the note been resolved? if so, it's status will now
                    // be Enum.Status.AuthorAdded and the style of the note should reflect a pending addition.
                    //note.Foreground = Preferences.NoteForeground;
                    result = true;
                    break;
                case (int)_Enum.Status.AuthorDeleted:
                    // has the disposition of the note been resolved? if so, it's status will now
                    // be Enum.Status.AuthorDeleted and the style of the note should reflect a pending deletion.
                    //note.Foreground = Preferences.NoteForeground;
                    result = true;
                    break;
            }
            return result;
        }

        private bool ShowPendingCollaboratorContributions(Note note, Collaborator collaborator, int? status)
        {
            // there is one author but many contributors, so we must check if 
            // this note was created by the currently selected contributor.
            var result = false;
            if (note.Audit.Author_Id == collaborator.AuthorId)
            {
                // has the disposition of the note been resolved? if so, it's status will now
                // be Enum.Status.ContributorAdded and the style of the note should reflect a pending addition.
                if (status == (int)_Enum.Status.ContributorAdded)
                {
                    //note.Foreground = Preferences.AddedColor;
                    result = true;
                }
            }
            else
            {
                // has the disposition of the note been resolved? if so, it's status will now
                // be Enum.Status.ContributorDeleted and the style of the note should reflect a pending deletion.
                if (status == (int)_Enum.Status.ContributorDeleted)
                {
                    // is the creator of this note the currently selected collaborator ?
                    if (note.Audit.CollaboratorIndex == Collaborations.CurrentCollaborator.Index)
                    {
                        //note.Foreground = Preferences.DeletedColor;
                        result = true;
                    }
                }
            }
            return result;
        }

        private void ResetCollaborationContext(Collaborator collaborator)
        {
            if (collaborator == null || Collaborations.CurrentCollaborator != null)
            // if we click the clear button OR select a different collaborator
            {
                // here we are hiding the previously selected collaborator changes.
                var id = Collaborations.CurrentCollaborator.AuthorId;
                Collaborations.CurrentCollaborator = null;
                CanExecuteClear = false;
                foreach (
                    var note in
                        Cache.Notes.Where(
                            n => n.Audit.Author_Id == id || Collaborations.GetStatus(n) == (int)_Enum.Status.ContributorDeleted)
                    )
                {
                    EA.GetEvent<UpdateNote>().Publish(note);
                }
            }
        }

        private void UpdateCompositionAfterCollaboratorChange()
        {
            foreach (var m in Cache.Measures)
            {
                if (!m.Chords.Any()) continue;
                Debug.WriteLine("UpdateCompositionAfterCollaboratorChange");
                EA.GetEvent<MeasureLoaded>().Publish(m.Id);
                EA.GetEvent<UpdateSpanManager>().Publish(m.Id);
                EA.GetEvent<UpdateActiveChords>().Publish(m.Id);
                EA.GetEvent<UpdateSubverses>().Publish(string.Empty);
            }
            EA.GetEvent<UpdateArc>().Publish(string.Empty);
        }
    }
}