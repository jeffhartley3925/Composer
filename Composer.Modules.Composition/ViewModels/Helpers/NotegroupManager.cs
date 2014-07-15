using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;
using System.Collections.ObjectModel;

namespace Composer.Modules.Composition.ViewModels
{
	public static class NotegroupManager
	{
		public static IEventAggregator Ea;
		public static decimal[] ChordStarttimes;
		public static decimal[] ChordActiveTimes;
		public static decimal[] ChordInactiveTimes;
		public static List<Notegroup> ChordNotegroups { get; set; }
		public static ObservableCollection<Chord> ActiveMChs;
		public static Dictionary<decimal, List<Notegroup>> MeasureChordNotegroups;
		private static short? _previousOrientation;

		public static void SetMeasureChordNotegroups()
		{
			MeasureChordNotegroups = ParseMeasure(out ChordStarttimes);
			ChordNotegroups = ParseChord();
		}

		public static Measure Measure { get; set; }

		public static Chord Chord { get; set; }

		static NotegroupManager()
		{
			Measure = null;
			Chord = null;
			Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
			SubscribeEvents();
		}

		private static void SubscribeEvents()
		{
			Ea.GetEvent<FlagNotegroup>().Subscribe(OnFlag);
		}

		public static Notegroup GetNotegroup(Note n)
		{
			Notegroup ng = null;
			var b = (from a in ChordNotegroups where a.Duration == n.Duration select a);
			var ngs = b as List<Notegroup> ?? b.ToList();
			if (ngs.Any())
			{
				ng = ngs.First();
			}
			return ng;
		}

		public static List<Notegroup> ParseChord()
		{
			var ngs = new List<Notegroup>();
			if (Chord == null) return null;

			foreach (var n in Chord.Notes)
			{
				if (CollaborationManager.IsActive(n))
				{
					if (n.Pitch == Infrastructure.Constants.Defaults.RestSymbol)
					{
						var ng = CreateNotegroup(n, Chord);
						if (ng != null)
						{
							ng.IsRest = true;
							ngs.Add(ng);
						}
					}
					else
					{

						bool bFound = false;
						foreach (var ng in ngs)
						{
							if (ng.Duration == n.Duration && ng.Orientation != (int)_Enum.Orientation.Rest)
							{
								ng.Notes.Add(n);
								ng.GroupY = ng.Root.Location_Y;
								bFound = true;
								break;
							}
						}
						if (!bFound)
						{
							ngs.Add(CreateNotegroup(n, Chord));
						}
					}
				}
			}
			_previousOrientation = null;
			return ngs;
		}

		private static Notegroup CreateNotegroup(Note n, Chord ch)
		{
			if (ch.StartTime != null)
			{
				return new Notegroup(n.Duration, (Double)ch.StartTime, GetOrientation(n),
									 Collaborations.GetStatus(n), n, ch);
			}
			return null;
		}

		private static short GetOrientation(Note n)
		{
			short orientation;
			if (n.Pitch == Infrastructure.Constants.Defaults.RestSymbol)
			{
				orientation = (short)_Enum.Orientation.Rest;
			}
			else if (_previousOrientation == null && n.Orientation != null)
			{
				orientation = (short)n.Orientation;
			}
			else
			{
				orientation = (_previousOrientation == (short)_Enum.Orientation.Up) ? (short)_Enum.Orientation.Down : (short)_Enum.Orientation.Up;
			}
			_previousOrientation = orientation;
			return orientation;
		}

		public static Notegroup ParseChord(Chord chord, Note note)
		{
			Notegroup ng = null;
			try
			{
				foreach (var n in chord.Notes.Where(_note => CollaborationManager.IsActive(_note)).Where(_note => _note.Duration == note.Duration))
				{
					if (ng == null)
					{
						if (chord.StartTime != null && n.Orientation != null)
						{
							ng = new Notegroup(n.Duration, (Double)chord.StartTime,
													  (short)n.Orientation) { IsRest = n.Pitch.Trim().ToUpper() == Infrastructure.Constants.Defaults.RestSymbol };
							ng.Notes.Add(n);
						}
					}
					else
					{
						ng.Notes.Add(n);
					}
				}
			}
			catch (Exception ex)
			{
				Exceptions.HandleException(ex);
			}
			return ng;
		}

		public static Dictionary<double, List<Notegroup>> ParseMeasure(out List<double> chActiveTimes, ObservableCollection<Chord> activeMChs)
		{
			// this overload adds every chords start time into ChordStartTimes, not just Actionable chord start times.
			// TODO merge this overload with one below. easier said than done.
			ActiveMChs = activeMChs;

			chActiveTimes = new List<double>();
			var mNgs = new Dictionary<double, List<Notegroup>>();

			try
			{
				foreach (var cH in ActiveMChs)
				{
					if (cH.StartTime != null)
					{
						var sT = (double)cH.StartTime;
						if (!mNgs.ContainsKey(sT))
						{
							Chord = cH;
							var nG = ParseChord();
							if (nG != null)
							{
								mNgs.Add(sT, nG);
							}
							if (CollaborationManager.IsActive(cH, CollaborationManager.GetCurrentAsCollaborator()))  // we have already filtered all inactive ActiveChords. why is this here?
							{
								chActiveTimes.Add(sT);
							}
						}
					}
				}
				chActiveTimes.Sort();
			}
			catch (Exception ex)
			{
				Exceptions.HandleException(ex);
			}
			return mNgs;
		}

		public static Dictionary<decimal, List<Notegroup>> ParseMeasure(out decimal[] chordStarttimes)
		{
			chordStarttimes = null;
			var measureNoteGroups = new Dictionary<decimal, List<Notegroup>>();

			try
			{
				var index = 0;
				var chordCnt = GetActionableChordCount();
				chordStarttimes = new decimal[chordCnt];
				var chords = ChordManager.GetActiveChords(Measure.Chords);
				foreach (var cH in chords)
				{
					if (cH.StartTime != null)
					{
						var sT = (decimal)cH.StartTime;
						if (!measureNoteGroups.ContainsKey(sT))
						{
							Chord = cH;
							var nG = ParseChord();
							if (nG != null)
							{
								measureNoteGroups.Add(sT, nG);
							}
							chordStarttimes[index] = sT;
							index++;
						}
					}
				}
				Array.Sort(chordStarttimes);
			}
			catch (Exception ex)
			{
				Exceptions.HandleException(ex);
			}
			return measureNoteGroups;
		}

		public static int GetActionableChordCount()
		{
			var cnt = 0;
			var measureNoteGroups = new Dictionary<decimal, decimal>();
			var chords = ChordManager.GetActiveChords(Measure.Chords);
			foreach (var chord in chords)
			{
				if (chord.StartTime != null)
				{
					var startTime = (decimal)chord.StartTime;
					if (!measureNoteGroups.ContainsKey(startTime))
					{
						measureNoteGroups.Add(startTime, startTime);
						cnt++;
					}
				}
			}
			return cnt;
		}

		public static Boolean IsRest(Notegroup notegroup)
		{
			return notegroup.Orientation == (int)_Enum.Orientation.Rest;
		}

		public static Boolean HasFlag(Notegroup notegroup)
		{
			return notegroup.Duration < 1;
		}

		public static void OnFlag(object obj)
		{
			var noteGroup = (Notegroup)obj;
			if (noteGroup != null)
			{
				if (noteGroup.Orientation != (int)_Enum.Orientation.Rest &&
				  !noteGroup.IsSpanned &&
					noteGroup.Duration < 1)
				{
					Ea.GetEvent<RemoveNotegroupFlag>().Publish(noteGroup);
					noteGroup.Root.Vector_Id = (short)DurationManager.GetVectorId((double)noteGroup.Duration);
				}
			}
		}
	}
}