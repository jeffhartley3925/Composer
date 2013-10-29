using System;
using System.Linq;
using System.Windows;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using System.Collections.Generic;
using Composer.Infrastructure.Dimensions;
using System.Collections.ObjectModel;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class EditPopupViewModel : BaseViewModel, IEditPopupViewModel
    {
        private MeasureViewModel _vm;
        private Repository.DataService.Measure _m;
        private _Enum.PasteCommandSource _pasteCommandSource = _Enum.PasteCommandSource.Na;
        private int _clipPointer;
        public EditPopupViewModel()
        {
            DefineCommands();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            EA.GetEvent<UpdateEditPopupMenuItemsEnableState>().Subscribe(OnUpdateEditPopupItemsEnableState);
            EA.GetEvent<EditPopupItemClicked>().Subscribe(OnEditPopupItemClicked);
            EA.GetEvent<UpdateEditPopupMenuTargetMeasure>().Subscribe(OnUpdateEditPopupTargetMeasureViewModel);
            EA.GetEvent<UpdateMeasureBar>().Subscribe(OnUpdateMeasureBar);
            EA.GetEvent<Paste>().Subscribe(OnPaste);
        }

        public void OnUpdateMeasureBar(short id)
        {
            //this event has several publishers. We only want to 
            //catch this event here when we are NOT adding a staffgroup.
            if (!EditorState.IsAddingStaffgroup)
            {
                _vm.Bar_Id = id;
            }
        }

        private List<Bar> _bars = Infrastructure.Dimensions.Bars.BarList;
        public List<Bar> Bars
        {
            get { return _bars; }
            set
            {
                _bars = value;
                OnPropertyChanged(() => Bars);
            }
        }

        public void OnEditPopupItemClicked(Tuple<string, string, _Enum.PasteCommandSource> payload)
        {
            var item = payload.Item1;
            var category = payload.Item2;
            _pasteCommandSource = payload.Item3;
            if (_pasteCommandSource == _Enum.PasteCommandSource.User)
            {
                _clipPointer = 0;
            }
            //both ChordManager.SelectedChordId and NoteController.SelectedNoteId are set in the NoteViewModel rightmousedown handler in anticipation
            //of the user selecting 'Note' or '_chord' from the 'Select' menu. We don't know what the user will do, so to maintain consistent state
            //when the user makes his choice.....
            //      1.If the user selects 'Note' set ChordManager.SelectedChordId to empty since a '_chord' was not selected
            //      2.if the user selects '_chord' set NoteController.SelectedNoteId to empty since a 'Note' was not selected
            //      3.If any other option is selected, set both ChordManager.SelectedChordId and NoteController.SelectedNoteId to empty since neither was selected.
            var clearSelectedNoteId = true;
            var clearSelectedChordId = true;

            switch (category)
            {
                case "Arc":
                    switch (item)
                    {
                        case Infrastructure.Constants.EditActions.Delete:
                            EA.GetEvent<DeleteArc>().Publish(ArcManager.SelectedArcId);
                            break;
                        case Infrastructure.Constants.EditActions.Flip:
                            EA.GetEvent<FlipArc>().Publish(ArcManager.SelectedArcId);
                            break;
                    }
                    break;
                default:
                    switch (item)
                    {
                        case Infrastructure.Constants.ObjectName.Note:
                            clearSelectedNoteId = false;
                            EA.GetEvent<SelectNote>().Publish(NoteController.SelectedNoteId);
                            break;

                        case Infrastructure.Constants.ObjectName.Chord:
                            clearSelectedChordId = false;
                            EA.GetEvent<SelectChord>().Publish(ChordManager.SelectedChordId);
                            break;

                        case Infrastructure.Constants.ObjectName.Measure:
                            EA.GetEvent<SelectMeasure>().Publish(_m.Id);
                            break;

                        case Infrastructure.Constants.ObjectName.Staff:
                            EA.GetEvent<SelectStaff>().Publish(_m.Staff_Id);
                            break;

                        case Infrastructure.Constants.ObjectName.Staffgroup:

                            var query = from sg in Cache.Staffgroups
                                        join s in Cache.Staffs
                                        on sg.Id equals s.Staffgroup_Id
                                        where s.Id == _m.Staff_Id
                                        select new { sgId = sg.Id };

                            EA.GetEvent<SelectStaffgroup>().Publish(query.First().sgId);
                            break;

                        case Infrastructure.Constants.ObjectName.Composition:
                            EA.GetEvent<SelectComposition>().Publish(new object());
                            break;

                        case Infrastructure.Constants.EditActions.Copy:
                            Infrastructure.Support.Clipboard.Notes = Infrastructure.Support.Selection.Notes;
                            break;

                        case Infrastructure.Constants.EditActions.Delete:
                            Infrastructure.Support.Selection.RemoveAll();
                            break;

                        case Infrastructure.Constants.EditActions.Paste:
                            //we can arrive here 2 ways. user initiated paste and programmatic paste
                            Infrastructure.Support.Selection.RemoveAll();
                            OnPaste(string.Empty);
                            break;

                        case Infrastructure.Constants.EditActions.InsertBar:
                            break;
                    }
                    break;
            }

            if (clearSelectedNoteId)
                NoteController.SelectedNoteId = Guid.Empty;

            if (clearSelectedChordId)
                ChordManager.SelectedChordId = Guid.Empty;
        }

        public void OnPaste(object obj)
        {
            //pasted chs are appended to the target measure. if the measure packs, but there are more chs to paste, then save clipboard item
            //idx, span the measure and broadcast message to all measureviewModels requesting
            //the viewModel of the next measure (by idx). the appropriate viewModel is returned by programmtically
            //'borrowing' (reusing) the EditPopupMenu Paste event (hack?). the same process continues untill all chs
            //are pasted or until a measure with chs in it is encountered.
            EditorState.IsPasting = true;

            var chords = new List<Repository.DataService.Chord>();

            if (_pasteCommandSource == _Enum.PasteCommandSource.Programmatic)
            {
                if (_m.Chords.Count > 0)
                {
                    //FUTURE: this is where we would add code to shift the entire remaining composition right to accomodate the entire paste.
                    _clipPointer = 0;
                    return;
                }
            }
            if (MeasureManager.IsPacked(ChordManager.Measure))
            {
                EA.GetEvent<ArrangeMeasure>().Publish(_m);
            }
            else
            {
                ChordManager.Measure = _m;

                //FUTURE: Add ability to insert paste. right now append paste only

                var chs = new ObservableCollection<Repository.DataService.Chord>(_m.Chords.OrderByDescending(p => p.StartTime));

                var lastCh = (chs.Count == 0) ? null : chs[0];

                for (var i = _clipPointer; i < Infrastructure.Support.Clipboard.Chords.Count; i++)
                {
                    var clipCh = Infrastructure.Support.Clipboard.Chords[i];
                    var x = GetChordXCoordinate(lastCh, clipCh);
                    EditorState.Chord = null;
                    var ch = ChordManager.Clone(_m, clipCh);
                    EA.GetEvent<SynchronizeChord>().Publish(ch);

                    if (_vm.ValidPlacement())
                    {
                        ch.StartTime = (double)_m.Duration + _m.Index * DurationManager.Bpm;
                        EA.GetEvent<SynchronizeChord>().Publish(ch);
                        ch.Location_X = x;
                        _m.Duration += ch.Duration;
                        _m.Chords.Add(ch);
                        lastCh = ch;
                        chords.Add(ch);
                        if (MeasureManager.IsPacked(ChordManager.Measure))
                        {
                            EA.GetEvent<ArrangeMeasure>().Publish(_m);
                        }
                    }
                    else
                    {
                        _clipPointer = i;
                        EA.GetEvent<UpdateSpanManager>().Publish(_m.Id);
                        EA.GetEvent<SpanMeasure>().Publish(_m);
                        _vm.GetNextPasteTarget();
                        break;
                    }
                }
                EA.GetEvent<UpdateSpanManager>().Publish(_m.Id);
                EA.GetEvent<SpanMeasure>().Publish(_m);
                EditorState.IsPasting = false;
            }
        }

        private int GetChordXCoordinate(Repository.DataService.Chord lastCh, Repository.DataService.Chord clipCh)
        {
            var x = Infrastructure.Constants.Measure.Padding;
            if (lastCh != null)
            {
                //TODO: use the DurationManager.GetProportionalSpace overload
                var proportionalSpace = (from a in DurationManager.Durations
                                            where (a.Value == (double)clipCh.Duration)
                                            select a.Spacing).Single();
                x = lastCh.Location_X + proportionalSpace;
            }
            return x;
        }

        public void OnUpdateEditPopupTargetMeasureViewModel(object obj)
        {
            _vm = (MeasureViewModel)obj;
            _m = _vm.Measure;
        }

        public void OnUpdateEditPopupItemsEnableState(object obj)
        {
            SelectNoteEnabled = false;
            SelectChordEnabled = false;

            if (EditorState.IsOverBar)
            {
                //when the mouse enters a bar, the mouseEnter handler (in MeasureViewModel) sets EditorState.IsOverBar to true;
                //when the mouse leaves the bar EditorState.IsOverBar to false;
                EditItemsVisibility = Visibility.Collapsed;
                BarsVisibility = Visibility.Visible;
            }
            else
            {
                EditItemsVisibility = Visibility.Visible;
                BarsVisibility = Visibility.Collapsed;
                SelectEnabled = true;

                int clipBoardNoteCount = (Infrastructure.Support.Clipboard.Notes == null) ? 0 : Infrastructure.Support.Clipboard.Notes.Count();
 
                SelectMeasureEnabled = _m.Chords.Count > 0;
                SelectCompositionEnabled = EditorState.IsComposing;

                if (EditorState.IsOverNote)
                {
                    //when a n is right clicked, the right click handler (in NoteViewModel) sets EditorState.IsOverNote to true;
                    SelectNoteEnabled = true;
                    SelectChordEnabled = true;
                }

                if (EditorState.IsOverArc)
                {
                    //when an arc is right clicked, the right click handler (in ArcViewModel) sets EditorState.IsOverArc to true;
                    ArcEnabled = true;
                    ArcItemsVisibility = Visibility.Visible;
                    EditItemsVisibility = Visibility.Collapsed;
                }
                else
                {
                    ArcEnabled = false;
                    ArcItemsVisibility = Visibility.Collapsed;
                }

                //the only place where EditorState.IsOverNote is used is here. Once we've used it, set it back to false;
                EditorState.IsOverNote = false;

                if (SelectMeasureEnabled)
                {
                    SelectStaffEnabled = true;
                    SelectStaffgroupEnabled = true;
                }
                else
                {
                    var sId = _m.Staff_Id;
                    SelectStaffEnabled = (from a in Cache.Measures where a.Staff_Id == sId select a.Chords.Count).Sum() > 0;
                    SelectStaffgroupEnabled = false;
                }
                if (Infrastructure.Support.Selection.Notes != null)
                {
                    DeleteEnabled = Infrastructure.Support.Selection.Notes.Count > 0;
                    CopyEnabled = Infrastructure.Support.Selection.Notes.Count > 0;
                }
                PasteEnabled = clipBoardNoteCount > 0;
            }
        }

        #region MenuItem enable/disable and visibility properties

        private void DefineCommands()
        {

        }

        private bool _inserBarEnabled;

        public bool InserBarEnabled
        {
            get { return _inserBarEnabled; }
            set
            {
                _inserBarEnabled = value;
                OnPropertyChanged(() => InserBarEnabled);
            }
        }

        private bool _arcEnabled;

        public bool ArcEnabled
        {
            get { return _arcEnabled; }
            set
            {
                _arcEnabled = value;
                OnPropertyChanged(() => ArcEnabled);
            }
        }

        private bool _selectNoteEnabled;

        public bool SelectNoteEnabled
        {
            get { return _selectNoteEnabled; }
            set
            {
                _selectNoteEnabled = value;
                OnPropertyChanged(() => SelectNoteEnabled);
            }
        }

        private Visibility _editItemsVisibility = Visibility.Collapsed;

        public Visibility EditItemsVisibility
        {
            get { return _editItemsVisibility; }
            set
            {
                _editItemsVisibility = value;
                OnPropertyChanged(() => EditItemsVisibility);
            }
        }

        private Visibility _arcItemsVisibility = Visibility.Collapsed;

        public Visibility ArcItemsVisibility
        {
            get { return _arcItemsVisibility; }
            set
            {
                _arcItemsVisibility = value;
                OnPropertyChanged(() => ArcItemsVisibility);
            }
        }

        private Visibility _barsVisibility = Visibility.Collapsed;

        public Visibility BarsVisibility
        {
            get { return _barsVisibility; }
            set
            {
                _barsVisibility = value;
                OnPropertyChanged(() => BarsVisibility);
            }
        }

        private bool _selectChordEnabled;

        public bool SelectChordEnabled
        {
            get { return _selectChordEnabled; }
            set
            {
                _selectChordEnabled = value;
                OnPropertyChanged(() => SelectChordEnabled);
            }
        }

        private bool _selectStaffEnabled;

        public bool SelectStaffEnabled
        {
            get { return _selectStaffEnabled; }
            set
            {
                _selectStaffEnabled = value;
                OnPropertyChanged(() => SelectStaffEnabled);
            }
        }

        private bool _selectStaffgroupEnabled;

        public bool SelectStaffgroupEnabled
        {
            get { return _selectStaffgroupEnabled; }
            set
            {
                _selectStaffgroupEnabled = value;
                OnPropertyChanged(() => SelectStaffgroupEnabled);
            }
        }

        private bool _selectCompositionEnabled;

        public bool SelectCompositionEnabled
        {
            get { return _selectCompositionEnabled; }
            set
            {
                _selectCompositionEnabled = value;
                OnPropertyChanged(() => SelectCompositionEnabled);
            }
        }

        private bool _selectMeasureEnabled;

        public bool SelectMeasureEnabled
        {
            get { return _selectMeasureEnabled; }
            set
            {
                _selectMeasureEnabled = value;
                OnPropertyChanged(() => SelectMeasureEnabled);
            }
        }

        private bool _copyEnabled;

        public bool CopyEnabled
        {
            get { return _copyEnabled; }
            set
            {
                _copyEnabled = value;
                OnPropertyChanged(() => CopyEnabled);
            }
        }

        private bool _selectEnabled;

        public bool SelectEnabled
        {
            get { return _selectEnabled; }
            set
            {
                _selectEnabled = value;
                OnPropertyChanged(() => SelectEnabled);
            }
        }

        private bool _deleteEnabled;

        public bool DeleteEnabled
        {
            get { return _deleteEnabled; }
            set
            {
                _deleteEnabled = value;
                OnPropertyChanged(() => DeleteEnabled);
            }
        }

        private bool _pasteEnabled;

        public bool PasteEnabled
        {
            get { return _pasteEnabled; }
            set
            {
                _pasteEnabled = value;
                OnPropertyChanged(() => PasteEnabled);
            }
        }

        #endregion
    }
}