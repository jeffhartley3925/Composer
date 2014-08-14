using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure;
using Composer.Modules.Composition.Models;
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
			Notegroup nG = null;
			var b = (from a in ChordNotegroups where a.Duration == n.Duration select a);
			var nGs = b as List<Notegroup> ?? b.ToList();
			if (nGs.Any())
			{
				nG = nGs.First();
			}
			return nG;
		}

		public static List<Notegroup> ParseChord()
		{
			var nGs = new List<Notegroup>();
			if (Chord == null) return null;

			foreach (var nT in Chord.Notes)
			{
				if (CollaborationManager.IsActive(nT))
				{
					if (nT.Pitch == Infrastructure.Constants.Defaults.RestSymbol)
					{
						var nG = CreateNotegroup(nT, Chord);
						if (nG != null)
						{
							nG.IsRest = true;
							nGs.Add(nG);
						}
					}
					else
					{

						bool bFound = false;
						foreach (var nG in nGs)
						{
							if (nG.Duration == nT.Duration && nG.Orientation != (int)_Enum.Orientation.Rest)
							{
								nG.Notes.Add(nT);
								nG.GroupY = nG.Root.Location_Y;
								bFound = true;
								break;
							}
						}
						if (!bFound)
						{
							nGs.Add(CreateNotegroup(nT, Chord));
						}
					}
				}
			}
			_previousOrientation = null;
			return nGs;
		}

		private static Notegroup CreateNotegroup(Note nT, Chord cH)
		{
			if (cH.StartTime != null)
			{
				return new Notegroup(nT.Duration, (Double)cH.StartTime, GetOrientation(nT),
									 Collaborations.GetStatus(nT), nT, cH);
			}
			return null;
		}

		private static short GetOrientation(Note nT)
		{
			short orientation;
			if (nT.Pitch == Infrastructure.Constants.Defaults.RestSymbol)
			{
				orientation = (short)_Enum.Orientation.Rest;
			}
			else if (_previousOrientation == null && nT.Orientation != null)
			{
				orientation = (short)nT.Orientation;
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
			Notegroup nG = null;
			try
			{
				foreach (var nT in chord.Notes.Where(_note => CollaborationManager.IsActive(_note)).Where(_note => _note.Duration == note.Duration))
				{
					if (nG == null)
					{
						if (chord.StartTime != null && nT.Orientation != null)
						{
							nG = new Notegroup(nT.Duration, (Double)chord.StartTime,
													  (short)nT.Orientation) { IsRest = nT.Pitch.Trim().ToUpper() == Infrastructure.Constants.Defaults.RestSymbol };
							nG.Notes.Add(nT);
						}
					}
					else
					{
						nG.Notes.Add(nT);
					}
				}
			}
			catch (Exception ex)
			{
				Exceptions.HandleException(ex);
			}
			return nG;
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
			var mEcHnGs = new Dictionary<decimal, decimal>();
			var cHs = ChordManager.GetActiveChords(Measure.Chords);
			foreach (var cH in cHs)
			{
				if (cH.StartTime != null)
				{
					var sT = (decimal)cH.StartTime;
					if (!mEcHnGs.ContainsKey(sT))
					{
						mEcHnGs.Add(sT, sT);
						cnt++;
					}
				}
			}
			return cnt;
		}

		public static Boolean IsRest(Notegroup nG)
		{
			return nG.Orientation == (int)_Enum.Orientation.Rest;
		}

		public static Boolean HasFlag(Notegroup nG)
		{
			return nG.Duration < 1;
		}

		public static void OnFlag(object obj)
		{
			var nG = (Notegroup)obj;
			if (nG != null)
			{
				if (nG.Orientation != (int)_Enum.Orientation.Rest &&
				   !nG.IsSpanned &&
					nG.Duration < 1)
				{
					Ea.GetEvent<RemoveNotegroupFlag>().Publish(nG);
					nG.Root.Vector_Id = (short)DurationManager.GetVectorId((double)nG.Duration);
				}
			}
		}
	}
}