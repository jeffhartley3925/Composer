
using System.Linq;
using System.Windows.Controls;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using System.Collections.Generic;
using Composer.Infrastructure.Behavior;
using Composer.Modules.Composition.ViewModels.Helpers;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class CollaborationPanelViewModel : BaseViewModel, ICollaborationPanelViewModel
    {
        private ListBox _listBox;

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
            if (_listBox != null)
            {
                _listBox.SelectedItem = null;
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
            if (_listBox != null)
            {
                _listBox.SelectedItem = null; //causes SelectionChanged event. see OnSelectionChanged
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
            EA.GetEvent<ResetNoteActivationState>().Publish(string.Empty);
            // the Clear button handler sets the SelectedIndex to null, throwing the SelectionChanged event, triggering this handler.
            _listBox = (ListBox)param.Sender;
            var collaborator = (Collaborator)_listBox.SelectedItem;
            if (collaborator == null || Collaborations.CurrentCollaborator != null) // if we click the clear button OR select a different col
            {
                // here we are hiding the previously selected collaborator changes.
                var colId = Collaborations.CurrentCollaborator.AuthorId;
                Collaborations.CurrentCollaborator = null;
                CanExecuteClear = false;

                foreach (var note in Cache.Notes.Where(note => note.Audit.Author_Id == colId ||
                                                               Collaborations.GetStatus(note) == (int)_Enum.Status.ContributorDeleted))
                {
                    EA.GetEvent<UpdateNote>().Publish(note);
                }

            }

            if (collaborator != null)
            {
                // TODO: replace all 'EA.GetEvent<UpdateNote>().Publish(n)' lines with a boolean that indicates 
                // the note needs to be updated, then publish the UpdateNote event once, at bottom of method if the boolean is true.

                // show the currently selected collaborator changes.
                CanExecuteClear = true;
                var id = (collaborator.AuthorId == CompositionManager.Composition.Audit.Author_Id) ? Current.User.Id : collaborator.AuthorId;
                var index = (from b in Collaborations.CurrentCollaborations where b.CollaboratorId == id select b.Index).First();

                Collaborations.Index = index;
                Collaborations.CurrentCollaborator = collaborator;
                EA.GetEvent<UpdateCollaboratorName>().Publish(string.Format("{0} {1}", collaborator.Name, string.Empty));

                foreach (var note in Cache.Notes)
                {
                    var status = Collaborations.GetStatus(note, index);

                    // TODO: is "EditorState.IsAuthor" equivalent to "EditContext == _Enum.EditContext.authoring". if so refactor 
                    // "EditorState.IsAuthor" to "if (EditContext == _Enum.EditContext.authoring)"
                    if (EditorState.IsAuthor) //is the logged on user the composition author?
                    {
                        // there is one author but many contributors, so we must check if 
                        // this n was created by the currently selected contributor.
                        if (note.Audit.Author_Id == collaborator.AuthorId)
                        {
                            // has the disposition of the n been resolved? if so, it's status will now
                            // be Enum.Status.ContributorAdded and the style of the n should reflect a pending addition.
                            if (status == (int)_Enum.Status.ContributorAdded)
                            {
                                note.Foreground = Preferences.AddedColor;
                                EA.GetEvent<UpdateNote>().Publish(note);
                            }
                        }
                        else
                        {
                            // has the disposition of the n been resolved? if so, it's status will now
                            // be Enum.Status.ContributorDeleted and the style of the n should reflect a pending deletion.
                            if (status == (int)_Enum.Status.ContributorDeleted)
                            {
                                if (note.Audit.CollaboratorIndex == Collaborations.CurrentCollaborator.Index)
                                //is the creator of this n the currently selected col ?
                                {
                                    note.Foreground = Preferences.DeletedColor;
                                    EA.GetEvent<UpdateNote>().Publish(note);
                                }
                            }
                        }
                        EA.GetEvent<UpdateNote>().Publish(note);
                    }
                    else
                    {
                        // if we arrive here, the current user is a contributor, not the author. 
                        // There can be many contributors but only one author, so unlike 
                        // above we only need to check the status of the n instead of 
                        // both status and ownership.

                        switch (status)
                        {
                            case (int)_Enum.Status.AuthorAdded:
                                // has the disposition of the n been resolved? if so, it's status will now
                                // be Enum.Status.AuthorAdded and the style of the n should reflect a pending addition.
                                note.Foreground = Preferences.AddedColor;
                                EA.GetEvent<UpdateNote>().Publish(note);
                                break;
                            case (int)_Enum.Status.AuthorDeleted:
                                // has the disposition of the n been resolved? if so, it's status will now
                                // be Enum.Status.AuthorDeleted and the style of the n should reflect a pending deletion.
                                note.Foreground = Preferences.DeletedColor;
                                EA.GetEvent<UpdateNote>().Publish(note);
                                break;
                        }
                    }
                }
                EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Collaboration);
            }
            UpdateComposition();
        }

        private void UpdateComposition()
        {
            for (var i = 0; i < Cache.Measures.Count; i++)
            {
                var measure = Cache.Measures[i];
                if (!measure.Chords.Any()) continue;
                EA.GetEvent<MeasureLoaded>().Publish(measure.Id);
                EA.GetEvent<UpdateSpanManager>().Publish(measure.Id);
                EA.GetEvent<UpdateSubverses>().Publish(string.Empty);
            }
            EA.GetEvent<UpdateArc>().Publish(string.Empty);
        }
    }
}