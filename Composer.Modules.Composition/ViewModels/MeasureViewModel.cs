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
	public sealed partial class MeasureViewModel : BaseViewModel, IMeasureViewModel
	{
		private IEnumerable<Chord> _mGcHs;
		private readonly int _mDensity = Densities.MeasureDensity;
		public MeasureView View;
		private double _threshholdStarttime = -1;

		#region Fields

		private decimal[] _chordStartTimes;
		private decimal[] _chordInactiveTimes;
		private decimal _starttime;

		private string _addNoteToChordPath = string.Empty;
		private string _background = Preferences.MeasureBackground;

		private double _baseRatio;
		private bool _debugging = false;
		private decimal _duration;
		private string _firstName;
		private string _foreground = Preferences.MeasureForeground;
		private string _imageUrl;
		private double _initializedWidth;
		private string _insertNotePath = string.Empty;
		private string _insertRestPath = string.Empty;
		private bool _isMouseCaptured;
		private string _lastName;
		private int _loadedChordsCount;
		private ObservableCollection<LocalSpan> _localSpans;
		private Measure _measure;

		private Dictionary<decimal, List<Notegroup>> _measureChordNotegroups;
		private double _mouseX;
		private bool _okToResize = true;
		private Visibility _playbackControlVisibility = Visibility.Collapsed;
		private double _ratio;
		private string _replaceNoteWithRestPath = string.Empty;
		private string _replaceRestWithNotePath = string.Empty;
		private DataServiceRepository<Repository.DataService.Composition> _repository;
		private ObservableCollection<Verse> _subVerses;
		public static List<Notegroup> ChordNotegroups { get; set; }
		public static Chord Chord { get; set; }
		private Chord _chord1;
		private Chord _chord2;
		public decimal[] ChordStartTimes;
		public decimal[] ChordInactiveTimes;

		#endregion

		public MeasureViewModel(string id)
		{
			View = null;
			VerseMargin = "8,-5,0,0";
			EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);

			HideSelector();
			var guid = Guid.Parse(id);
			var measure = (from a in Cache.Measures where a.Id == guid select a).DefaultIfEmpty(null).Single();
			if (measure != null)
			{
				EditorState.ResizedMeasureIndexes.Add(measure.Index);
				Measure = measure;
				Width = int.Parse(Measure.Width);
				if (Measure.TimeSignature_Id != null) TimeSignature_Id = (int)Measure.TimeSignature_Id;
			}
			SubscribeEvents();
			DefineCommands();
			_initializedWidth = Width;
			if (Measure.Chords.Count > 0)
			{
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
		}

		public Chord LastMeasureGroupChord { get; set; }

		private IEnumerable<Chord> _activeMeasureGroupChords;
		public IEnumerable<Chord> ActiveMeasureGroupChords
		{
			get { return _activeMeasureGroupChords ?? (_activeMeasureGroupChords = new ObservableCollection<Chord>()); }
			set
			{
				_activeMeasureGroupChords = value;
				_activeMeasureGroupChords = new ObservableCollection<Chord>(_activeMeasureGroupChords.OrderBy(p => p.StartTime));
			}
		}

		public Chord LastSequenceChord { get; set; }

		private IEnumerable<Chord> _activeSequenceChords;
		public IEnumerable<Chord> ActiveSequenceChords
		{
			get { return _activeSequenceChords ?? (_activeSequenceChords = new ObservableCollection<Chord>()); }
			set
			{
				_activeSequenceChords = value;
				_activeSequenceChords = new ObservableCollection<Chord>(_activeSequenceChords.OrderBy(p => p.StartTime));
			}
		}

		private ObservableCollection<Chord> _activeChords;
		public ObservableCollection<Chord> ActiveMeasureChords
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
			ActiveMeasureChords = new ObservableCollection<Chord>((
				from a in Measure.Chords
				where CollaborationManager.IsActive(a)
				select a).OrderBy(p => p.StartTime));
			ActiveSequenceChords = Utils.GetActiveChordsBySequence(Measure.Sequence, Guid.Empty);
			ActiveMeasureGroupChords = Utils.GetMeasureGroupChords(Measure.Id, Guid.Empty, false /* indistinct */);
			LastSequenceChord = (from c in ActiveSequenceChords select c).Last();
			LastMeasureGroupChord = (from c in ActiveMeasureGroupChords select c).Last();
			EA.GetEvent<NotifyActiveChords>().Publish(new Tuple<Guid, object, object, object>(Measure.Id, ActiveMeasureChords, ActiveSequenceChords, ActiveMeasureGroupChords));
		}

		public void OnNotifyActiveChords(Tuple<Guid, object, object, object> payload)
		{
			//TODO: These collections are created here, no need to update them.
			if (Measure.Chords.Count == 0) return;
			var id = payload.Item1;
			if (id != Measure.Id) return;
			ActiveMeasureChords = (ObservableCollection<Chord>)payload.Item2;
			ActiveSequenceChords = (ObservableCollection<Chord>)payload.Item3;
			ActiveMeasureGroupChords = (ObservableCollection<Chord>)payload.Item4;
			LastSequenceChord = (from c in ActiveSequenceChords select c).Last();
			LastMeasureGroupChord = (from c in ActiveMeasureGroupChords select c).Last();
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

		public string ImageUrl
		{
			get { return _imageUrl; }
			set
			{
				_imageUrl = value;
				OnPropertyChanged(() => ImageUrl);
			}
		}

		public string FirstName
		{
			get { return _firstName; }
			set
			{
				_firstName = value;
				OnPropertyChanged(() => FirstName);
			}
		}

		public string LastName
		{
			get { return _lastName; }
			set
			{
				_lastName = value;
				OnPropertyChanged(() => LastName);
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

		public ObservableCollection<Verse> SubVerses
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

		public ObservableCollection<LocalSpan> LocalSpans
		{
			get { return _localSpans; }
			set
			{
				if (value != _localSpans)
				{
					_localSpans = value;
					SpanManager.LocalSpans = value;
					OnPropertyChanged(() => LocalSpans);
				}
			}
		}

		public void SubscribeEvents()
		{
			EA.GetEvent<ShiftChords>().Subscribe(OnShiftChords);
			EA.GetEvent<SetSequenceWidth>().Subscribe(OnSetSequenceWidth);
			EA.GetEvent<AdjustChords>().Subscribe(OnAdjustChords);
			EA.GetEvent<FlagMeasure>().Subscribe(OnFlagMeasure);
			EA.GetEvent<UpdateActiveChords>().Subscribe(OnUpdateActiveChords);
			EA.GetEvent<NotifyActiveChords>().Subscribe(OnNotifyActiveChords);
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
			EA.GetEvent<ResizeSequence>().Subscribe(OnResizeSequence, true);
			EA.GetEvent<RespaceMeasureGroup>().Subscribe(OnRespaceMeasureGroup, true);
			EA.GetEvent<MeasureLoaded>().Subscribe(OnMeasureLoaded);
			if (Measure.Chords.Count > 0 && EditorState.IsOpening)
			{
				EA.GetEvent<NotifyChord>().Subscribe(OnNotifyChord);
			}

			EA.GetEvent<SelectMeasure>().Subscribe(OnSelectMeasure);
			EA.GetEvent<DeSelectMeasure>().Subscribe(OnDeSelectMeasure);
			EA.GetEvent<ApplyVerse>().Subscribe(OnApplyVerse);
			EA.GetEvent<ClearVerses>().Subscribe(OnClearVerses);
			EA.GetEvent<BroadcastNewMeasureRequest>().Subscribe(OnBroadcastNewMeasureRequest);
			EA.GetEvent<BumpMeasureWidth>().Subscribe(OnBumpMeasureWidth);
			EA.GetEvent<CommitTransposition>().Subscribe(OnCommitTransposition, true);
			EA.GetEvent<PopEditPopupMenu>().Subscribe(OnPopEditPopupMenu, true);
			EA.GetEvent<ShowSavePanel>().Subscribe(OnShowSavePanel, true);
			EA.GetEvent<ResumeEditing>().Subscribe(OnResumeEditing);
			EA.GetEvent<UpdateChordsProjection>().Subscribe(OnUpdateChordsProjection);
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
			foreach (var chord in ActiveMeasureChords)
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

		public void OnResizeSequence(object obj)
		{
			var payload = (WidthChangePayload)obj;
			if (payload.MeasureId != Measure.Id) return;
			try
			{
				EditorState.Ratio = 1;
				EditorState.MeasureResizeScope = _Enum.MeasureResizeScope.Composition;

				_threshholdStarttime = -1;
				SetWidth(payload.Width);
				EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
				if (ActiveSequenceChords.Any())
				{
					EA.GetEvent<AdjustChords>().Publish(new Tuple<Guid, bool, Guid>(Measure.Id, payload.IsResizeStartMeasure, Guid.Empty));
					OnArrangeMeasure();
					UpdateProvenanceWidth();
				}

				EA.GetEvent<DeselectAllBars>().Publish(string.Empty);
				EA.GetEvent<ArrangeVerse>().Publish(Measure);
				// TODO: arcs cannot cross staff boundaries, so only raise the following event if this measure is on the same staff as the target measure
				EA.GetEvent<ArrangeArcs>().Publish(Measure);
			}
			catch (Exception ex)
			{
				Exceptions.HandleException(ex);
			}
		}

		private void UpdateProvenanceWidth()
		{
			var s = Utils.GetStaff(Measure.Staff_Id);
			var w = (from a in s.Measures select double.Parse(a.Width)).Sum() +
					Defaults.StaffDimensionWidth + Defaults.CompositionLeftMargin - 70;
			EditorState.GlobalStaffWidth = w;
			EA.GetEvent<SetProvenanceWidth>().Publish(w);
		}

		private void SetWidth(double width)
		{
			_ratio = 1;
			if (!EditorState.IsOpening)
			{
				_ratio = width / Width * _baseRatio;
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
			Duration = (decimal)Convert.ToDouble((from c in ActiveMeasureChords select c.Duration).Sum());
		}

		private double GetRatio()
		{
			double ratio = 1;
			if (EditorState.IsOpening)
			{
				if (ActiveMeasureChords.Count <= 1) return ratio;
				var actualProportionalSpacing = ActiveMeasureChords[1].Location_X - ActiveMeasureChords[0].Location_X;
				double defaultProportionalSpacing =
					DurationManager.GetProportionalSpace((double)ActiveMeasureChords[0].Duration);
				ratio = actualProportionalSpacing / defaultProportionalSpacing;
			}
			else
			{
				ratio = Width / _initializedWidth;
			}
			return ratio;
		}

		public override void OnMouseLeave(ExtendedCommandParameter param)
		{
			EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
			HideCursor();
			HideLedgerGuide();
			HideMarker();
		}

		private void UpdateSpanManager()
		{
			SpanManager.LocalSpans = LocalSpans;
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
			if (id != Measure.Id) return;
			_measureChordNotegroups = NotegroupManager.ParseMeasure(out _chordStartTimes);
			foreach (var st in _chordStartTimes)
			{
				if (!_measureChordNotegroups.ContainsKey(st)) continue;
				var ngs = _measureChordNotegroups[st];
				foreach (var ng in ngs)
				{
					if (!NotegroupManager.HasFlag(ng) || NotegroupManager.IsRest(ng)) continue;
					var root = ng.Root;
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
			if (id == Measure.Id)
			{
				SpanManager.LocalSpans = LocalSpans;
			}
		}

		public void OnSpanUpdate(object obj)
		{
			var payload = (SpanPayload)obj;
			if (payload.Measure.Id == Measure.Id)
			{
				LocalSpans = payload.LocalSpans;
			}
		}

		public void OnSetPlaybackControlVisibility(Guid id)
		{
			if (id == Measure.Id)
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
			if (Measure.Id == EditorState.ActiveMeasureId)
			{
				var chords = new ObservableCollection<Chord>(ActiveMeasureChords.OrderByDescending(p => p.StartTime));
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
			}
			else if (id == Measure.Id)
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
			if (ActiveSequenceChords.Any())
			{
				width = LastSequenceChord.Location_X + (int) Math.Floor(endSpace);
			}
			else
			{
				width = Width + (int)Math.Floor(endSpace);
			}
			EA.GetEvent<SetSequenceWidth>().Publish(new Tuple<Guid, int, int>(Measure.Id, Measure.Sequence, Math.Max(width, Preferences.CompositionMeasureWidth)));
		}

		private void SetRepository()
		{
			if (_repository == null)
			{
				_repository =
					ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
			}
		}

		public void OnDeleteTrailingRests(object obj)
		{
			SetRepository();
			var nIds = new List<Guid>();
			foreach (var ch in ActiveMeasureChords)
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
			SetRepository();
			if (payload.Item1 != Measure.Id) return;
			var n = Utils.GetNote(payload.Item2);
			if (!CollaborationManager.IsActive(n)) return;
			var ch = Utils.GetChord(n.Chord_Id);
			if (ch == null) return;
			var duration = (from c in ch.Notes select c.Duration).DefaultIfEmpty<decimal>(0).Min();
			DeleteChordNotes(ch);
			RemoveChordFromMeasure(ch, duration);
			AdjustFollowingChords(n, duration);

			EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
			UpdateMeasureDuration();
			SetActiveMeasureCount();
		}

		private void DeleteChordNotes(Chord ch)
		{
			var ids = ch.Notes.Select(n => n.Id).ToList();
			foreach (var id in ids)
			{
				var n = Utils.GetNote(id);
				_repository.Delete(n);
				Cache.Notes.Remove(n);
				ch.Notes.Remove(n);
			}
		}

		private void RemoveChordFromMeasure(Chord ch, decimal chDuration)
		{
			Measure.Chords.Remove(ch);
			_repository.Delete(ch);
			Cache.Chords.Remove(ch);
			Measure.Duration = Math.Max(0, Measure.Duration - chDuration);
		}

		private void AdjustFollowingChords(Note n, decimal chDuration)
		{
			foreach (var ch in ActiveMeasureChords)
			{
				if (ch.Location_X <= n.Location_X) continue;
				ch.StartTime = ch.StartTime - (double)chDuration;
				EA.GetEvent<SynchronizeChord>().Publish(ch);
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
			SubVerses = new ObservableCollection<Verse>();
		}

		public void OnDeSelectMeasure(Guid id)
		{
			if (Measure.Id != id) return;
			FooterSelectAllVisibility = Visibility.Collapsed;
			FooterSelectAllText = "Select";
			foreach (var ch in ActiveMeasureChords)
			{
				EA.GetEvent<DeSelectChord>().Publish(ch.Id);
			}
		}

		public void OnSelectMeasure(Guid id)
		{
			if (Measure.Id != id) return;
			FooterSelectAllVisibility = Visibility.Visible;
			FooterSelectAllText = "Deselect";
			foreach (var chord in ActiveMeasureChords)
			{
				EA.GetEvent<SelectChord>().Publish(chord.Id);
			}
		}

		public void OnNotifyChord(Guid id)
		{
			if (id != Measure.Id) return;
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
			SetRepository();
			ChordManager.ActiveChords = ActiveMeasureChords;
			var placementMode = GetPlacementMode(out _chord1, out _chord2);
			Chord = ChordManager.GetOrCreate(Measure.Id, placementMode, _chord1);
			//TODO: this returned chord may be inactive. if so, activate it. see ChordManager.GetOrCreate
			if (Chord != null)
			{
				Note n;
				if (AddNoteToChord(out n)) return null;
				var notegroup = GetNotegroup(n);
				if (notegroup != null)
				{
					n.Orientation = notegroup.Orientation;
					EA.GetEvent<FlagNotegroup>().Publish(notegroup);
					var activeNotes = ChordManager.GetActiveNotes(Chord.Notes);
					if (activeNotes.Count == 1)
					{
						if (Chord.Notes.Count == 1)
						{
							AddChordToMeasure();
						}
						EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);

						if (placementMode == _Enum.NotePlacementMode.Insert)
						{
							//it's not necessary to update chord/notes that come before this one
							if (Chord.StartTime != null) _threshholdStarttime = -1;
						}
						Chord.Location_X = GetChordXCoordinate(placementMode, Chord);
						Measure.Duration = (decimal)Convert.ToDouble((from c in ActiveMeasureChords select c.Duration).Sum());
						_repository.Update(Measure);
						EA.GetEvent<UpdateMeasurePackState>().Publish(new Tuple<Guid, _Enum.EntityFilter>(Measure.Id, _Enum.EntityFilter.Measure));

					}
					n.Location_X = Chord.Location_X;
				}
			}
			if (EditorState.IsCollaboration)
			{
				// if this composition has collaborators, then locations and start times may need to be adjusted.
				// EA.GetEvent<MeasureLoaded>().Publish(Measure.Id);
			}
			if (Chord != null && Chord.Duration < 1)
			{
				Span();
			}
			EA.GetEvent<ArrangeArcs>().Publish(Measure);
			EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Editing);
			return Chord;
		}

		public void OnRespaceMeasureGroup(Tuple<Guid, int?> payload)
		{
			if (payload.Item1 != Measure.Id) return;
			var width = (payload.Item2 == null) ? Width : (int) payload.Item2;
			EA.GetEvent<UpdateChordsProjection>().Publish(new Tuple<Guid, bool, Guid>(Measure.Id, false, Guid.Empty));
			var sg = Utils.GetStaffgroup(Measure);
			var widthChangePayload =
				new WidthChangePayload
				{
					MeasureId = Measure.Id,
					StaffId = Measure.Staff_Id,
					Sequence = Measure.Sequence,
					Width = width,
					StaffgroupId = sg.Id
				};
			EditorState.ResizedMeasureIndexes.Clear();
			MeasureGroup.Resize(widthChangePayload);
		}

		private void AddChordToMeasure()
		{
			Measure.Chords.Add(Chord);
			Cache.Chords.Add(Chord);
			Statistics.Update(Chord.Measure_Id);
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
				var isFullMeasure = MeasureManager.IsPackedForStaffgroup(Measure);
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
			if (Measure.Id == id)
			{
				if (ActiveMeasureChords.Any())
				{
					EA.GetEvent<SetPlaybackControlVisibility>().Publish(Measure.Id);
					if (CheckAllActiveMeasuresLoaded())
					{
						OnArrangeMeasure();
						OnAllMeasuresLoaded();
						EA.GetEvent<ResumeEditing>().Publish(string.Empty);

					}
					EA.GetEvent<AdjustChords>().Publish(new Tuple<Guid, bool, Guid>(Measure.Id, false, Guid.Empty));
					EA.GetEvent<BumpMeasureWidth>().Publish(new Tuple<Guid, double, int>(Measure.Id, Preferences.M_END_SPC, Measure.Sequence));
					Span();
				}
			}
		}

		private void OnAllMeasuresLoaded()
		{
			EA.GetEvent<SetSocialChannels>().Publish(string.Empty);
			EA.GetEvent<SetRequestPrompt>().Publish(string.Empty);
		}

		private void OnArrangeMeasure()
		{
			DistributeLyrics();
			EA.GetEvent<AdjustBracketHeight>().Publish(string.Empty);
			DistributeArcs();
			EA.GetEvent<ArrangeVerse>().Publish(Measure);
			EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
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

			if (!ActiveMeasureChords.Any()) return mode;
			ch1 = ActiveMeasureChords[0];

			for (var i = 0; i < ActiveMeasureChords.Count - 1; i++)
			{
				var ach1 = ActiveMeasureChords[i];
				var ach2 = ActiveMeasureChords[i + 1];

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
					if (_chord1 != null && _chord2 != null)
					{
						locX = _chord1.Location_X + spacing;
						ch.Location_X = locX;
						ch.StartTime = _chord2.StartTime;
						foreach (var ach in ActiveMeasureChords)  //no need to filter m.chs using GetActiveChords(). 
						{
							if (ach.Location_X > _chord1.Location_X && ch != ach)
							{
								ach.Location_X += spacing;
								if (ach.StartTime != null) ach.StartTime = (double)ach.StartTime + (double)ch.Duration;
								_repository.Update(ach);
							}
							EA.GetEvent<SynchronizeChord>().Publish(ach);
						}
					}
					break;
				case _Enum.NotePlacementMode.Append:
					var a = (from c in ActiveMeasureChords where c.StartTime < Chord.StartTime select c.Location_X);
					var e = a as List<int> ?? a.ToList();
					locX = (!e.Any()) ? Infrastructure.Constants.Measure.Padding : Convert.ToInt32(e.Max()) + spacing;
					break;
			}
			return locX;
		}

		public void OnShiftChords(Tuple<Guid, int, double, int, Guid> payload)
		{
			if (Measure.Sequence != payload.Item2 || Measure.Id == payload.Item1) return;
			var st = payload.Item3;
			var startX = payload.Item4;
			Chord prevChord = null;
			var excludeChId = payload.Item5;
			foreach (var ch in ActiveMeasureGroupChords)
			{
				//if (ch.Id == excludeChId) continue;
				if (ch.StartTime >= st)
				{
					ch.Location_X = (prevChord == null) ? startX : ChordManager.GetProportionalLocationX(prevChord, _ratio);
					EA.GetEvent<SynchronizeChord>().Publish(ch);
					prevChord = ch;
				}
			}
		}

		private void SetGlobalStaffWidth()
		{
			// NOTE: not used, this may be related to provenance or print view layout. keep for now.
			var mStaff = Utils.GetStaff(_measure.Staff_Id);
			var mStaffWidth = (from a in mStaff.Measures select double.Parse(a.Width)).Sum() +
							  Defaults.StaffDimensionWidth +
							  Defaults.CompositionLeftMargin - 70;
			EditorState.GlobalStaffWidth = mStaffWidth;
		}

		public sealed class SendChordsProjection : CompositePresentationEvent<Tuple<Guid, List<ChordProjection>>>{ }
		public sealed class UpdateChordsProjection : CompositePresentationEvent<Tuple<Guid, bool, Guid>> { }

		public void OnUpdateChordsProjection(Tuple<Guid, bool, Guid> payload)
		{
			if (payload.Item1 != Measure.Id) return;
			var excludeChId = payload.Item3;
			var isResizeStartMeasure = payload.Item2;
			var excludeM = Measure;
			var activeChSg = Utils.GetStaffgroup(Measure);
			var activeChMSt = ((Measure.Index % _mDensity) * DurationManager.Bpm) +
							  (activeChSg.Index * _mDensity * DurationManager.Bpm);
			_mGcHs =  Utils.GetMeasureGroupChords(Measure.Id, excludeM.Id, false);
			var chordProjections = GetChordsProjection(isResizeStartMeasure, activeChMSt, activeChSg.Index, excludeChId);
			EA.GetEvent<SendChordsProjection>().Publish(new Tuple < Guid, List < ChordProjection >>(Measure.Id, chordProjections));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="isResizeStartMeasure"></param>
		/// <param name="activeChMSt"></param>
		/// <param name="activeChSgIndex"></param>
		/// <param name="excludeChId"></param>
		/// <returns></returns>
		private List<ChordProjection> GetChordsProjection(bool isResizeStartMeasure, int activeChMSt, int activeChSgIndex, Guid excludeChId)
		{
			var cHs = new List<ChordProjection>();
			var sTs = new List<double>();
			foreach (var cH in _mGcHs)
			{
				if (cH.Id == excludeChId) continue;
				var compareChSgIndex = Utils.GetStaffgroup(cH.Measure_Id, true).Index;
				var mIndex = Utils.GetMeasure(cH.Measure_Id).Index;
				if (cH.StartTime == null) continue;
				var normalizedSt = Normalize((double)cH.StartTime, activeChMSt, compareChSgIndex, activeChSgIndex);
				if (isResizeStartMeasure) continue;
				if (!EditorState.ResizedMeasureIndexes.Contains(mIndex)) continue;
				cHs.Add(new ChordProjection(cH.Id, cH.Measure_Id, (double)cH.StartTime, cH.Location_X,
					activeChMSt, cH.Duration, compareChSgIndex, activeChSgIndex));
				sTs.Add(normalizedSt);
			}
			return cHs.OrderByDescending(ch => ch.NormalizedStarttime).ThenByDescending(l => l.LocationX).ToList();
		}

		/// <summary>
		/// when aligning a target chord, we first look for any comparison chord in the measure group with the same starttime, or any
		/// comparison chord in the same measure group with the same 'normalized' starttime. if we find one, we align the target chord with 
		/// the comparison chord. comparison chords that are in the same measure group, but not in the same measure as the target chord
		/// must first have their starttime normalized, otherwise the starttimes won't be comparable.
		/// </summary>
		/// <param name="compareChSt">the raw starttime of a comparison chord in one of the measures in the measuregroup</param>
		/// <param name="targetMSt">the starttime of the measure containing the chord to be aligned</param>
		/// <param name="compareChSgIndex">the staffgroup index of the compare chord staffgroup</param>
		/// <param name="targetChSgIndex">the staffgroup index of the target chord staffgroup</param>
		/// <returns></returns>
		private double Normalize(double compareChSt, double targetMSt, int compareChSgIndex, int targetChSgIndex)
		{
			var result = compareChSt;
			var staffDuration = (DurationManager.Bpm * Densities.MeasureDensity);
			if (compareChSgIndex < targetChSgIndex) result = compareChSt + (targetChSgIndex - compareChSgIndex) * staffDuration;
			else if (compareChSt > targetMSt + DurationManager.Bpm) result = compareChSt - (compareChSgIndex - targetChSgIndex) * staffDuration;
			return result;
		}

		public void OnAdjustChords(Tuple<Guid, bool, Guid> payload)
		{
			if (Measure.Id != payload.Item1) return;
			var excludeChId = payload.Item3;
			var isResizeStartM = payload.Item2;
			var dontSynchMgSpacing = (ActiveMeasureChords.Count() >= ActiveMeasureGroupChords.Count());
			List<double> chordActiveTimes;
			var prevId = Guid.Empty;
			SetNotegroupContext();
			NotegroupManager.ParseMeasure(out chordActiveTimes, ActiveMeasureChords);
			EA.GetEvent<UpdateChordsProjection>().Publish(new Tuple<Guid, bool, Guid>(Measure.Id, false, excludeChId));
			foreach (var sT in chordActiveTimes)
			{
				foreach (var cH in ActiveMeasureChords)
				{
					if (cH.StartTime == sT)
					{
						if (sT > _threshholdStarttime)
						{
							cH.Duration = ChordManager.SetDuration(cH);
							if (Math.Abs(_ratio) < double.Epsilon) _ratio = GetRatio();
							var chordPayload = new Tuple<Guid, Guid, double, bool, bool>(cH.Id, prevId, _ratio, isResizeStartM, dontSynchMgSpacing);
							EA.GetEvent<SetChordLocationAndStarttime>().Publish(chordPayload);
						}
						prevId = cH.Id;
						break;
					}
				}
			}
			var startTime = ActiveMeasureChords.Last().StartTime;
			if (startTime != null) _threshholdStarttime = (double)startTime;
			Span();
			CheckCompositionReadyState();
		}
		/// <summary>
		/// 
		/// </summary>
		private void CheckCompositionReadyState()
		{
			if (EditorState.ComposeReadyState == 1)
			{
				EditorState.ComposeReadyState = 2;
				EA.GetEvent<UpdateChordsProjection>().Publish(new Tuple<Guid, bool, Guid>(Measure.Id, false, Guid.Empty));
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

		public void OnApplyVerse(Tuple<object, int, int, Guid, int, int> payload)
		{
			// Repository.DataService.Verse collection is a member of Repository.DataService.Composition, but we don't 
			// bind to this collection because the binding scope needs to be Repository.DataService._measure. IE: the 
			// storage scope of verses is composition level, but the binding Scope is the _measure. I don't want to 
			// spin up temporary sub-collections of Repository.DataService.Verse objects. Instead, spin up a different 
			// collection of Verses to bind too. This design choice also helps facilitate the projection of verse text 
			// into words (we don't persist words as a separate entity, but each word still has a view, view-model, etc)

			var id = payload.Item4;

			if (id == Measure.Id) // is this the measureViewModel for the target measure?
			{
				var words = (ObservableCollection<Word>)payload.Item1;
				var index = payload.Item3;
				var v = new Verse(index, id.ToString())
				{
					Words = words,
					VerseText = string.Empty,
					Disposition = payload.Item5
				};

				if (SubVerses == null)
					SubVerses = new ObservableCollection<Verse>();

				SubVerses.Add(v);

				var sv = new List<Verse>();
				sv.AddRange(SubVerses.OrderBy(i => i.Index));
				SubVerses = new ObservableCollection<Verse>(sv);

				// verseCount required for trestleHeight calculation;
				EditorState.VerseCount = CompositionManager.Composition.Verses.Count;
				// force bind to new trestleHeight value by setting EmptyBind to anything.
				EmptyBind = DateTime.Now.ToString(CultureInfo.InvariantCulture);
			}
		}
	}
}