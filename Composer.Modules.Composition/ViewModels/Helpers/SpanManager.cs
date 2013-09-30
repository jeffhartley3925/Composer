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

        public static void OnSpan(object o)
        {
            var measure = (Measure)o;
            Span(measure);
        }

        public static void Span(Measure m)
        {
            Measure = m;
            NotegroupManager.Measure = m;
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

        private static void Span(int chIdx1, int chIdx2)
        {
            var increment = 1;

            _ngs1 = GetNotegroups(chIdx1);
            _ngs2 = GetNotegroups(chIdx2);

            if (!ValidSpan(chIdx1, chIdx2)) return;
            if (!_ngs2.Any())
                Span(chIdx2 + 1, chIdx2 + 2);

            foreach (var ng1 in _ngs1)
            {
                var r1 = ng1.Root;
                if (Spannable(ng1, r1))
                {
                    foreach (var ng2 in _ngs2)
                    {
                        var r2 = ng2.Root;
                        if (Spannable(ng2, r2) &&
                            Math.Abs(r1.Location_Y - r2.Location_Y) <= Preferences.MediumOkToSpanThreshhold &&
                            r2.Orientation == r1.Orientation)
                        {
                            _ea.GetEvent<RemoveNotegroupFlag>().Publish(ng1);
                            _ea.GetEvent<RemoveNotegroupFlag>().Publish(ng2);
                            Render(ng1, ng2, r1, r2);
                            increment = 2;
                        }
                        else
                        {
                            FlagNotegroups(new[] { ng1, ng2 }.ToList());
                            increment = 2;
                        }
                        //break; //added 10/08/2012
                    }
                }
                else
                {
                    _ea.GetEvent<FlagNotegroup>().Publish(ng1);
                }
            }
            Span(chIdx1 + increment, chIdx2 + increment);
        }

        private static bool ValidSpan(int chIdx1, int chIdx2)
        {
            if (ChordStarttimes.Length <= 1 || chIdx1 > chIdx2 || chIdx2 > ChordStarttimes.Length - 1)
            {
                if (_ngs1 != null) FlagNotegroups(_ngs1);
                if (_ngs2 != null) FlagNotegroups(_ngs2);
                return false;
            }
            return MeasureChordNotegroups.Any();
        }

        private static bool Spannable(Notegroup ng, Note root)
        {
            Func<Notegroup, bool> isSpannable =
                a => !ng.IsRest &&
                     !ng.IsSpanned &&
                      ng.Duration < 1 &&
                      ng.Orientation < 2 && //if orientation = 2, it's a n. 2 is equivalent to null (orientation has no meaning in this context)
                      CollaborationManager.IsActive(root);
            return isSpannable(ng);
        }

        private static List<Notegroup> GetNotegroups(int idx)
        {
            var ngs = new List<Notegroup>();
            if (idx < ChordStarttimes.Length)
            {
                var a = (from x in MeasureChordNotegroups
                         where x.Key == ChordStarttimes[idx]
                         select x.Value);
                var enumerable = a as List<List<Notegroup>> ?? a.ToList();
                if (enumerable.Any())
                {
                    ngs = enumerable.First();
                }
            }
            return ngs;
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
        private static void FlagNotegroup(Notegroup ng)
        {
            if (!ng.IsSpanned)
            {
                _ea.GetEvent<FlagNotegroup>().Publish(ng);
                foreach (var n in ng.Notes)
                {
                    n.IsSpanned = false;
                }
            }
        }

        public static void RemoveNotegroupFlags(List<Notegroup> ngs)
        {
            foreach (Notegroup ng in ngs)
            {
                foreach (Note n in ng.Notes)
                {
                    if (!NoteController.IsRest(n))
                    {
                        n.Vector_Id = 8;
                        n.IsSpanned = true;
                    }
                }
            }
        }

        public static void OnRemoveNotegroupFlag(Object obj)
        {
            var notegroup = (Notegroup)obj;
            RemoveNotegroupFlags(new[] { notegroup }.ToList());
        }

        public static void OnRemoveNoteFlag(Note n)
        {
            if (!NoteController.IsRest(n))
            {
                n.Vector_Id = 8;
            }
        }

        private static void Render(Notegroup ng1, Notegroup ng2, Note r1, Note r2)
        {
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
            Decimal d1 = r1.Duration;
            Decimal d2 = r2.Duration;

            if (r1.Orientation != null) span.Orientation = (short)r1.Orientation;

            #region span logic

            //here we handle full spanners
            var spanWidth = 5;
            const int lineWidth = 2;
            var step = 0;

            sb.AppendFormat(SpanFormatter, spanWidth * step, spanWidth * step - lineWidth, dX, (dY + spanWidth * step), (dY - spanWidth * step - lineWidth), 0);

            if (d1 <= .25M && d2 <= .25M)
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
            if (d1 + d2 == .25M)
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
            if (d1 == .5M && d2 == .25M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth - step, spanWidth - step - lineWidth, dX, dY + spanWidth, dY + spanWidth - lineWidth, dX / 2);
                }
                else
                {
                    spanWidth = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(spanWidth) : -Math.Abs(spanWidth);
                    sb.AppendFormat(SpanFormatter, spanWidth + step, spanWidth + step - lineWidth, dX, dY + spanWidth, dY + spanWidth - lineWidth, dX / 2);
                }
            }
            else if (d1 == .25M && d2 == .5M)
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

            if (d1 == .5M && d2 == .125M)
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

            else if (d1 == .125M && d2 == .5M)
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

            if (d1 == .25M && d2 == .125M)
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

            else if (d1 == .125M && d2 == .25M)
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
