using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public static class NotegroupManager
    {
        public static IEventAggregator Ea;
        public static decimal[] ChordStarttimes;
        public static decimal[] ChordActiveTimes;
        public static decimal[] ChordInactiveTimes;
        public static List<Notegroup> ChordNotegroups { get; set; }

        public static Dictionary<decimal, List<Notegroup>> MeasureChordNotegroups;
        private static short? _previousOrientation;

        public static void SetMeasureChordNotegroups()
        {
            MeasureChordNotegroups = ParseMeasure(out ChordStarttimes, out ChordInactiveTimes);
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

        public static Notegroup GetNotegroup(Note note)
        {
            Notegroup noteGroup = null;
            var b = (from a in ChordNotegroups where a.Duration == note.Duration select a);
            var notegroups = b as List<Notegroup> ?? b.ToList();
            if (notegroups.Any())
            {
                noteGroup = notegroups.First();
            }
            return noteGroup;
        }

        public static List<Notegroup> ParseChord()
        {
            var noteGroups = new List<Notegroup>();
            if (Chord == null) return null;

            foreach (var note in Chord.Notes)
            {
                if (note.Pitch == Infrastructure.Constants.Defaults.RestSymbol)
                {
                    var ng = CreateNotegroup(note, Chord);
                    if (ng != null)
                    {
                        ng.IsRest = true;
                        noteGroups.Add(ng);
                    }
                }
                else
                {
                    if (CollaborationManager.IsActionable(note, null))
                    {
                        bool bFound = false;
                        foreach (var notegroup in noteGroups)
                        {
                            if (notegroup.Duration == note.Duration && notegroup.Orientation != (int)_Enum.Orientation.Rest)
                            {
                                notegroup.Notes.Add(note);
                                notegroup.GroupY = notegroup.Root.Location_Y;
                                bFound = true;
                                break;
                            }
                        }
                        if (!bFound)
                        {
                            noteGroups.Add(CreateNotegroup(note, Chord));
                        }
                    }
                }
            }
            _previousOrientation = null;
            return noteGroups;
        }

        private static Notegroup CreateNotegroup(Note note, Chord chord)
        {
            if (chord.StartTime != null)
            {
                return new Notegroup(note.Duration, (Double)chord.StartTime, GetOrientation(note),
                                     Collaborations.GetStatus(note), note, chord);
            }
            return null;
        }

        private static short GetOrientation(Note note)
        {
            short orientation;
            if (note.Pitch == Infrastructure.Constants.Defaults.RestSymbol)
            {
                orientation = (short)_Enum.Orientation.Rest;
            }
            else if (_previousOrientation == null && note.Orientation != null)
            {
                orientation = (short)note.Orientation;
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
            Notegroup noteGroup = null;
            try
            {
                foreach (var n in chord.Notes.Where(_note => CollaborationManager.IsActive(_note)).Where(_note => _note.Duration == note.Duration))
                {
                    if (noteGroup == null)
                    {
                        if (chord.StartTime != null && n.Orientation != null)
                        {
                            noteGroup = new Notegroup(n.Duration, (Double)chord.StartTime,
                                                      (short)n.Orientation) { IsRest = n.Pitch.Trim().ToUpper() == Infrastructure.Constants.Defaults.RestSymbol };
                            noteGroup.Notes.Add(n);
                        }
                    }
                    else
                    {
                        noteGroup.Notes.Add(n);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return noteGroup;
        }

        public static Dictionary<decimal, List<Notegroup>> ParseMeasure(out decimal[] ChordStarttimes, out decimal[] ChordInactiveTimes, out decimal[] ChordActiveTimes, int allChordCnt)
        {
            //this overload adds every chords starttime into ChordStartTimes, not just Actionable chord starttimes.
            //TODO merge this overload with one below. easier said than done.

            int inactiveChordCnt = 0;
            int activeChordCnt = 0;

            ChordStarttimes = new decimal[allChordCnt];
            ChordInactiveTimes = new decimal[inactiveChordCnt]; //"new decimal[0]" stops errors when sorting, and null checks all over the place. it's actual size is set below
            ChordActiveTimes = new decimal[activeChordCnt];
            var measureNoteGroups = new Dictionary<decimal, List<Notegroup>>();

            try
            {
                int allChordIndex = 0;
                int activeChordIndex = 0;
                int inactiveChordIndex = 0;
                var chords = ChordManager.GetActiveChords(Measure.Chords);
                foreach (var chord in chords)
                {
                    if (chord.StartTime != null)
                    {
                        var startTime = (decimal)chord.StartTime;
                        if (!measureNoteGroups.ContainsKey(startTime))
                        {
                            Chord = chord;
                            var notegroup = ParseChord();
                            if (notegroup != null)
                            {
                                measureNoteGroups.Add(startTime, notegroup);
                            }
                            ChordStarttimes[allChordIndex] = startTime;
                            if (CollaborationManager.IsActive(chord))
                            {
                                activeChordCnt++;
                            }
                            else
                            {
                                inactiveChordCnt++;
                            }
                            allChordIndex++;
                        }
                    }
                }

                ChordActiveTimes = new decimal[activeChordCnt];
                ChordInactiveTimes = new decimal[inactiveChordCnt];
                measureNoteGroups = new Dictionary<decimal, List<Notegroup>>();
                foreach (var chord in chords)
                {
                    if (chord.StartTime != null)
                    {
                        var startTime = (decimal)chord.StartTime;
                        if (!measureNoteGroups.ContainsKey(startTime))
                        {
                            Chord = chord;
                            List<Notegroup> notegroup = ParseChord();
                            if (notegroup != null)
                            {
                                measureNoteGroups.Add(startTime, notegroup);
                            }
                            if (!CollaborationManager.IsActive(chord))
                            {
                                ChordInactiveTimes[inactiveChordIndex] = startTime;
                                inactiveChordIndex++;
                            }
                            else
                            {
                                ChordActiveTimes[activeChordIndex] = startTime;
                                activeChordIndex++;
                            }
                        }
                    }
                }
                Array.Sort(ChordStarttimes);
                Array.Sort(ChordActiveTimes);
                Array.Sort(ChordInactiveTimes);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return measureNoteGroups;
        }

        public static Dictionary<decimal, List<Notegroup>> ParseMeasure(out decimal[] ChordStarttimes, out decimal[] ChordInactiveTimes)
        {
            ChordStarttimes = null;
            ChordInactiveTimes = null;
            var measureNoteGroups = new Dictionary<decimal, List<Notegroup>>();

            try
            {
                var index = 0;
                var chordCnt = GetActionableChordCount();
                ChordStarttimes = new decimal[chordCnt];
                ChordInactiveTimes = new decimal[chordCnt];
                var inactiveChordCnt = 0;
                var chords = ChordManager.GetActiveChords(Measure.Chords);
                foreach (Chord chord in chords)
                {
                    if (chord.StartTime != null)
                    {
                        var startTime = (decimal)chord.StartTime;
                        if (!measureNoteGroups.ContainsKey(startTime))
                        {
                            Chord = chord;
                            var notegroup = ParseChord();
                            if (notegroup != null)
                            {
                                measureNoteGroups.Add(startTime, notegroup);
                            }
                            ChordStarttimes[index] = startTime;
                            index++;
                        }
                    }
                }
                index = 0;
                ChordInactiveTimes = new decimal[inactiveChordCnt];
                if (inactiveChordCnt > 0)
                {
                    foreach (var chord in Measure.Chords)
                    {
                        if (chord.StartTime != null)
                        {
                            var startTime = (decimal)chord.StartTime;

                            if (!CollaborationManager.IsActive(chord))
                            {
                                ChordInactiveTimes[index] = startTime;
                                index++;
                            }
                        }
                    }
                    Array.Sort(ChordInactiveTimes);
                }
                Array.Sort(ChordStarttimes);
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