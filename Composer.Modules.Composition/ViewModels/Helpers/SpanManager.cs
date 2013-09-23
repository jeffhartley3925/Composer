using System;
using System.Linq;
using System.Collections.ObjectModel;
using Composer.Infrastructure;
using System.Collections.Generic;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using System.Text;
using Composer.Repository;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public static class SpanManager
    {
        private static DataServiceRepository<Repository.DataService.Composition> _repository;
        private static IEventAggregator _ea;
        public static Dictionary<decimal, List<Notegroup>> MeasureChordNotegroups;
        public static decimal[] ChordStarttimes;
        public static decimal[] ChordInactiveTimes;
        public static ObservableCollection<LocalSpan> LocalSpans { get; set; }
        public static Measure Measure { get; set; }
        private const string SpanFormatter = "M {5} {0} L {5} {1} L {2} {4} L {2} {3} L {5} {0} Z";

        static SpanManager()
        {
            Initialize();
        }

        private static void Initialize()
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
            _ea.GetEvent<RemoveNotegroupFlag>().Subscribe(OnRemoveNotegroupFlag);
            _ea.GetEvent<SpanMeasure>().Subscribe(OnSpan);
        }

        public static void OnSpan(object obj)
        {
            var measure = (Measure)obj;
            Span(measure);
        }

        public static void Span(Measure measure)
        {
            Measure = measure;
            NotegroupManager.Measure = measure;
            MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStarttimes, out ChordInactiveTimes);
            Array.Sort(ChordStarttimes);
            DeleteSpans();
            Span(0, 1);
            _ea.GetEvent<SpanUpdate>().Publish(new SpanPayload(Measure, LocalSpans));
        }

        private static void DeleteSpans()
        {
            if (LocalSpans != null)
            {
                var removed = LocalSpans.ToList();
                foreach (var item in removed)
                {
                    LocalSpans.Remove(item);
                }
                removed.Clear();
                _ea.GetEvent<SpanUpdate>().Publish(new SpanPayload(Measure, LocalSpans));
                MeasureManager.Flag();
            }
        }

        private static List<Notegroup> _ngs2;
        private static List<Notegroup> _ngs1;

        private static void Span(int chordIndexA, int chordIndexB)
        {
            _ngs1 = GetNotegroups(chordIndexA);
            _ngs2 = GetNotegroups(chordIndexB);
            var increment = 1;

            if (ValidSpan(chordIndexA, chordIndexB))
            {
                if (!_ngs2.Any())
                    Span(chordIndexB + 1, chordIndexB + 2);

                foreach (var ng1 in _ngs1)
                {
                    var r1 = ng1.Root;
                    if (Spannable(ng1))
                    {
                        foreach (var ng2 in _ngs2)
                        {
                            var r2 = ng2.Root;
                            if (Spannable(ng2) &&
                                Math.Abs(r1.Location_Y - r2.Location_Y) <= GetYCoordThreshold(r1, r2) &&
                                r2.Orientation == r1.Orientation)
                            {
                                _ea.GetEvent<RemoveNotegroupFlag>().Publish(ng1);
                                _ea.GetEvent<RemoveNotegroupFlag>().Publish(ng2);
                                Render(ng1, ng2);
                                increment = 2;
                            }
                            else
                            {
                                FlagNotegroups(new[] { ng1, ng2 }.ToList());
                                increment = 2;
                            }
                            break; //added 10/08/2012
                        }
                    }
                    else
                    {
                        _ea.GetEvent<FlagNotegroup>().Publish(ng1);
                    }
                }
                Span(chordIndexA + increment, chordIndexB + increment);
            }
        }

        private static bool ValidSpan(int chordIndexA, int chordIndexB)
        {
            if (ChordStarttimes.Length <= 1 || chordIndexA > chordIndexB || chordIndexB > ChordStarttimes.Length - 1)
            {
                if (_ngs1 != null) FlagNotegroups(_ngs1);
                if (_ngs2 != null) FlagNotegroups(_ngs2);
                return false;
            }
            //TODO: odd
            return true && MeasureChordNotegroups.Any();
        }

        private static bool Spannable(Notegroup ng)
        {
            Func<Notegroup, bool> isSpannable =
                a => !ng.IsRest &&
                      !ng.IsSpanned &&
                      ng.Duration < 1 &&
                      ng.Orientation < 2 && //if orientation = 2, it's a rest. 2 is equivalent to null (orientation has no meaning in this context)
                      CollaborationManager.IsActive(ng.Root);
            return isSpannable(ng);
        }

        private static int GetYCoordThreshold(Note note1, Note note2)
        {
            //the vertical distance between 2 notes is part of the determination whether 2 notes can be spanned.
            //the threshold is the max distance between 2 notes that will allow them to be spanned.
            //TODO: threshhold is never used.
            var threshhold = 0;

            var d = (int)((note1.Duration + note2.Duration) * 1000);
            switch (d)
            {
                case 250:
                    threshhold = Preferences.MediumOkToSpanThreshhold;
                    break;
                case 375:
                    threshhold = Preferences.MediumOkToSpanThreshhold;
                    break;
                case 500:
                    threshhold = Preferences.MediumOkToSpanThreshhold;
                    break;
                case 625:
                    threshhold = Preferences.MediumOkToSpanThreshhold;
                    break;
                case 750:
                    threshhold = Preferences.MediumOkToSpanThreshhold;
                    break;
                case 875:
                    threshhold = Preferences.MediumOkToSpanThreshhold;
                    break;
                case 1000:
                    threshhold = Preferences.LargeOkToSpanThreshhold;
                    break;
                default:
                    threshhold = Preferences.MediumOkToSpanThreshhold;
                    break;
            }
            return Preferences.MediumOkToSpanThreshhold;
        }

        private static List<Notegroup> GetNotegroups(int index)
        {
            var notegroups = new List<Notegroup>();
            if (index < ChordStarttimes.Length)
            {
                var a = (from x in MeasureChordNotegroups
                         where x.Key == ChordStarttimes[index]
                         select x.Value);
                var enumerable = a as List<List<Notegroup>> ?? a.ToList();
                if (enumerable.Any())
                {
                    notegroups = enumerable.First();
                }
            }
            return notegroups;
        }

        private static void FlagNotegroups(List<Notegroup> ngs)
        {
            if (ngs == null) return;
            if (ngs.Count == 0) return;
            foreach (Notegroup ng in ngs)
            {
                FlagNotegroup(ng);
            }
        }
        private static void FlagNotegroup(Notegroup notegroup)
        {
            if (!notegroup.IsSpanned)
            {
                _ea.GetEvent<FlagNotegroup>().Publish(notegroup);
                //added 4 lines on 1/27/2013
                foreach (Repository.DataService.Note note in notegroup.Notes)
                {
                    note.IsSpanned = false;
                }
            }
        }

        public static void RemoveNotegroupFlags(List<Notegroup> notegroups)
        {
            foreach (Notegroup notegroup in notegroups)
            {
                foreach (Note note in notegroup.Notes)
                {
                    if (!NoteController.IsRest(note))
                    {
                        note.Vector_Id = 8;
                        note.IsSpanned = true;
                    }
                }
            }
        }

        public static void OnRemoveNotegroupFlag(Object obj)
        {
            var notegroup = (Notegroup)obj;
            RemoveNotegroupFlags(new[] { notegroup }.ToList());
        }

        public static void OnRemoveNoteFlag(Note note)
        {
            if (!NoteController.IsRest(note))
            {
                note.Vector_Id = 8;
            }
        }

        private static void Render(Notegroup ng1, Notegroup ng2)
        {
            var r1 = ng1.Root;
            var r2 = ng2.Root;
            ng1.IsSpanned = true;
            ng2.IsSpanned = true;
            var span = new LocalSpan();
            var sb = new StringBuilder();

            var c = (from obj in Cache.Chords where obj.Id == r1.Chord_Id select obj);
            var enumerable = c as List<Chord> ?? c.ToList();
            if (!enumerable.Any()) return;
            var chord = enumerable.First();

            span.Location_X = chord.Location_X + 19;
            span.Location_Y = r1.Location_Y + 25;
            span.MeasureIndex = Measure.Index;
            span.Measure_Id = Measure.Id;
            var dX = ng2.GroupX - ng1.GroupX - 1;
            var dY = (ng2.GroupY - ng1.GroupY);
            var dy = Math.Abs(dY);
            Decimal dur1 = r1.Duration;
            Decimal dur2 = r2.Duration;

            if (r1.Orientation != null) span.Orientation = (short)r1.Orientation;

            #region span logic

            //here we handle full spanners
            var spanWidth = 5;
            const int lineWidth = 2;
            var step = 0;

            sb.AppendFormat(SpanFormatter, spanWidth * step, spanWidth * step - lineWidth, dX, (dY + spanWidth * step), (dY - spanWidth * step - lineWidth), 0);

            if (dur1 <= .25M && dur2 <= .25M)
            {
                step = 1;
                switch (span.Orientation)
                {
                    case (short)_Enum.Orientation.Up:
                        sb.AppendFormat(SpanFormatter, spanWidth * step, spanWidth * step - lineWidth, dX, (dY + spanWidth * step), (dY + spanWidth * step - lineWidth), 0);
                        break;
                    case (short)_Enum.Orientation.Down:
                        sb.AppendFormat(SpanFormatter, -spanWidth * step, -(spanWidth * step + lineWidth), dX, (dY - spanWidth * step), (dY - spanWidth * step - lineWidth), 0);
                        break;
                }
            }
            if (dur1 + dur2 == .25M)
            {
                step = 2;
                switch (span.Orientation)
                {
                    case (short)_Enum.Orientation.Up:
                        sb.AppendFormat(SpanFormatter, spanWidth * step, spanWidth * step - lineWidth, dX, (dY + spanWidth * step), (dY + spanWidth * step - lineWidth), 0);
                        break;
                    case (short)_Enum.Orientation.Down:
                        sb.AppendFormat(SpanFormatter, -spanWidth * step, -(spanWidth * step + lineWidth), dX, (dY - spanWidth * step), (dY - spanWidth * step - lineWidth), 0);
                        break;
                }
            }

            step = dy / 2;
            //here we handle partial spans
            if (dur1 == .5M && dur2 == .25M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    string result = string.Format("M {5} {0} L {5} {1} L {2} {4} L {2} {3} L {5} {0} Z", spanWidth - step, spanWidth - step - lineWidth, dX, dY + spanWidth, dY + spanWidth - lineWidth, dX / 2);
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth - step, spanWidth - step - lineWidth, dX, dY + spanWidth, dY + spanWidth - lineWidth, dX / 2);
                }
                else
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth + step, spanWidth + step - lineWidth, dX, dY + spanWidth, dY + spanWidth - lineWidth, dX / 2);
                }
            }
            else if (dur1 == .25M && dur2 == .5M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth, spanWidth - lineWidth, dX / 2, dY + spanWidth + step, dY + spanWidth + step - lineWidth, 0);
                }
                else
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth, spanWidth - lineWidth, dX / 2, dY + spanWidth - step, dY + spanWidth - step - lineWidth, 0);
                }
            }

            if (dur1 == .5M && dur2 == .125M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    switch (span.Orientation)
                    {
                        case (short)_Enum.Orientation.Up:
                            spanWidth = Math.Abs(spanWidth);
                            sb.AppendFormat(SpanFormatter, spanWidth - step, spanWidth - step - lineWidth, dX, dY + spanWidth, dY + spanWidth - lineWidth, dX / 2);
                            sb.AppendFormat(SpanFormatter, spanWidth * 2 - step, spanWidth * 2 - step - lineWidth, dX, dY + spanWidth * 2, dY + spanWidth * 2 - lineWidth, dX / 2);
                            break;
                        case (short)_Enum.Orientation.Down:
                            spanWidth = -Math.Abs(spanWidth);
                            sb.AppendFormat(SpanFormatter, spanWidth - step, spanWidth - step - lineWidth, dX, dY + spanWidth, dY + spanWidth - lineWidth, dX / 2);
                            sb.AppendFormat(SpanFormatter, spanWidth * 2 - step, spanWidth * 2 - step - lineWidth, dX, dY + spanWidth * 2, dY + spanWidth * 2 - lineWidth, dX / 2);
                            break;
                    }
                }
                else
                {
                    switch (span.Orientation)
                    {
                        case (short)_Enum.Orientation.Up:
                            spanWidth = Math.Abs(spanWidth);
                            sb.AppendFormat(SpanFormatter, spanWidth + step, spanWidth + step - lineWidth, dX, dY + spanWidth, dY + spanWidth - lineWidth, dX / 2);
                            sb.AppendFormat(SpanFormatter, spanWidth * 2 + step, spanWidth * 2 + step - lineWidth, dX, dY + spanWidth * 2, dY + spanWidth * 2 - lineWidth, dX / 2);
                            break;
                        case (short)_Enum.Orientation.Down:
                            spanWidth = -Math.Abs(spanWidth);
                            sb.AppendFormat(SpanFormatter, spanWidth + step, spanWidth + step - lineWidth, dX, dY + spanWidth, dY + spanWidth - lineWidth, dX / 2);
                            sb.AppendFormat(SpanFormatter, spanWidth * 2 + step, spanWidth * 2 + step - lineWidth, dX, dY + spanWidth * 2, dY + spanWidth * 2 - lineWidth, dX / 2);
                            break;
                    }
                }
            }

            else if (dur1 == .125M && dur2 == .5M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth, spanWidth - lineWidth, dX / 2, dY + spanWidth + step, dY + spanWidth + step - lineWidth, 0);
                    sb.AppendFormat(SpanFormatter, spanWidth * 2, spanWidth * 2 - lineWidth, dX / 2, dY + spanWidth * 2 + step, dY + spanWidth * 2 + step - lineWidth, 0);
                }
                else
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth, spanWidth - lineWidth, dX / 2, dY + spanWidth - step, dY + spanWidth - step - lineWidth, 0);
                    sb.AppendFormat(SpanFormatter, spanWidth * 2, spanWidth * 2 - lineWidth, dX / 2, dY + spanWidth * 2 - step, dY + spanWidth * 2 - step - lineWidth, 0);
                }
            }

            if (dur1 == .25M && dur2 == .125M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth * 2 - step, spanWidth * 2 - step - lineWidth, dX, dY + spanWidth * 2, dY + spanWidth * 2 - lineWidth, dX / 2);
                }
                else
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth * 2 + step, spanWidth * 2 + step - lineWidth, dX, dY + spanWidth * 2, dY + spanWidth * 2 - lineWidth, dX / 2);
                }
            }

            else if (dur1 == .125M && dur2 == .25M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth * 2, spanWidth * 2 - lineWidth, dX / 2, dY + spanWidth * 2 + step, dY + spanWidth * 2 + step - lineWidth, 0);
                }
                else
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth * 2, spanWidth * 2 - lineWidth, dX / 2, dY + spanWidth * 2 - step, dY + spanWidth * 2 - step - lineWidth, 0);
                }
            }

            #endregion span logic

            span.Path = sb.ToString();
            if (LocalSpans == null)
                LocalSpans = new ObservableCollection<LocalSpan>();
            LocalSpans.Add(span);
            Cache.Spans.Add(span);
        }
    }
}
