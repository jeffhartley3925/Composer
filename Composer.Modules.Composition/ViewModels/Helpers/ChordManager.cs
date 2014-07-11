using System;
using System.Linq;
using Composer.Infrastructure;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Client;

namespace Composer.Modules.Composition.ViewModels
{
    public static class ChordManager
    {
        public static Guid SelectedChordId = Guid.Empty;
        private static DataServiceRepository<Repository.DataService.Composition> _repository;
        public static Chord Chord { get; set; }
        public static Measure Measure { get; set; }
        private static IEventAggregator _ea;
        public static ObservableCollection<Chord> ActiveChords;

        static ChordManager()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
                SubscribeEvents();
            }
        }

        private static void SubscribeEvents()
        {

        }

        public static int GetProportionalSpacing(Chord ch, double ratio)
        {
            var spacing = DurationManager.GetProportionalSpace((double)ch.Duration);
            return (int)(Math.Ceiling(spacing * ratio));
        }

        public static int GetProportionalLocationX(Chord ch, double ratio)
        {
            var spacing = GetProportionalSpacing(ch, ratio);
            return ch.Location_X + spacing;
        }

		public static int GetAveragedLocationX(Chord ch1, Chord ch2, double ratio)
		{
            var spacing = ChordManager.GetProportionalSpacing(ch1, ratio);
            return ch1.Location_X + spacing;
			//return (int)(Math.Floor(ch1.Location_X + (((ch2.Location_X) - ch1.Location_X) / 2)));
		}

        public static ObservableCollection<Chord> GetActiveChords(Measure m)
        {
            return GetActiveChords(m.Chords, CollaborationManager.GetCurrentAsCollaborator());
        }

        public static ObservableCollection<Chord> GetActiveChords(Measure m, Collaborator collaborator)
        {
            return GetActiveChords(m.Chords, collaborator);
        }

        public static ObservableCollection<Chord> GetActiveChords(DataServiceCollection<Chord> chs)
        {
            return GetActiveChords(chs, CollaborationManager.GetCurrentAsCollaborator());
        }

        public static ObservableCollection<Chord> GetActiveChords(DataServiceCollection<Chord> chs, Collaborator collaborator)
        {
            return new ObservableCollection<Chord>((
                from ch in chs
                where CollaborationManager.IsActive(ch, collaborator)
                select ch).OrderBy(p => p.StartTime));
        }

        public static ObservableCollection<Note> GetActiveNotes(DataServiceCollection<Note> ns)
        {
            return new ObservableCollection<Note>((
                from n in ns
                where CollaborationManager.IsActive(n)
                select n).OrderBy(p => p.StartTime));
        }

        public static decimal SetDuration(Chord ch)
        {
            var n = GetActiveNotes(ch.Notes);
            var a = (from c in n select c.Duration);
            var e = a as List<decimal> ?? a.ToList();
            return (!e.Any()) ? ch.Duration : e.Min();
        }

        public static double GetChordStarttime(double mSt, _Enum.NotePlacementMode placementMode, Chord leftChord)
        {
            double result = 0;
            switch (placementMode)
            {
                case _Enum.NotePlacementMode.Append :
                    var d = Convert.ToDouble((from c in ActiveChords select c.Duration).Sum());
                    result = d + mSt;
                    break;
                case _Enum.NotePlacementMode.Insert:
                    if (leftChord.StartTime != null)
                        if (EditorState.Duration != null)
                            result = (double)leftChord.StartTime + (double)EditorState.Duration;
                    break;
            }
            return result;
        }

        /// <summary>
        /// if click was in line with existing chord, return the chord, otherwise....
        /// check if there is a inactive chord at the same starttime. if so return the chord, then activate it, otherwise....
        /// create and return a new chord entity
        /// </summary>
        /// <param name="mId"></param>
        /// <param name="placementMode"></param>
        /// <param name="leftChord"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static Chord GetOrCreate(Guid mId, _Enum.NotePlacementMode placementMode, Chord leftChord)
        {
            Chord ch;
            if (EditorState.Chord != null)
            {
                // click was in line with existing chord. the note will be addedto this chord.
                // the chord duration should be set to the minimum duration of its notes.
                if (EditorState.Duration != null && (decimal)EditorState.Duration < EditorState.Chord.Duration)
                {
                    EditorState.Chord.Duration = (decimal)EditorState.Duration;
                }
                return EditorState.Chord;
            }
            var mStaffgroup = Utils.GetStaffgroup(Measure);
            var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            var mStarttime = ((Measure.Index % mDensity) * DurationManager.Bpm) + (mStaffgroup.Index * mDensity * DurationManager.Bpm); //TODO: this can move out of here, since its a constant.

            var chStarttime = GetChordStarttime(mStarttime, placementMode, leftChord);
            // is there an inactive chord with the same starttime?
            var a = (from b in Cache.Chords where b.StartTime == chStarttime && EditorState.ActiveMeasureId == b.Measure_Id && !CollaborationManager.IsActive(b) select b);
            var e = a as List<Chord> ?? a.ToList();
            if (e.Any())
            {
                ch = e.First();
				if (ch.Notes.Count == 0)
				{
					// a chord with no notes - this actually happenned.
					// TODO: Delete the chord? better yet, set status to inert? handle with database constraint?
					throw new Exception("Wow, a chord with no notes!");
				}
				else
				{
                    //TODO: need to activate the chord
					EditorState.Chord = ch;
					if (EditorState.Duration != null) ch.Duration = (decimal)EditorState.Duration;
				}

            }
            else
            {
                ch = AddChord(mId, chStarttime);
            }
            return ch;
        }

	    public static Chord AddChord(Guid pId, double chStarttime)
	    {
		    Chord ch;
		    ch = _repository.Create<Chord>();
		    ch.Id = Guid.NewGuid();
		    if (EditorState.Duration != null) ch.Duration = (decimal) EditorState.Duration;
		    ch.StartTime = chStarttime;
		    ch.Measure_Id = pId;
		    ch.Audit = GetAudit();
		    ch.Status = CollaborationManager.GetBaseStatus();
		    return ch;
	    }

	    public static Audit GetAudit()
        {
            var audit = new Audit
            {
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                Author_Id = Current.User.Id,
                CollaboratorIndex = 0
            };
            return audit;
        }

        public static Chord Clone(Measure m, Chord ch)
        {
            Chord o = null;
            try
            {
                Measure = m;
                EditorState.Duration = (double)ch.Duration;
                if (ch.StartTime != null)
                {
                    o = GetOrCreate(m.Id, _Enum.NotePlacementMode.None, null);
                    o.Id = Guid.NewGuid();
                    o.Measure_Id = m.Id;
                    o.Duration = ch.Duration;
                    o.Key_Id = ch.Key_Id;
                    o.Location_X = ch.Location_X;
                    o.Location_Y = ch.Location_Y;
                    o.Audit = GetAudit();
                    o.StartTime = ch.StartTime;
                    o.Status = CollaborationManager.GetBaseStatus();
                    var idx = 0;
                    foreach (var sourceNote in ch.Notes.Select(n => NoteController.Clone(o.Id, ch, Measure, o.Location_X + (idx * 16), n.Location_Y, n)))
                    {
                        o.Notes.Add(sourceNote);
                        idx++;
                    }
                    Cache.Chords.Add(o);
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return o;
        }

        public static ObservableCollection<Chord> GetActiveChords(IEnumerable<Chord> sequenceChords)
        {
            return GetActiveChords(sequenceChords);
        }
    }
}