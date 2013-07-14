using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;

namespace Composer.Infrastructure.Support
{
    public static class Selection
    {
        private static IEventAggregator _ea;

        public static void Clear()
        {
            RemoveAll();
            ImpactedMeasures = new List<Measure>();
            ImpactedChords = new List<Chord>();
        }

        public static void AddNote(Note n)
        {
            var l = Notes;
            if (!l.Contains(n))
            {
                l.Add(n);
            }
            Notes = l;
        }

        public static void RemoveNote(Note n)
        {
            var l = Notes;
            if (l.Contains(n))
            {
                l.Remove(n);
            }
            Notes = l;
        }

        public static void RemoveAll()
        {
            Notes = new List<Note>();
        }

        private static List<Note> _notes;
        public static List<Note> Notes
        {
            get { return _notes ?? (_notes = new List<Note>()); }
            set
            {
                _notes = value;
                if (_notes != null)
                {
                    SetImpactedChords(_notes);
                }
            }
        }

        private static List<Arc> _arcs = new List<Arc>();
        public static List<Arc> Arcs
        {
            get { return _arcs; }
            set
            {
                _arcs = value;
            }
        }

        private static List<Chord> _impactedChords = new List<Chord>();
        public static List<Chord> ImpactedChords
        {
            get { return _impactedChords ?? (_impactedChords = new List<Chord>()); }
            set
            {
                _impactedChords = value;
                if (_impactedChords != null)
                {
                    SetImpactedMeasures(_impactedChords);
                }
            }
        }

        private static List<Measure> _impactedMeasures = new List<Measure>();
        public static List<Measure> ImpactedMeasures
        {
            get { return _impactedMeasures ?? (_impactedMeasures = new List<Measure>()); }
            set
            {
                _impactedMeasures = value;
                if (_impactedMeasures != null)
                {
                    LoadEventAggregator();
                    _ea.GetEvent<SetMeasureBackground>().Publish(_impactedMeasures.Count == 1
                                                                     ? _impactedMeasures[0].Id
                                                                     : Guid.Empty);
                }
            }
        }

        private static void SetImpactedChords(List<Note> l)
        {
            var p = new List<Chord>();
            if (l != null)
            {
                if (l.Count > 0)
                {
                    foreach (var t in l)
                    {
                        var b = (from a in Cache.Chords where a.Id == t.Chord_Id select a);
                        if (b.Any())
                        {
                            var c = b.Single();
                            if (! p.Contains(c))
                            {
                                p.Add(c);
                            }
                        }
                    }
                }
            }
            ImpactedChords = p;
        }

        private static void SetImpactedMeasures(List<Chord> l)
        {
            var p = new List<Measure>();
            if (l != null)
            {
                if (l.Count > 0)
                {
                    foreach (var t in l)
                    {
                        var b = (from a in Cache.Measures where a.Id == t.Measure_Id select a);
                        if (b.Any())
                        {
                            var c = b.Single();
                            if (!p.Contains(c))
                            {
                                p.Add(c);
                            }
                        }
                    }
                }
            }
            ImpactedMeasures = p;
        }

        private static void LoadEventAggregator()
        {
            if (_ea == null)
            {
                _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            }
        }

        public static void Delete()
        {
            LoadEventAggregator();
            foreach (Note note in Notes)
            {
                _ea.GetEvent<DeleteNote>().Publish(note);
            }
            RemoveAll();
        }

        public static bool Exists()
        {
            return Notes.Count > 0;
        }

        public static void Reverse()
        {
            LoadEventAggregator();
            _ea.GetEvent<ReverseSelectedNotes>().Publish("");
        }

        public static void SetAccidental(_Enum.Accidental accidental)
        {
            LoadEventAggregator();
            foreach (Note note in Notes)
            {
                var p = new Tuple<_Enum.Accidental, Note>(accidental, note);
                _ea.GetEvent<SetAccidental>().Publish(p);
            }
        }
    }
}
