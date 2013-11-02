using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Composer.Infrastructure.Events;
using Composer.Infrastructure;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Presentation.Commands;
using Composer.Infrastructure.Behavior;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class LyricsPanelViewModel : BaseViewModel, ILyricsPanelViewModel
    {
        private int _verseIndex;
        private int _verseSequence;
        private int _verseDisposition;
        private int _active;
        private int _inactive;

        private enum LyricsEditorState { Adding, Applying, Editing, None }
        private LyricsEditorState _editorState = LyricsEditorState.None;

        private static DataServiceRepository<Repository.DataService.Composition> _repository;

        private const string NewVerseMessage = "Type or paste verse text here.";

        private ObservableCollection<Repository.DataService.Verse> _verses = new ObservableCollection<Repository.DataService.Verse>();
        public ObservableCollection<Repository.DataService.Verse> Verses
        {
            get { return _verses; }
            set
            {
                _verses = value;
                _active = (from a in _verses where a.Disposition == 1 select a).Count();
                _inactive = (from a in _verses where a.Disposition == 0 select a).Count();
                _verses = new ObservableCollection<Repository.DataService.Verse>(_verses.OrderBy(p => p.Index));

                _verseIndex = _verses.Count();
                _verseSequence = _verseIndex * Defaults.SequenceIncrement;
                VerseListVisible = (_verses.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged(() => Verses);
            }
        }

        private Repository.DataService.Verse _selectedVerse;
        public Repository.DataService.Verse SelectedVerse
        {
            get { return _selectedVerse; }
            set
            {
                if (_selectedVerse != null) { _selectedVerse.UIHelper = string.Empty; }
                ResetEditor();
                _selectedVerse = value;
                if (_selectedVerse != null)
                {
                    if (_selectedVerse.Disposition == 1)
                    {
                        EditorEnabled = true;
                        _editorState = LyricsEditorState.Editing;
                    }
                    EditorText = _selectedVerse.Text;
                    EditorIndex = _selectedVerse.Index;
                    _selectedVerse.UIHelper = "Selected";
                }
                OnPropertyChanged(() => SelectedVerse);
            }
        }

        private int _editorIndex = Constants.INVALID_VERSE_INDEX;
        public int EditorIndex
        {
            get { return _editorIndex; }
            set
            {
                if (value != _editorIndex)
                {
                    _editorIndex = value;
                    OnPropertyChanged(() => EditorIndex);
                }
            }
        }

        private string _editorText = string.Empty;
        public string EditorText
        {
            get { return _editorText; }
            set
            {
                if (value != _editorText)
                {
                    _editorText = value;
                    _editorText = _editorText.Replace(Defaults.VerseWordDelimitter, '*');
                    _editorText = _editorText.Replace(Defaults.VerseWordPropertyDelimitter, '*');
                    OnPropertyChanged(() => EditorText);
                }
            }
        }

        private bool _editorEnabled;
        public bool EditorEnabled
        {
            get
            {
                return _editorEnabled;
            }
            set
            {
                if (value != _editorEnabled)
                {
                    _editorEnabled = value;
                    OnPropertyChanged(() => EditorEnabled);
                }
            }
        }

        public LyricsPanelViewModel()
        {
            _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();

            DefineCommands();
            SubscribeEvents();
            ResetEditor();
        }

        private void DefineCommands()
        {
            ClearButtonClickedCommand = new DelegateCommand<object>(OnClearButtonClicked, OnCanExecuteClear);
            DoneButtonClickedCommand = new DelegateCommand<object>(OnDoneButtonClicked, OnCanExecuteDone);
            NewButtonClickedCommand = new DelegateCommand<object>(OnNewButtonClicked, OnCanExecuteNew);
            ApplyButtonClickedCommand = new DelegateCommand<object>(OnApplyButtonClicked, OnCanExecuteApply);
            TextChangedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnTextChangedCommand, null);
            ClickCloseCommand = new DelegatedCommand<object>(OnClose);
        }

        public void OnClose(object obj)
        {
            Close();
        }

        private DelegatedCommand<object> _clickCloseCommand;
        public DelegatedCommand<object> ClickCloseCommand
        {
            get { return _clickCloseCommand; }
            set
            {
                _clickCloseCommand = value;
                OnPropertyChanged(() => ClickCloseCommand);
            }
        }

        public void OnTextChangedCommand(ExtendedCommandParameter sender)
        {
            var tb = (TextBox)sender.Sender;
            EditorText = tb.Text;
            CanExecuteApply = EditorText.Length > 0 && EditorText.Trim() != NewVerseMessage;
            CanExecuteClear = EditorText.Length > 0;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _textChangedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> TextChangedCommand
        {
            get { return _textChangedCommand; }
            set
            {
                if (value != _textChangedCommand)
                {
                    _textChangedCommand = value;
                    OnPropertyChanged(() => TextChangedCommand);
                }
            }
        }

        private void SubscribeEvents()
        {
            EA.GetEvent<UpdateLyricsPanel>().Subscribe(OnUpdateLyricsPanel);
            EA.GetEvent<ReorderVerses>().Subscribe(OnReorderVerses);
            EA.GetEvent<DeleteVerse>().Subscribe(OnDeleteVerse);
            EA.GetEvent<ToggleVerseInclusion>().Subscribe(OnToggleVerseInclusion);
            EA.GetEvent<CloneVerse>().Subscribe(OnClone);
            EA.GetEvent<UpdateSubverses>().Subscribe(OnUpdateSubverses);
        }

        public void OnUpdateSubverses(object obj)
        {
            UpdateSubverses();
        }

        private void UpdateSubverses()
        {
            Compress();
            Verses = new ObservableCollection<Repository.DataService.Verse>(Verses.OrderBy(p => p.Index));
            EA.GetEvent<ClearVerses>().Publish(string.Empty);
            foreach (var verse in Verses)
            {
                EditorText = verse.Text;
                EditorIndex = verse.Index;
                _verseSequence = verse.Sequence;
                if (verse.Disposition != null) _verseDisposition = (int)verse.Disposition;
                ApplySubverses();
            }
        }

        public void OnClone(object obj)
        {

        }

        public void OnDeleteVerse(int index)
        {
            _repository.Delete(SelectedVerse);
            Cache.Verses.Remove(SelectedVerse);
            CompositionManager.Composition.Verses.Remove(SelectedVerse);
            Verses.Remove(SelectedVerse);
            UpdateSubverses();
        }

        public void OnToggleVerseInclusion(Tuple<string, int> payload)
        {
            SelectedVerse.Disposition = (short)((SelectedVerse.Disposition == 0) ? 1 : 0);
            SelectedVerse.Index = (SelectedVerse.Disposition == 0) ? (short)(_active + 1) : (short)(_active + _inactive + 1);
            SelectedVerse = _selectedVerse;

            UpdateSubverses();
            ClearVerse();
        }

        public void OnReorderVerses(Tuple<_Enum.Direction, int> payload)
        {
            var direction = payload.Item1;
            int sourceIndex = SelectedVerse.Index;
            var targetIndex = Constants.INVALID_VERSE_INDEX;
            var savedState = SelectedVerse;
            switch (direction)
            {
                case _Enum.Direction.Up:
                    if (sourceIndex > 1)
                    {
                        targetIndex = sourceIndex - 1;
                    }
                    break;
                case _Enum.Direction.Down:
                    if (sourceIndex < Verses.Count)
                    {
                        targetIndex = sourceIndex + 1;
                    }
                    break;
            }
            if (targetIndex != Constants.INVALID_VERSE_INDEX)
            {
                var targetVerse = (from a in Verses where a.Index == targetIndex select a).First();
                var sourceVerse = (from a in Verses where a.Index == sourceIndex select a).First();
                sourceVerse.Index = (short)targetIndex;
                targetVerse.Index = (short)sourceIndex;
                UpdateSubverses();
                ResetEditor();
                SelectedVerse = savedState;
            }
        }

        public void OnUpdateLyricsPanel(object obj)
        {
            Verses = (ObservableCollection<Repository.DataService.Verse>)obj;
            if (Verses != null)
            {
                UpdateSubverses();
                ResetEditor();
            }
        }

        #region Done Button Support

        private bool _canExecuteDone = true;
        public bool CanExecuteDone
        {
            get { return _canExecuteDone; }
            set
            {
                _canExecuteDone = value;
                DoneButtonClickedCommand.RaiseCanExecuteChanged();
            }
        }
        public bool OnCanExecuteDone(object obj)
        {
            return CanExecuteDone;
        }
        public DelegateCommand<object> DoneButtonClickedCommand { get; private set; }

        public void OnDoneButtonClicked(object obj)
        {
            Close();
        }

        private void Close()
        {
            EA.GetEvent<HideLyricsPanel>().Publish(string.Empty);
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

        public void OnClearButtonClicked(object obj)
        {
            ClearVerse();
        }

        private void ClearVerse()
        {
            ResetEditor();
            CanExecuteClear = false;
            CanExecuteApply = false;
            SelectedVerse = null;
        }

        #endregion

        #region New Button Support

        private bool _canExecuteNew = true;
        public bool CanExecuteNew
        {
            get { return _canExecuteNew; }
            set
            {
                _canExecuteNew = value;
                NewButtonClickedCommand.RaiseCanExecuteChanged();
            }
        }

        public bool OnCanExecuteNew(object obj)
        {
            return CanExecuteNew;
        }

        public DelegateCommand<object> NewButtonClickedCommand { get; private set; }

        public void OnNewButtonClicked(object obj)
        {
            _editorState = LyricsEditorState.Adding;
            CanExecuteApply = false;
            ClearVerse();
            New();
        }

        private void New()
        {
            EditorEnabled = true;
            EditorText = NewVerseMessage;
        }

        #endregion

        #region Apply Button Support

        private bool _canExecuteApply;
        public bool CanExecuteApply
        {
            get { return _canExecuteApply; }
            set
            {
                _canExecuteApply = value;
                ApplyButtonClickedCommand.RaiseCanExecuteChanged();
            }
        }

        public bool OnCanExecuteApply(object obj)
        {
            return CanExecuteApply;
        }

        private void ResetEditor()
        {
            EditorText = "";
            EditorIndex = Constants.INVALID_VERSE_INDEX;
            _editorState = LyricsEditorState.None;
        }

        private Visibility _verseListVisible = Visibility.Collapsed;
        public Visibility VerseListVisible
        {
            get { return _verseListVisible; }
            set
            {
                _verseListVisible = value;
                OnPropertyChanged(() => VerseListVisible);
            }
        }

        public DelegateCommand<object> ApplyButtonClickedCommand { get; private set; }

        public void OnApplyButtonClicked(object obj)
        {
            if (Validate())
            {
                if (_editorState != LyricsEditorState.Applying)
                {
                    _editorState = LyricsEditorState.Applying;
                    if (_selectedVerse != null)
                    {
                        SelectedVerse.Text = EditorText;
                    }
                    else
                    {
                        CreateVerse();
                    }
                    UpdateSubverses();
                    ClearVerse();
                }
                EditorState.VerseCount = Verses.Count;
                EA.GetEvent<AdjustBracketHeight>().Publish(string.Empty);
            }
        }

        private void CreateVerse()
        {
            _verseSequence = Verses.Count*Defaults.SequenceIncrement;
            _verseIndex = Verses.Count + 1;
            var verse = VerseManager.Create(CompositionManager.Composition.Id, _verseSequence);
            verse.Text = EditorText;
            verse.Index = (short) _verseIndex;
            verse.Sequence = _verseSequence;
            Verses.Add(verse);
            CompositionManager.Composition.Verses.Add(verse);
        }

        private bool Validate()
        {
            var result = !(EditorText.IndexOf(NewVerseMessage, StringComparison.Ordinal) >= 0);
            return result;
        }

        private void Compress()
        {
            short index = 0;
            Verses = new ObservableCollection<Repository.DataService.Verse>(Verses.OrderBy(p => p.Index));
            foreach (var verse in Verses.Where(verse => verse.Disposition == 1))
            {
                index++;
                verse.Index = index;
            }
            foreach (var verse in Verses.Where(verse => verse.Disposition == 0))
            {
                index++;
                verse.Index = index;
            }
            Verses = new ObservableCollection<Repository.DataService.Verse>(Verses.OrderBy(p => p.Index));
        }

        private void ApplySubverses()
        {
            // Apply is called in 2 places. the click handler for the apply button, and programmatically when a composition is loaded from the database.
            // when the composition is loaded from db, code loops through each verse, passes the verse text to this view model. here, editor is set to the 
            // verse text, and this method is called.

            // split verseText into words, gaps, syllables and dashes so we can iterate over them.
            // TODO: use RX here

            var i = 0;
            var complete = false;
            var words = EditorText.Split(' ');
            Repository.DataService.Chord pCh = null;
            try
            {
                foreach (var sg in CompositionManager.Composition.Staffgroups)
                {
                    foreach (var s in sg.Staffs)
                    {
                        if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple ||
                           (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand && s.Index % 2 == 0))
                        {
                            foreach (var m in s.Measures)
                            {
                                ObservableCollection<Repository.DataService.Chord> chs = m.Chords;
                                chs = new ObservableCollection<Repository.DataService.Chord>(chs.OrderBy(p => p.StartTime));
                                if (chs.Count > 0)
                                {
                                    VerseManager.Words = new ObservableCollection<Word>();
                                    foreach (var ch in chs)
                                    {
                                        if (CollaborationManager.IsActive(ch))
                                        {
                                            //alignment value is used to override the Canvas.Left value, not to bind to the HorizontalAligment attribute.
                                            var alignment = (VerseManager.Words.Count == 0) ? Defaults.AlignLeft : Defaults.AlignCenter; //first word in eaach m ls left justified.
                                            var x = (VerseManager.Words.Count == 0) ? Measure.Padding : ch.Location_X;
                                            var _w = words[i];
                                            Word w;
                                            if (_w == string.Format("{0}", Lyrics.SplitChordHyphen))
                                            {
                                                //the word being placed in this case is '-' and it is positioned between 2 adjacent chs
                                                var previousX = (pCh != null) ? pCh.Location_X : Measure.Padding;
                                                x = x - (ch.Location_X - previousX) / 2;
                                                if (ch.StartTime != null)
                                                {
                                                    w = new Word(m.Id, (double)ch.StartTime, _verseIndex, _w, x, Defaults.AlignCenter);
                                                    VerseManager.Words.Add(w);
                                                }

                                                if (i == words.Length - 1)
                                                {
                                                    complete = true;
                                                    break;
                                                }
                                                i++;
                                                //Add the next word
                                                x = ch.Location_X;
                                                _w = words[i];
                                                if (ch.StartTime != null)
                                                {
                                                    w = new Word(m.Id, (double)ch.StartTime, _verseIndex, _w, x, alignment);
                                                    VerseManager.Words.Add(w);
                                                }
                                                if (i == words.Length - 1)
                                                {
                                                    complete = true;
                                                    break;
                                                }
                                                i++;
                                            }
                                            else
                                            {
                                                if (ch.StartTime != null)
                                                {
                                                    w = new Word(m.Id, (double)ch.StartTime, _verseIndex, _w, x, alignment);
                                                    VerseManager.Words.Add(w);
                                                }
                                                if (i == words.Length - 1)
                                                {
                                                    complete = true;
                                                    break;
                                                }
                                                i++;
                                            }
                                            pCh = ch;
                                        }
                                    }
                                    // send verse as 'measure snippets' to their respective measures
                                    var payload = new Tuple<object, int, int, Guid, int, int>(VerseManager.Words, _verseSequence, EditorIndex, m.Id, _verseDisposition, m.Index);
                                    EA.GetEvent<ApplyVerse>().Publish(payload);
                                }
                                if (complete) break;
                            }
                            if (complete) break;
                        }
                        if (complete) break;
                    }
                    EA.GetEvent<UpdateVerseIndexes>().Publish(Verses.Count);
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "Exception in ApplySubverses");
            }
        }

        #endregion
    }
}