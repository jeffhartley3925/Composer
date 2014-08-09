using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using Composer.Infrastructure;
using Composer.Infrastructure.Behavior;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Dimensions;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Support;
using Composer.Modules.Composition.Models;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Modules.Composition.Views;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.ServiceLocation;
using Measure = Composer.Repository.DataService.Measure;
using Selection = Composer.Infrastructure.Support.Selection;

namespace Composer.Modules.Composition.ViewModels
{
	using Composer.Modules.Composition.Annotations;

	public sealed partial class MeasureViewModel : BaseViewModel, IMeasureViewModel, IEventCatcher
	{
		private IEnumerable<Chord> _mGcHs;
		private readonly int _mDensity = Densities.MeasureDensity;
		public MeasureView View;

		#region Fields

		private decimal[] _chordStartTimes;
		private decimal[] _chordInactiveTimes;
		private decimal _starttime;

		private string _addNoteToChordPath = string.Empty;
		private string _background = Preferences.MeasureBackground;

		private double _baseRatio;
		private bool _debugging = false;
		private decimal _duration;

		private string _foreground = Preferences.MeasureForeground;

		private double _initializedWidth;
		private string _insertNotePath = string.Empty;
		private string _insertRestPath = string.Empty;
		private bool _isMouseCaptured;

		private int _loadedChordsCount;
		private ObservableCollection<Span> _localSpans;
		private Measure _measure;

		private Dictionary<decimal, List<Notegroup>> _mEcHNgs;
		private double _mouseX;
		private bool _okToResize = true;
		private Visibility _playbackControlVisibility = Visibility.Collapsed;
		private double _ratio;
		private string _replaceNoteWithRestPath = string.Empty;
		private string _replaceRestWithNotePath = string.Empty;
		private ObservableCollection<Subverse> _subVerses;
		public static List<Notegroup> ChordNotegroups { get; set; }
		public static Chord Chord { get; set; }
		private Chord LeftChord;
		private Chord RightChord;

		#endregion

		public MeasureViewModel(string id)
		{
			View = null;
			VerseMargin = "8,-5,0,0";
			EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);

			HideSelector();
			var measure = Cache.Measures.Where(a => a.Id == Guid.Parse(id)).DefaultIfEmpty(null).Single();
			if (measure != null)
			{
				EditorState.ResizedMeasureIndexes.Add(measure.Index);
				Measure = measure;
				Width = int.Parse(Measure.Width);
				if (Measure.TimeSignature_Id != null)
				{
					TimeSignature_Id = (int)Measure.TimeSignature_Id;
				}
			}
			SubscribeEvents();
			DefineCommands();
			_initializedWidth = Width;
			if (Measure.Chords.Count > 0)
			{
				if (Measuregroup != null)
				{
					//EA.GetEvent<RespaceMeasuregroup>().Publish(Measuregroup.Id);
				}
				EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
			}
			UpdateMeasureDuration();
			SetActiveMeasureCount();
			EA.GetEvent<UpdateMeasurePackState>().Publish(new Tuple<Guid, _Enum.EntityFilter>(Measure.Id, _Enum.EntityFilter.Measure));
			_ratio = GetRatio();
			_baseRatio = _ratio;
			EA.GetEvent<SetMeasureEndBar>().Publish(string.Empty);
			PlaybackControlVisibility = Visibility.Collapsed;
			SetTextPaths();
            Repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
		}

        private Measuregroup _measureGroup = null;
        public Measuregroup Measuregroup
        {
            get 
            {
                if (_measureGroup == null)
                {
                    if (MeasuregroupManager.CompMgs != null)
                    {
                        var b = (from a in MeasuregroupManager.CompMgs where a.Measures.Contains(Measure) select a);
                        if (b.Any())
                        {
                            _measureGroup = b.First();
                        }
                    }
                }
                return _measureGroup;
            }
            set
            {
                _measureGroup = value;
            }
        }

		[CanBeNull]
		public Chord LastMgCh { get; set; }

		private IEnumerable<Chord> _activeMeasureGroupChords;
		public IEnumerable<Chord> ActiveMgChs
		{
			get { return _activeMeasureGroupChords ?? (_activeMeasureGroupChords = new ObservableCollection<Chord>()); }
			set
			{
				_activeMeasureGroupChords = value;
				_activeMeasureGroupChords = new ObservableCollection<Chord>(_activeMeasureGroupChords.OrderBy(p => p.StartTime));
			}
		}

		public Chord LastSqCh { get; set; }

		private IEnumerable<Chord> _activeSequenceChords;
		public IEnumerable<Chord> ActiveSqChs
		{
			get { return _activeSequenceChords ?? (_activeSequenceChords = new ObservableCollection<Chord>()); }
			set
			{
				_activeSequenceChords = value;
				_activeSequenceChords = new ObservableCollection<Chord>(_activeSequenceChords.OrderBy(p => p.StartTime));
			}
		}

		private ObservableCollection<Chord> _activeChords;
		public ObservableCollection<Chord> ActiveMeChs
		{
			get { return _activeChords ?? (_activeChords = new ObservableCollection<Chord>()); }
			set
			{
				_activeChords = value;
				_activeChords = new ObservableCollection<Chord>(_activeChords.OrderBy(p => p.StartTime));
			}
		}

		public void OnUpdateActiveChords(Guid id)
		{
			if (id != Measure.Id || Measure.Chords.Count <= 0) return;
			this.ActiveMeChs = new ObservableCollection<Chord>((
				from a in Measure.Chords
				where CollaborationManager.IsActive(a)
				select a).OrderBy(p => p.StartTime));
			this.ActiveSqChs = Utils.GetActiveChordsBySequence(Measure.Sequence, Guid.Empty);
            this.ActiveMgChs = Utils.GetMeasureGroupChords(Measure.Id, Guid.Empty, _Enum.Filter.Indistinct);
			this.LastSqCh = (this.ActiveSqChs.Any()) ? (from c in this.ActiveSqChs select c).Last() : null;
			this.LastMgCh = (this.ActiveMgChs.Any()) ? (from c in this.ActiveMgChs select c).Last() : null;
			EA.GetEvent<NotifyActiveChords>().Publish(new Tuple<Guid, object, object, object, int, Guid>(Measure.Id, ActiveMeChs, ActiveSqChs, ActiveMgChs, Measure.Sequence, Measuregroup.Id));
		}

		public Visibility PlaybackControlVisibility
		{
			get { return _playbackControlVisibility; }
			set
			{
				_playbackControlVisibility = value;
				OnPropertyChanged(() => PlaybackControlVisibility);
			}
		}

		public decimal Duration
		{
			get { return _duration; }
			set
			{
				_duration = value;
				OnPropertyChanged(() => Duration);
			}
		}

		public string Background
		{
			get { return _background; }
			set
			{
				_background = value;
				OnPropertyChanged(() => Background);
			}
		}

		public string Foreground
		{
			get { return _foreground; }
			set
			{
				_foreground = value;
				OnPropertyChanged(() => Background);
			}
		}

		public ObservableCollection<Subverse> SubVerses
		{
			get { return _subVerses; }
			set
			{
				_subVerses = value;
				OnPropertyChanged(() => SubVerses);
			}
		}

		private int _timeSignatureId;

		public int TimeSignature_Id
		{
			get { return _timeSignatureId; }
			set
			{
				_timeSignatureId = value;
				OnPropertyChanged(() => TimeSignature_Id);

				var timeSignature = (from a in TimeSignatures.TimeSignatureList
									 where a.Id == _timeSignatureId
									 select a.Name).First();

				if (string.IsNullOrEmpty(timeSignature))
				{
					timeSignature =
						(from a in TimeSignatures.TimeSignatureList
						 where a.Id == Preferences.DefaultTimeSignatureId
						 select a.Name).First();
				}

				DurationManager.Bpm = Int32.Parse(timeSignature.Split(',')[0]);
				DurationManager.BeatUnit = Int32.Parse(timeSignature.Split(',')[1]);
				DurationManager.Initialize();
				_starttime = (Measure.Index) * DurationManager.Bpm;
			}
		}

		public Measure Measure
		{
			get { return _measure; }
			set
			{
				_measure = value;
				Background = Preferences.MeasureBackground;
				BarId = Measure.Bar_Id;
				EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Editing);
				Duration = _measure.Duration;
				OnPropertyChanged(() => Measure);
			}
		}

		public ObservableCollection<Span> LocalSpans
		{
			get { return _localSpans; }
			set
			{
				_localSpans = value;
				SpanManager.LocalSpans = value;
				OnPropertyChanged(() => LocalSpans);
			}
		}

		public void SubscribeEvents()
		{
            EA.GetEvent<ArrangeMeasure>().Subscribe(OnArrangeMeasure);
			EA.GetEvent<ShiftChords>().Subscribe(OnShiftChords);
			EA.GetEvent<SetSequenceWidth>().Subscribe(OnSetSequenceWidth);
			EA.GetEvent<FlagMeasure>().Subscribe(OnFlagMeasure);
			EA.GetEvent<UpdateActiveChords>().Subscribe(OnUpdateActiveChords);
			EA.GetEvent<UpdateMeasureBarX>().Subscribe(OnUpdateMeasureBarX);
			EA.GetEvent<HideMeasureEditHelpers>().Subscribe(OnHideMeasureEditHelpers);
			EA.GetEvent<SetPlaybackControlVisibility>().Subscribe(OnSetPlaybackControlVisibility);
			EA.GetEvent<UpdateMeasureBarColor>().Subscribe(OnUpdateMeasureBarColor);
			EA.GetEvent<UpdateMeasureBar>().Subscribe(OnUpdateMeasureBar);
			EA.GetEvent<Backspace>().Subscribe(OnBackspace);
			EA.GetEvent<DeleteTrailingRests>().Subscribe(OnDeleteTrailingRests);
			EA.GetEvent<DeleteEntireChord>().Subscribe(OnDeleteEntireChord);

			EA.GetEvent<SetMeasureBackground>().Subscribe(OnSetMeasureBackground);
			EA.GetEvent<UpdateSpanManager>().Subscribe(OnUpdateSpanManager);
			EA.GetEvent<SpanUpdate>().Subscribe(OnSpanUpdate);
			EA.GetEvent<ResizeMeasure>().Subscribe(OnResizeMeasure, true);
			
			EA.GetEvent<MeasureLoaded>().Subscribe(OnMeasureLoaded);
			if (Measure.Chords.Count > 0 && EditorState.IsOpening)
			{
				EA.GetEvent<NotifyChord>().Subscribe(OnNotifyChord);
			}

			EA.GetEvent<SelectMeasure>().Subscribe(OnSelectMeasure);
			EA.GetEvent<DeSelectMeasure>().Subscribe(OnDeSelectMeasure);
			EA.GetEvent<ApplySubVerse>().Subscribe(OnApplySubVerse);
			EA.GetEvent<ClearVerses>().Subscribe(OnClearVerses);
			EA.GetEvent<BroadcastNewMeasureRequest>().Subscribe(OnBroadcastNewMeasureRequest);
			EA.GetEvent<BumpMeasureWidth>().Subscribe(OnBumpMeasureWidth);
			EA.GetEvent<CommitTransposition>().Subscribe(OnCommitTransposition, true);
			EA.GetEvent<PopEditPopupMenu>().Subscribe(OnPopEditPopupMenu, true);
			EA.GetEvent<ShowSavePanel>().Subscribe(OnShowSavePanel, true);
			EA.GetEvent<ResumeEditing>().Subscribe(OnResumeEditing);
			SubscribeBarEvents();
			SubscribeFooterEvents();
		}

		public void DefineCommands()
		{
			ClickCommand = new DelegatedCommand<object>(OnClick);
			MouseMoveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseMove, null);
			MouseLeaveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeave, null);
			MouseRightButtonUpCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseRightButtonUp, null);
			DefineBarCommands();
			DefineFooterCommands();
		}

		#region Properties, Commands and EventHandlers

		private Visibility _chordSelectorVisiblity = Visibility.Collapsed;
		public Visibility ChordSelectorVisibility
		{
			get { return _chordSelectorVisiblity; }
			set
			{
				_chordSelectorVisiblity = value;
				OnPropertyChanged(() => ChordSelectorVisibility);
			}
		}

		private void AcceptOrRejectAll(_Enum.Disposition disposition)
		{
			foreach (var chord in this.ActiveMeChs)
			{
				foreach (var note in chord.Notes)
				{
					switch (disposition)
					{
						case _Enum.Disposition.Accept:
							EA.GetEvent<AcceptChange>().Publish(note.Id);
							break;
						case _Enum.Disposition.Reject:
							EA.GetEvent<RejectChange>().Publish(note.Id);
							break;
					}
				}
			}
		}

		private void DeleteAll()
		{
			var chords = (from a in Measure.Chords select a).ToList();
			foreach (var chord in chords)
			{
				var notes = (from b in chord.Notes select b).ToList();
				foreach (var note in notes)
				{
					EA.GetEvent<DeleteNote>().Publish(note);
				}
			}
		}


		#endregion

		#region Methods, Properties, Commands and EventHandlers

		private string _bottomChordSelectorMargin;

		private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseRightButtonUpCommand;

		private string _verseMargin;
		private int _width;


		public ExtendedDelegateCommand<ExtendedCommandParameter> MouseRightButtonUpCommand
		{
			get { return _mouseRightButtonUpCommand; }
			set
			{
				_mouseRightButtonUpCommand = value;
				OnPropertyChanged(() => MouseRightButtonUpCommand);
			}
		}

		public int Width
		{
			get { return _width; }
			set
			{
				if (!EditorState.IsComposing)
				{
					var w = (from a in Cache.Measures where a.Sequence == Measure.Sequence select double.Parse(a.Width)).Max();
					_width = (int)w;
				}
				else
				{
					_width = value;
					if (EditorState.IsResizingMeasure)
					{
						// it was possible to drag a bar to the left of the preceding bar which produced a negative width.
						if (_width < 40) _width = 40;
					}
				}
				Measure.Width = _width.ToString(CultureInfo.InvariantCulture);
				//TODO: No longer using MeasureBar_X.
				MeasureBarX = 0;
				OnPropertyChanged(() => Width);
			}
		}

		public string VerseMargin
		{
			get { return _verseMargin; }
			set
			{
				_verseMargin = value;
				OnPropertyChanged(() => VerseMargin);
			}
		}


		public void GetNextPasteTarget()
		{
			// if the content of the clipboard is greater than the remaining s'space' in the target _measure, then
			// this method is called to help determine what _measure the paste should continue in.
			int index = Measure.Index;
			var measure =
				(from a in Cache.Measures where a.Index == index + 1 select a).DefaultIfEmpty(null).Single();
			EA.GetEvent<BroadcastNewMeasureRequest>().Publish(measure);
		}

		public void OnPopEditPopupMenu(Guid id)
		{
			if (id == Measure.Id)
			{
				var pt = new Point(MeasureClick_X + 10 - CompositionManager.XScrollOffset,
					MeasureClick_Y + 10 - CompositionManager.YScrollOffset);
				// var pt = new Point(MeasureClick_X + 10, MeasureClick_Y + 10);
				var payload =
					new Tuple<Point, int, int, double, double, string, Guid>(pt, Measure.Sequence, Measure.Index,
						Measure.Index * DurationManager.Bpm, DurationManager.Bpm, Measure.Width, Measure.Staff_Id);

				EA.GetEvent<SetEditPopupMenu>().Publish(payload);
				EA.GetEvent<UpdateEditPopupMenuTargetMeasure>().Publish(this);
				EA.GetEvent<UpdateEditPopupMenuItemsEnableState>().Publish(string.Empty);
				EA.GetEvent<ShowEditPopupMenu>().Publish(string.Empty);

				SetChordContext();
			}
		}

		public void OnMouseRightButtonUp(ExtendedCommandParameter commandParameter)
		{
			var pt = new Point(MeasureClick_X + 10 - CompositionManager.XScrollOffset,
				MeasureClick_Y + 10 - CompositionManager.YScrollOffset);
			var payload =
				new Tuple<Point, int, int, double, double, string, Guid>(pt, Measure.Sequence, Measure.Index,
					Measure.Index * DurationManager.Bpm, DurationManager.Bpm, Measure.Width, Measure.Staff_Id);

			EA.GetEvent<SetEditPopupMenu>().Publish(payload);
			EA.GetEvent<UpdateEditPopupMenuTargetMeasure>().Publish(this);
			EA.GetEvent<UpdateEditPopupMenuItemsEnableState>().Publish(string.Empty);
			EA.GetEvent<ShowEditPopupMenu>().Publish(string.Empty);
			SetChordContext();
		}

		public void OnResizeMeasure(object obj)
		{
			var payload = (WidthChange)obj;
			if (payload.Sequence != Measure.Sequence) return;
			try
			{
				EditorState.Ratio = 1;
				EditorState.MeasureResizeScope = _Enum.MeasureResizeScope.Composition;
				SetWidth(payload.Width);
				EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
				EA.GetEvent<DeselectAllBars>().Publish(string.Empty);
			}
			catch (Exception ex)
			{
				Exceptions.HandleException(ex);
			}
		}

		private void SetWidth(double width)
		{
			_ratio = 1;
			if (!EditorState.IsOpening)
			{
				_ratio = width / Width * _baseRatio;
                if (this.Measuregroup != null)
				    EA.GetEvent<UpdateMeasureSpacingRatio>().Publish(new Tuple<Guid, Guid, double>(this.Measuregroup.Id, Measure.Id,_ratio));
				_baseRatio = _ratio;
			}
			Width = (int)Math.Floor(width);
		}

		#endregion

		private void SetChordContext()
		{
			ChordManager.Measure = Measure;
		}

		private void SetNotegroupContext()
		{
			NotegroupManager.ChordStarttimes = null;
			NotegroupManager.ChordNotegroups = ChordNotegroups;
			NotegroupManager.Measure = Measure;
			NotegroupManager.Chord = Chord;
		}

		private static void SetActiveMeasureCount()
		{
			// why are we excluding the first m - (a.Index > 0 )?
			EditorState.ActiveMeasureCount =
				(from a in Cache.Measures where ChordManager.GetActiveChords(a).Count > 0 select a)
					.DefaultIfEmpty(null)
					.Count();
		}

		private void UpdateMeasureDuration()
		{
			Duration = (decimal)Convert.ToDouble((from c in this.ActiveMeChs select c.Duration).Sum());
		}

		private double GetRatio()
		{
			double ratio = 1;
			if (EditorState.IsOpening)
			{
				if (this.ActiveMeChs.Count <= 1) return ratio;
				var actualProportionalSpacing = this.ActiveMeChs[1].Location_X - this.ActiveMeChs[0].Location_X;
				double defaultProportionalSpacing =
					DurationManager.GetProportionalSpace((double)this.ActiveMeChs[0].Duration);
				ratio = actualProportionalSpacing / defaultProportionalSpacing;
			}
			else
			{
				ratio = Width / _initializedWidth;
			}
            if (this.Measuregroup != null)
			    EA.GetEvent<UpdateMeasureSpacingRatio>().Publish(new Tuple<Guid, Guid, double>(this.Measuregroup.Id, Measure.Id, ratio));
			return ratio;
		}

		public override void OnMouseLeave(ExtendedCommandParameter param)
		{
			EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
			HideCursor();
			HideLedgerGuide();
			HideMarker();
		}

		public void OnSetSequenceWidth(Tuple<Guid, int, int> payload)
		{
			if (payload.Item2 == Measure.Sequence)
			{
				Width = payload.Item3;
			}
		}

		public void OnFlagMeasure(Guid id)
		{
			if (!IsTargetVM(id)) return;
			_mEcHNgs = NotegroupManager.ParseMeasure(out _chordStartTimes);
			foreach (var st in _chordStartTimes)
			{
				if (!_mEcHNgs.ContainsKey(st)) continue;
				var nGs = _mEcHNgs[st];
				foreach (var nG in nGs)
				{
					if (!NotegroupManager.HasFlag(nG) || NotegroupManager.IsRest(nG)) continue;
					var root = nG.Root;
					root.Vector_Id = (short)DurationManager.GetVectorId((double)root.Duration);
				}
			}
		}

		public void OnToolClick()
		{
			var p = Utilities.CoordinateSystem.TranslateToCompositionCoords
				(
					MeasureClick_X,
					MeasureClick_Y,
					Measure.Sequence,
					Measure.Index,
					(double)_starttime,
					DurationManager.Bpm,
					Measure.Width,
					Measure.Staff_Id
				);

			switch (EditorState.Tool)
			{
				case "SelectArea":
					EditorState.ClickMode = "Click";
					EA.GetEvent<AreaSelect>().Publish(p);
					break;
			}
		}

		public void OnUpdateSpanManager(object obj)
		{
			var id = (Guid)obj;
            if (IsTargetVM(id))
			{
				SpanManager.LocalSpans = LocalSpans;
			}
		}

		public void OnSpanUpdate(object obj)
		{
			var payload = (SpanData)obj;
			if (IsTargetVM(payload.Measure.Id))
			{
				LocalSpans = payload.LocalSpans;
			}
		}

		public void OnSetPlaybackControlVisibility(Guid id)
		{
            if (IsTargetVM(id))
			{
				var mStaff = Utils.GetStaff(Measure.Staff_Id);
				if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple ||
					(EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand && mStaff.Index % 2 == 0))
				{
					PlaybackControlVisibility = (Measure.Chords.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
				}
			}
		}

		public void OnResumeEditing(object obj)
		{
			EditingFooterVisible = Visibility.Visible;
		}

		public void OnShowSavePanel(object obj)
		{
			EditingFooterVisible = Visibility.Collapsed;
		}

		public void OnBackspace(object obj)
		{
            if (IsTargetVM(EditorState.ActiveMeasureId))
			{
				var chords = new ObservableCollection<Chord>(this.ActiveMeChs.OrderByDescending(p => p.StartTime));
				if (chords.Count > 0)
				{
					EA.GetEvent<DeleteEntireChord>().Publish(new Tuple<Guid, Guid>(Measure.Id, chords[0].Notes[0].Id));
					Span();
					if (chords.Count == 1)
					{
						EA.GetEvent<HideMeasureFooter>().Publish(Measure.Id);
					}
				}
			}
		}

		public void OnSetMeasureBackground(Guid id)
		{
			if (id == Guid.Empty)
			{
				Background = Preferences.MeasureBackground;
				EditorState.ActiveMeasureId = Guid.Empty;
			    return;
			}
			if (IsTargetVM(id))
			{
				EA.GetEvent<SetMeasureBackground>().Publish(Guid.Empty);
				Background = Preferences.ActiveMeasureBackground;
				EditorState.ActiveMeasureId = Measure.Id;
			}
		}

		public void OnCommitTransposition(Tuple<Guid, object> payload)
		{
			var state = (TranspositionState)payload.Item2;
			Measure.Key_Id = state.Key.Id;
		}

		private double GetProportionalEndSpace(double endSpace)
		{
			var p = endSpace * _ratio * _baseRatio;
			if (p > Preferences.M_END_SPC)
				p = Preferences.M_END_SPC;
			return p;
		}

		public void OnBumpMeasureWidth(Tuple<Guid, double, int> payload)
		{
			int width;
			if (payload.Item3 != Measure.Sequence) return;
			var endSpace = GetProportionalEndSpace(payload.Item2);
			if (this.ActiveSqChs.Any())
			{
				width = this.LastSqCh.Location_X + (int) Math.Floor(endSpace);
			}
			else
			{
				width = Width + (int)Math.Floor(endSpace);
			}
			EA.GetEvent<SetSequenceWidth>().Publish(new Tuple<Guid, int, int>(Measure.Id, Measure.Sequence, Math.Max(width, Preferences.CompositionMeasureWidth)));
		}

		public void OnDeleteTrailingRests(object obj)
		{
			var nIds = new List<Guid>();
			foreach (var ch in this.ActiveMeChs)
			{
				if (ch.Notes[0].Pitch == "R")
					nIds.Add(ch.Notes[0].Id);
				else
					break;
			}
			foreach (var nId in nIds)
			{
				EA.GetEvent<DeleteEntireChord>().Publish(new Tuple<Guid, Guid>(Measure.Id, nId));
			}
		}

		public void OnDeleteEntireChord(Tuple<Guid, Guid> payload)
		{
            if (!IsTargetVM(payload.Item1)) return;
			var n = Utils.GetNote(payload.Item2);
			if (!CollaborationManager.IsActive(n)) return;
			var cH = Utils.GetChord(n.Chord_Id);
			if (cH == null) return;
			var duration = (from c in cH.Notes select c.Duration).DefaultIfEmpty<decimal>(0).Min();
			DeleteChordNotes(cH);
			RemoveChordFromMeasure(cH, duration);
			AdjustFollowingChords(n, duration);

			EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
			UpdateMeasureDuration();
			SetActiveMeasureCount();
		}

		private void DeleteChordNotes(Chord cH)
		{
			var ids = cH.Notes.Select(n => n.Id).ToList();
			foreach (var id in ids)
			{
				var n = Utils.GetNote(id);
                Repository.Delete(n);
				Cache.Notes.Remove(n);
				cH.Notes.Remove(n);
			}
		}

		private void RemoveChordFromMeasure(Chord ch, decimal chDuration)
		{
			Measure.Chords.Remove(ch);
			Repository.Delete(ch);
			Cache.Chords.Remove(ch);
			Measure.Duration = Math.Max(0, Measure.Duration - chDuration);
		}

		private void AdjustFollowingChords(Note n, decimal chDuration)
		{
			foreach (var cH in this.ActiveMeChs)
			{
				if (cH.Location_X <= n.Location_X) continue;
				cH.StartTime = cH.StartTime - (double)chDuration;
				EA.GetEvent<SynchronizeChord>().Publish(cH);
			}
		}

		public void OnBroadcastNewMeasureRequest(object obj)
		{
			if (obj == null) return;
			var measure = (Measure)obj;
			if (Measure.Index != measure.Index) return;
			// the next paste target _measure (calculated in GetNextPasteTarget()) is sent to the EditPopupMenu ViewModel.
			EA.GetEvent<UpdateEditPopupMenuTargetMeasure>().Publish(this);
			// send 'Paste' command as if it was selected on the Edit Popup Menu (or control-V);
			EA.GetEvent<EditPopupItemClicked>()
				.Publish(new Tuple<string, string, _Enum.PasteCommandSource>(EditActions.Paste, "",
					_Enum.PasteCommandSource.Programmatic));
		}

		public void OnClearVerses(object obj)
		{
			SubVerses = new ObservableCollection<Subverse>();
		}

		public void OnDeSelectMeasure(Guid id)
		{
            if (!IsTargetVM(id)) return;
			FooterSelectAllVisibility = Visibility.Collapsed;
			FooterSelectAllText = "Select";
			foreach (var cH in this.ActiveMeChs)
			{
				EA.GetEvent<DeSelectChord>().Publish(cH.Id);
			}
		}

		public void OnSelectMeasure(Guid id)
		{
            if (!IsTargetVM(id)) return;
			FooterSelectAllVisibility = Visibility.Visible;
			FooterSelectAllText = "Deselect";
			foreach (var cH in this.ActiveMeChs)
			{
				EA.GetEvent<SelectChord>().Publish(cH.Id);
			}
		}

		public void OnNotifyChord(Guid id)
		{
            if (!IsTargetVM(id)) return;
			_loadedChordsCount++;
			if (_loadedChordsCount != Measure.Chords.Count()) return;
			EA.GetEvent<MeasureLoaded>().Publish(Measure.Id);
			EA.GetEvent<NotifyChord>().Unsubscribe(OnNotifyChord);
		}

		private void DistributeLyrics()
		{
			CompositionManager.Composition.Verses.OrderBy(p => p.Index);
			Cache.Verses = CompositionManager.Composition.Verses;
			EA.GetEvent<UpdateVerseIndexes>().Publish(Cache.Verses.Count);
		}

		public override void OnClick(object obj)
		{
			EA.GetEvent<HideEditPopup>().Publish(string.Empty);
			if (Selection.Notes.Any() || Selection.Arcs.Any())
			{
				// there's an active selection, so stop here and use this click to deselect all selected notes
				EA.GetEvent<DeSelectAll>().Publish(string.Empty);
				return;
			}
			// notify the parent staff about the click so the staff can do 
			// whatever it needs to do when a measure is clicked.
			EA.GetEvent<SendMeasureClickToStaff>().Publish(Measure.Staff_Id);
			// remove active _measure status from all Measures
			EA.GetEvent<SetMeasureBackground>().Publish(Guid.Empty);
			// make this m the active _measure
			EA.GetEvent<SetMeasureBackground>().Publish(Measure.Id);

			if (EditorState.DurationSelected())
			{
				EditorState.Duration = (from a in DurationManager.Durations
										where (a.Caption == EditorState.DurationCaption)
										select a.Value).DefaultIfEmpty(Constants.INVALID_DURATION).Single();
				if (ValidPlacement())
				{
					SetChordContext();
					AddNoteToChord();
					// TODO: Why am I updating the provenance panel every time I click a measure?
					EA.GetEvent<UpdateProvenancePanel>().Publish(CompositionManager.Composition);
				}
			}
			else
			{
				// the user clicked with a tool that is not a note or rest. route click to tool dispatcher
				OnToolClick();
			}
			EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
			UpdateMeasureDuration();
			SetActiveMeasureCount();
		}

		private bool AddNoteToChord(out Note n)
		{
			n = NoteController.Create(Chord, Measure, MeasureClick_Y);
			if (n == null) return true;
			Chord.Notes.Add(n);
			Cache.Notes.Add(n);
			return false;
		}

		public Chord AddNoteToChord()
		{
			ChordManager.ActiveChords = this.ActiveMeChs;
			var placementMode = GetPlacementMode(out LeftChord, out RightChord);
			Chord = ChordManager.GetOrCreate(Measure.Id, placementMode, LeftChord);
			//TODO: this returned chord may be inactive. if so, activate it. see ChordManager.GetOrCreate
			if (Chord != null)
			{
				Note n;
				if (AddNoteToChord(out n)) return null;
				var notegroup = GetNotegroup(n);
				if (notegroup != null)
				{
					n.Orientation = notegroup.Orientation;
					var activeNotes = ChordManager.GetActiveNotes(Chord.Notes);
					if (activeNotes.Count == 1)
					{
						if (Chord.Notes.Count == 1)
						{
							AddChordToMeasure();
						}
						double threshholdStarttime = (double)Chord.StartTime;
						//if (placementMode == _Enum.NotePlacementMode.Insert)
						//{
						//	//it's not necessary to update chords with starttimes less than the current chord
						//	threshholdStarttime = (double) Chord.StartTime;
						//}
						EA.GetEvent<SetThreshholdStarttime>().Publish(new Tuple<Guid, double>(Measuregroup.Id, threshholdStarttime));
						Chord.Location_X = GetChordXCoordinate(placementMode, Chord);
						Measure.Duration = (decimal)Convert.ToDouble((from c in this.ActiveMeChs select c.Duration).Sum());
                        Repository.Update(Measure);
						EA.GetEvent<UpdateMeasurePackState>().Publish(new Tuple<Guid, _Enum.EntityFilter>(Measure.Id, _Enum.EntityFilter.Measure));
					}
					n.Location_X = Chord.Location_X;
				}
			}
			if (Chord != null && Chord.Duration < 1)
			{
				Span();
			}
			EA.GetEvent<ArrangeArcs>().Publish(Measure);
			EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Editing);
            //EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
			return Chord;
		}

		private void AddChordToMeasure()
		{
			Measure.Chords.Add(Chord);
			Cache.Chords.Add(Chord);
			Statistics.UpdateCompositionMeasureStatistics(Chord.Measure_Id);
		}

		private Notegroup GetNotegroup(Note n)
		{
			SetNotegroupContext();
			ChordNotegroups = NotegroupManager.ParseChord();
			SetNotegroupContext();
			return NotegroupManager.GetNotegroup(n);
		}

		public bool ValidPlacement()
		{
			var result = true;
			try
			{
				var isFullMeasure = MeasureManager.IsPacked(Measure);
				var isAddingToChord = (EditorState.Chord != null);
				if (EditorState.Duration != Constants.INVALID_DURATION)
				{
					if (EditorState.Duration == null) return false;
					result = (!isFullMeasure || isAddingToChord) &&
							 (Duration + (decimal)EditorState.Duration <= DurationManager.Bpm || isAddingToChord);
				}
			}
			catch (Exception ex)
			{
				result = false;
				Exceptions.HandleException(ex);
			}
			return result;
		}

		private static bool CheckAllActiveMeasuresLoaded()
		{
			EditorState.LoadedActiveMeasureCount++;
			return EditorState.ActiveMeasureCount <= EditorState.LoadedActiveMeasureCount;
		}

		public void OnMeasureLoaded(Guid id)
		{
			if (IsTargetVM(id))
			{
				if (this.ActiveMeChs.Any())
				{
					EA.GetEvent<SetPlaybackControlVisibility>().Publish(Measure.Id);
					if (CheckAllActiveMeasuresLoaded())
					{
                        OnAllMeasuresLoaded();
					}
                    if (Measuregroup != null)
						EA.GetEvent<RespaceMeasuregroup>().Publish(Measuregroup.Id);
					EA.GetEvent<BumpMeasureWidth>().Publish(new Tuple<Guid, double, int>(Measure.Id, Preferences.M_END_SPC, Measure.Sequence));
					Span();
				}
			}
		}

		private void OnAllMeasuresLoaded()
		{
			EA.GetEvent<SetSocialChannels>().Publish(string.Empty);
			EA.GetEvent<SetRequestPrompt>().Publish(string.Empty);
            EA.GetEvent<ArrangeMeasure>().Publish(Measure.Id);
            EA.GetEvent<ResumeEditing>().Publish(string.Empty);
			CheckCompositionReadyState();
		}

        public void OnArrangeMeasure(Guid id)
        {
            if (!IsTargetVM(id)) return;
            DistributeArcs();
            DistributeLyrics();
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            EA.GetEvent<AdjustBracketHeight>().Publish(string.Empty);
            EA.GetEvent<ArrangeArcs>().Publish(Measure);
            EA.GetEvent<ArrangeVerse>().Publish(Measure);
            Span();
        }

		public _Enum.NotePlacementMode GetPlacementMode(out Chord ch1, out Chord ch2)
		{
			var clickX = MeasureClick_X + Finetune.Measure.ClickNormalizerX;
			var mode = GetAdjacentChords(out ch1, out ch2, clickX);
			return mode;
		}

		public _Enum.NotePlacementMode GetAdjacentChords(out Chord ch1, out Chord ch2, int x)
		{
			ch1 = null;
			ch2 = null;
			var locX1 = Defaults.MinusInfinity;
			var locX2 = Defaults.PlusInfinity;
			var mode = _Enum.NotePlacementMode.Append;

			if (!this.ActiveMeChs.Any()) return mode;
			ch1 = this.ActiveMeChs[0];

			for (var i = 0; i < this.ActiveMeChs.Count - 1; i++)
			{
				var ach1 = this.ActiveMeChs[i];
				var ach2 = this.ActiveMeChs[i + 1];

				if (x > ach1.Location_X && x < ach2.Location_X)
				{
					ch1 = ach1;
					ch2 = ach2;
					mode = _Enum.NotePlacementMode.Insert;
				}
				if (x > ach1.Location_X && ach1.Location_X > locX1)
				{
					ch1 = ach1;
					locX1 = ach1.Location_X;
				}
				if (x < ach2.Location_X && ach2.Location_X < locX2)
				{
					ch2 = ach2;
					locX2 = ach2.Location_X;
				}
			}
			return mode;
		}

		private int GetChordXCoordinate(_Enum.NotePlacementMode mode, Chord ch)
		{
			var locX = 0;
			var spacing = DurationManager.GetProportionalSpace();
			switch (mode)
			{
				case _Enum.NotePlacementMode.Insert:
					if (LeftChord != null && RightChord != null)
					{
						locX = LeftChord.Location_X + spacing;
						ch.Location_X = locX;
						ch.StartTime = RightChord.StartTime;
						foreach (var ach in this.ActiveMeChs)  //no need to filter m.chs using GetActiveChords(). 
						{
							if (ach.Location_X > LeftChord.Location_X && ch != ach)
							{
								ach.Location_X += spacing;
								if (ach.StartTime != null) ach.StartTime = (double)ach.StartTime + (double)ch.Duration;
                                Repository.Update(ach);
							}
							EA.GetEvent<SynchronizeChord>().Publish(ach);
						}
					}
					break;
				case _Enum.NotePlacementMode.Append:
					var a = (from c in this.ActiveMeChs where c.StartTime < Chord.StartTime select c.Location_X);
					var e = a as List<int> ?? a.ToList();
					locX = (!e.Any()) ? Infrastructure.Constants.Measure.Padding : Convert.ToInt32(e.Max()) + spacing;
					break;
			}
			return locX;
		}

		public void OnShiftChords(Tuple<Guid, int, double, int, Guid> payload)
        {
            //TODO: Sequence is not the correct scope here. Add a staffgroup filter.
			if (Measure.Sequence != payload.Item2 || IsTargetVM(payload.Item1)) return;
			var st = payload.Item3;
			var startX = payload.Item4;
			Chord prevChord = null;
			var excludeChId = payload.Item5;
			foreach (var ch in this.ActiveMgChs)
			{
				if (ch.StartTime >= st)
				{
					ch.Location_X = (prevChord == null) ? startX : ChordManager.GetProportionalLocationX(prevChord, _ratio);
					EA.GetEvent<SynchronizeChord>().Publish(ch);
					prevChord = ch;
				}
			}
		}

		private void CheckCompositionReadyState()
		{
			if (EditorState.ComposeReadyState == 1)
			{
				EditorState.ComposeReadyState = 2;
				EditorState.IsComposing = true;
				EditorState.IsOpening = false;
			}
		}

		private void DistributeArcs()
		{
			if (CompositionManager.Composition.Arcs.Count <= 0) return;
			if (EditorState.ArcsLoaded) return;
			EA.GetEvent<BroadcastArcs>().Publish(CompositionManager.Composition.Arcs);
			EditorState.ArcsLoaded = true;
		}

		private void Span()
		{
			SpanManager.LocalSpans = LocalSpans;
			EA.GetEvent<SpanMeasure>().Publish(Measure.Id);
		}

		public void OnApplySubVerse(Tuple<object, int, int, Guid, int, int> payload)
		{
			var id = payload.Item4;
			if (IsTargetVM(id))
			{
				var words = (IEnumerable<Word>)payload.Item1;
				var index = payload.Item3;
				var v = new Subverse(index, id.ToString())
				{
					Words = words,
					VerseText = string.Empty,
					Disposition = payload.Item5
				};

				if (SubVerses == null)
					SubVerses = new ObservableCollection<Subverse>();

				SubVerses.Add(v);

				var sv = new List<Subverse>();
				sv.AddRange(SubVerses.OrderBy(i => i.Index));
				SubVerses = new ObservableCollection<Subverse>(sv);

				// verseCount required for trestleHeight calculation;
				EditorState.VerseCount = CompositionManager.Composition.Verses.Count;
				// force bind to new trestleHeight value by setting EmptyBind to anything.
				EmptyBind = DateTime.Now.ToString(CultureInfo.InvariantCulture);
			}
		}

        public bool IsTargetVM(Guid Id)
        {
            return this.Measure.Id == Id;
        }
    }
}