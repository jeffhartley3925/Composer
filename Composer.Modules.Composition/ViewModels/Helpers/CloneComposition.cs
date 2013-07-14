using System.Linq;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;
using Microsoft.Practices.ServiceLocation;
using System.Data.Services.Client;
using Composer.Repository;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public class CloneComposition : ICloneComposition
    {
        private static IEventAggregator _ea;

        private static DataServiceRepository<Repository.DataService.Composition> _repository;

        public CloneComposition()
        {
            _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _ea.GetEvent<CloneCompositionEvent>().Subscribe(OnCloneComposition);
        }

        public void OnCloneComposition(object obj)
        {
            Repository.DataService.Composition sourceComposition = CompositionManager.Composition;

            SetDimensions(sourceComposition);

            var verses = new DataServiceCollection<Repository.DataService.Verse>(null, TrackingMode.None);
            var staffgroups = new DataServiceCollection<Repository.DataService.Staffgroup>(null, TrackingMode.None);
            var arcs = new DataServiceCollection<Repository.DataService.Arc>(null, TrackingMode.None);

            var composition = CompositionManager.Create();
            Cache.Clear();

            foreach (var sourceStaffgroup in sourceComposition.Staffgroups)
            {
                var staffgroup = StaffgroupManager.Clone(composition.Id, sourceStaffgroup);
                
                var staffs = new DataServiceCollection<Repository.DataService.Staff>(null, TrackingMode.None);
                foreach (var sourceStaff in sourceStaffgroup.Staffs)
                {
                    var staff = StaffManager.Clone(staffgroup.Id, sourceStaff);
                    var measures = new DataServiceCollection<Repository.DataService.Measure>(null, TrackingMode.None);
                    foreach (var sourceMeasure in sourceStaff.Measures)
                    {
                        var measure = MeasureManager.Clone(staff.Id, sourceMeasure);
                        var chords = new DataServiceCollection<Repository.DataService.Chord>(null, TrackingMode.None);
                        foreach (var sourceChord in sourceMeasure.Chords)
                        {
                            ChordManager.Measure = measure;
                            var chord = ChordManager.Clone(measure, sourceChord);
                            var notes = new DataServiceCollection<Repository.DataService.Note>(null, TrackingMode.None);
                            foreach (var sourceNote in sourceChord.Notes)
                            {
                                var note = NoteController.Clone(chord.Id, sourceChord, measure, sourceNote.Location_X, sourceNote.Location_Y, sourceNote);
                                if (note != null)
                                {
                                    _repository.Context.AddLink(chord, "Notes", note);
                                    notes.Add(note);
                                }
                            }
                            chord.Notes = notes;
                            _repository.Context.AddLink(measure, "Chords", chord);
                            chords.Add(chord);
                        }
                        measure.Chords = chords;
                        _repository.Context.AddLink(staff, "Measures", measure);
                        measures.Add(measure);
                    }
                    staff.Measures = measures;
                    _repository.Context.AddLink(staffgroup, "Staffs", staff);
                    staffs.Add(staff);
                }
                staffgroup.Staffs = staffs;
                _repository.Context.AddLink(composition, "Staffgroups", staffgroup);
                staffgroups.Add(staffgroup);
            }
            int s;
            for (s = 0; s < sourceComposition.Arcs.Count; s++)
            {
                var arc = ArcManager.Clone(sourceComposition.Arcs[s]);
                _repository.Context.AddLink(composition, "Arcs", arc);
                arcs.Add(arc);
            }

            for (s = 0; s < sourceComposition.Verses.Count; s++)
            {
                var verse = VerseManager.Clone(composition.Id, sourceComposition.Verses[s]);
                Cache.Verses.Add(verse);
                _repository.Context.AddLink(composition, "Verses", verse);
                verses.Add(verse);
            }
            composition.Instrument_Id = sourceComposition.Instrument_Id;
            composition.Key_Id = sourceComposition.Key_Id;
            composition.Provenance = sourceComposition.Provenance;
            composition.Arcs = arcs;
            composition.Verses = verses;
            composition.Staffgroups = staffgroups;

            _ea.GetEvent<NewComposition>().Publish(composition);
        }
        private void SetDimensions(Repository.DataService.Composition composition)
        {
            Infrastructure.Support.Densities.StaffgroupDensity = composition.Staffgroups.Count;
            Infrastructure.Support.Densities.StaffDensity = composition.Staffgroups[0].Staffs.Count;
            Infrastructure.Support.Densities.MeasureDensity = composition.Staffgroups[0].Staffs[0].Measures.Count;

            Infrastructure.Dimensions.Keys.Key = (from a in Infrastructure.Dimensions.Keys.KeyList where a.Id == composition.Key_Id select a).First();
            Infrastructure.Dimensions.TimeSignatures.TimeSignature = (from a in Infrastructure.Dimensions.TimeSignatures.TimeSignatureList where a.Id == composition.TimeSignature_Id select a).First();
            Infrastructure.Dimensions.Bars.Bar = (from a in Infrastructure.Dimensions.Bars.BarList where a.Id == composition.Staffgroups[0].Staffs[0].Measures[0].Bar_Id select a).First();
            Infrastructure.Dimensions.Instruments.Instrument = (from a in Infrastructure.Dimensions.Instruments.InstrumentList where a.Id == composition.Instrument_Id select a).First();
        }
    }
}
