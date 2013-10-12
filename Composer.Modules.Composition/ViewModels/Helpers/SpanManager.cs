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
            Span((Measure)obj);
        }

        public static void Span(Measure m)
        {
            Measure = m;
            NotegroupManager.Measure = m;
            MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStarttimes, out ChordInactiveTimes);
            Array.Sort(ChordStarttimes);
            DeleteSpans();
            Span(0, 1);
            // TODO: we pass the entire measure in the payload when all we need is the measure id.
            _ea.GetEvent<SpanUpdate>().Publish(new SpanPayload(Measure, LocalSpans));
        }

        private static void DeleteSpans()
        {
            if (LocalSpans == null) return;
            var spans = LocalSpans.ToList();
            foreach (var span in spans)
            {
                LocalSpans.Remove(span);
            }
            spans.Clear();
            // TODO: we pass the entire measure in the payload when all we need is the measure id.
            _ea.GetEvent<SpanUpdate>().Publish(new SpanPayload(Measure, LocalSpans));
            MeasureManager.Flag();
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
                var n1 = ng1.Root;
                if (Spannable(ng1, n1))
                {
                    foreach (var ng2 in _ngs2)
                    {
                        var n2 = ng2.Root;
                        if (Spannable(ng2, n2) &&
                            Math.Abs(n1.Location_Y - n2.Location_Y) <= Preferences.MediumOkToSpanThreshhold &&
                            n2.Orientation == n1.Orientation)
                        {
                            _ea.GetEvent<RemoveNotegroupFlag>().Publish(ng1);
                            _ea.GetEvent<RemoveNotegroupFlag>().Publish(ng2);
                            Render(ng1, ng2, n1, n2);
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
            Span(chIdx1 + increment, chIdx2 + increment);
        }

        private static bool ValidSpan(int chIdx1, int chIdx2)
        {
            // even though I thought it up, this logic is foreign to me.
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
                      ng.Orientation < 2 && // if orientation < 2, it's a note. //TODO: we need to use one way to distinguish a rest from a note. 
                      CollaborationManager.IsActive(root);
            return isSpannable(ng);
        }

        private static List<Notegroup> GetNotegroups(int idx)
        {
            var ngs = new List<Notegroup>();
            if (idx >= ChordStarttimes.Length) return ngs;
            var a = (from x in MeasureChordNotegroups
                where x.Key == ChordStarttimes[idx]
                select x.Value);
            var e = a as List<List<Notegroup>> ?? a.ToList();
            if (e.Any())
            {
                ngs = e.First();
            }
            return ngs;
        }

        private static void FlagNotegroups(ICollection<Notegroup> ngs)
        {
            if (ngs == null) return;
            if (ngs.Count == 0) return;
            foreach (var ng in ngs)
            {
                FlagNotegroup(ng);
            }
        }
        private static void FlagNotegroup(Notegroup ng)
        {
            if (ng.IsSpanned) return;
            _ea.GetEvent<FlagNotegroup>().Publish(ng);
            foreach (var n in ng.Notes)
            {
                n.IsSpanned = false;
            }
        }

        public static void RemoveNotegroupFlags(List<Notegroup> ngs)
        {
            foreach (var ng in ngs)
            {
                foreach (var n in ng.Notes.Where(n => !NoteController.IsRest(n)))
                {
                    n.Vector_Id = 8;
                    n.IsSpanned = true;
                }
            }
        }

        public static void OnRemoveNotegroupFlag(Object obj)
        {
            var ng = (Notegroup)obj;
            RemoveNotegroupFlags(new[] { ng }.ToList());
        }

        public static void OnRemoveNoteFlag(Note n)
        {
            if (!NoteController.IsRest(n))
            {
                n.Vector_Id = 8;
            }
        }

        private static void Render(Notegroup ng1, Notegroup ng2, Note n1, Note n2)
        {
            ng1.IsSpanned = true;
            ng2.IsSpanned = true;
            var span = new LocalSpan();
            var sb = new StringBuilder();

            var ch = (from obj in Cache.Chords where obj.Id == n1.Chord_Id select obj).First();

            span.Location_X = ch.Location_X + 19;
            span.Location_Y = n1.Location_Y + 25;
            span.MeasureIndex = Measure.Index;
            span.Measure_Id = Measure.Id;
            var dX = ng2.GroupX - ng1.GroupX - 1;
            var dY = (ng2.GroupY - ng1.GroupY);
            var dy = Math.Abs(dY);
            var n1Duration = n1.Duration;
            var n2Duration = n2.Duration;

            if (n1.Orientation != null) span.Orientation = (short)n1.Orientation;

            #region Span Logic

            //here we handle full spanners
            var w = 5;
            const int lineWidth = 2;
            var step = 0;

            sb.AppendFormat(SpanFormatter, w * step, w * step - lineWidth, dX, (dY + w * step), (dY - w * step - lineWidth), 0);

            if (n1Duration <= .25M && n2Duration <= .25M)
            {
                step = 1;
                switch (span.Orientation)
                {
                    case (short)_Enum.Orientation.Up:
                        sb.AppendFormat(SpanFormatter, w * step, w * step - lineWidth, dX, (dY + w * step), (dY + w * step - lineWidth), 0);
                        break;
                    case (short)_Enum.Orientation.Down:
                        sb.AppendFormat(SpanFormatter, -w * step, -(w * step + lineWidth), dX, (dY - w * step), (dY - w * step - lineWidth), 0);
                        break;
                }
            }
            if (n1Duration + n2Duration == .25M)
            {
                step = 2;
                switch (span.Orientation)
                {
                    case (short)_Enum.Orientation.Up:
                        sb.AppendFormat(SpanFormatter, w * step, w * step - lineWidth, dX, (dY + w * step), (dY + w * step - lineWidth), 0);
                        break;
                    case (short)_Enum.Orientation.Down:
                        sb.AppendFormat(SpanFormatter, -w * step, -(w * step + lineWidth), dX, (dY - w * step), (dY - w * step - lineWidth), 0);
                        break;
                }
            }

            step = dy / 2;
            //here we handle partial spans
            if (n1Duration == .5M && n2Duration == .25M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    w = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    sb.AppendFormat(SpanFormatter, w - step, w - step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                }
                else
                {
                    w = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    sb.AppendFormat(SpanFormatter, w + step, w + step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                }
            }
            else if (n1Duration == .25M && n2Duration == .5M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    w = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    sb.AppendFormat(SpanFormatter, w, w - lineWidth, dX / 2, dY + w + step, dY + w + step - lineWidth, 0);
                }
                else
                {
                    w = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    sb.AppendFormat(SpanFormatter, w, w - lineWidth, dX / 2, dY + w - step, dY + w - step - lineWidth, 0);
                }
            }

            if (n1Duration == .5M && n2Duration == .125M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    switch (span.Orientation)
                    {
                        case (short)_Enum.Orientation.Up:
                            w = Math.Abs(w);
                            sb.AppendFormat(SpanFormatter, w - step, w - step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                            sb.AppendFormat(SpanFormatter, w * 2 - step, w * 2 - step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                            break;
                        case (short)_Enum.Orientation.Down:
                            w = -Math.Abs(w);
                            sb.AppendFormat(SpanFormatter, w - step, w - step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                            sb.AppendFormat(SpanFormatter, w * 2 - step, w * 2 - step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                            break;
                    }
                }
                else
                {
                    switch (span.Orientation)
                    {
                        case (short)_Enum.Orientation.Up:
                            w = Math.Abs(w);
                            sb.AppendFormat(SpanFormatter, w + step, w + step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                            sb.AppendFormat(SpanFormatter, w * 2 + step, w * 2 + step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                            break;
                        case (short)_Enum.Orientation.Down:
                            w = -Math.Abs(w);
                            sb.AppendFormat(SpanFormatter, w + step, w + step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                            sb.AppendFormat(SpanFormatter, w * 2 + step, w * 2 + step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                            break;
                    }
                }
            }

            else if (n1Duration == .125M && n2Duration == .5M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    w = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    sb.AppendFormat(SpanFormatter, w, w - lineWidth, dX / 2, dY + w + step, dY + w + step - lineWidth, 0);
                    sb.AppendFormat(SpanFormatter, w * 2, w * 2 - lineWidth, dX / 2, dY + w * 2 + step, dY + w * 2 + step - lineWidth, 0);
                }
                else
                {
                    w = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    sb.AppendFormat(SpanFormatter, w, w - lineWidth, dX / 2, dY + w - step, dY + w - step - lineWidth, 0);
                    sb.AppendFormat(SpanFormatter, w * 2, w * 2 - lineWidth, dX / 2, dY + w * 2 - step, dY + w * 2 - step - lineWidth, 0);
                }
            }

            if (n1Duration == .25M && n2Duration == .125M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    w = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    sb.AppendFormat(SpanFormatter, w * 2 - step, w * 2 - step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                }
                else
                {
                    w = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    sb.AppendFormat(SpanFormatter, w * 2 + step, w * 2 + step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                }
            }

            else if (n1Duration == .125M && n2Duration == .25M)
            {
                if (ng1.GroupY >= ng2.GroupY)
                {
                    w = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    sb.AppendFormat(SpanFormatter, w * 2, w * 2 - lineWidth, dX / 2, dY + w * 2 + step, dY + w * 2 + step - lineWidth, 0);
                }
                else
                {
                    w = ((short)_Enum.Orientation.Up == span.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    sb.AppendFormat(SpanFormatter, w * 2, w * 2 - lineWidth, dX / 2, dY + w * 2 - step, dY + w * 2 - step - lineWidth, 0);
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
