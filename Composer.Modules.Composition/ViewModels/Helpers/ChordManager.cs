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

        public static int GetProportionalSpacing(Chord cH, double ratio)
        {
            var spacing = DurationManager.GetProportionalSpace((double)cH.Duration);
            return (int)(Math.Ceiling(spacing * ratio));
        }

        public static int GetProportionalLocationX(Chord cH, double ratio)
        {
            var spacing = GetProportionalSpacing(cH, ratio);
            return cH.Location_X + spacing;
        }

        public static List<Chord> GetActiveChords(Measure mE)
        {
            return GetActiveChords(mE.Chords, CollaborationManager.GetCurrentAsCollaborator());
        }

        public static List<Chord> GetActiveChords(Measure mE, Collaborator collaborator)
        {
            return GetActiveChords(mE.Chords, collaborator);
        }

        public static List<Chord> GetActiveChords(DataServiceCollection<Chord> cHs)
        {
            return GetActiveChords(cHs, CollaborationManager.GetCurrentAsCollaborator());
        }

        public static List<Chord> GetActiveChords(DataServiceCollection<Chord> cHs, Collaborator collaborator)
        {
            return new ObservableCollection<Chord>((
                from cH in cHs
                where CollaborationManager.IsActive(cH, collaborator)
                select cH).OrderBy(p => p.StartTime)).ToList();
        }

        public static ObservableCollection<Note> GetActiveNotes(DataServiceCollection<Note> nTs)
        {
            return new ObservableCollection<Note>((
                from nT in nTs
                where CollaborationManager.IsActive(nT)
                select nT).OrderBy(p => p.StartTime));
        }

        public static decimal SetDuration(Chord ch)
        {
            var nTs = GetActiveNotes(ch.Notes);
            var dU = (from c in nTs select c.Duration);
            var e = dU as List<decimal> ?? dU.ToList();
            return (!e.Any()) ? ch.Duration : e.Min();
        }

        public static double GetChordStarttime(double mEsT, _Enum.NotePlacementMode placementMode, Chord leftChord)
        {
            double result = 0;
            switch (placementMode)
            {
                case _Enum.NotePlacementMode.Append :
                    var dU = Convert.ToDouble((from c in ActiveChords select c.Duration).Sum());
                    result = dU + mEsT;
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
            Chord cH;
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
            var mEsG = Utils.GetStaffgroup(Measure);
            var mEdE = Infrastructure.Support.Densities.MeasureDensity;
            var mEsT = ((Measure.Index % mEdE) * DurationManager.Bpm) + (mEsG.Index * mEdE * DurationManager.Bpm); //TODO: this can move out of here, since its a constant.

            var cHsT = GetChordStarttime(mEsT, placementMode, leftChord);
            // is there an inactive chord with the same starttime?
            var a = (from b in Cache.Chords where b.StartTime == cHsT && EditorState.ActiveMeasureId == b.Measure_Id && !CollaborationManager.IsActive(b) select b);
            var e = a as List<Chord> ?? a.ToList();
            if (e.Any())
            {
                cH = e.First();
				if (cH.Notes.Count == 0)
				{
					// a chord with no notes - this actually happenned.
					// TODO: Delete the chord? better yet, set status to inert? handle with database constraint?
					throw new Exception("Wow, a chord with no notes!");
				}
				else
				{
                    //TODO: need to activate the chord
					EditorState.Chord = cH;
					if (EditorState.Duration != null) cH.Duration = (decimal)EditorState.Duration;
				}

            }
            else
            {
                cH = AddChord(mId, cHsT);
            }
            return cH;
        }

	    public static Chord AddChord(Guid pId, double chStarttime)
	    {
		    Chord cH;
		    cH = _repository.Create<Chord>();
		    cH.Id = Guid.NewGuid();
		    if (EditorState.Duration != null) cH.Duration = (decimal) EditorState.Duration;
		    cH.StartTime = chStarttime;
		    cH.Measure_Id = pId;
		    cH.Audit = GetAudit();
		    cH.Status = CollaborationManager.GetBaseStatus();
		    return cH;
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

        public static Chord Clone(Measure mE, Chord cH)
        {
            Chord o = null;
            try
            {
                Measure = mE;
                EditorState.Duration = (double)cH.Duration;
                if (cH.StartTime != null)
                {
                    o = GetOrCreate(mE.Id, _Enum.NotePlacementMode.None, null);
                    o.Id = Guid.NewGuid();
                    o.Measure_Id = mE.Id;
                    o.Duration = cH.Duration;
                    o.Key_Id = cH.Key_Id;
                    o.Location_X = cH.Location_X;
                    o.Location_Y = cH.Location_Y;
                    o.Audit = GetAudit();
                    o.StartTime = cH.StartTime;
                    o.Status = CollaborationManager.GetBaseStatus();
                    var iX = 0;
                    foreach (var nT in cH.Notes.Select(n => NoteController.Clone(o.Id, cH, Measure, o.Location_X + (iX * 16), n.Location_Y, n)))
                    {
                        o.Notes.Add(nT);
                        iX++;
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