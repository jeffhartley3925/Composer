using System;
using System.Linq;
using System.Collections.ObjectModel;
using Composer.Infrastructure;
using System.Collections.Generic;
using Composer.Modules.Composition.Models;
using Composer.Modules.Composition.ViewModels.Helpers;
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
        public static Dictionary<decimal, List<Notegroup>> mEcHnGs;
        public static decimal[] ChordStarttimes;
        public static decimal[] ChordInactiveTimes;
        public static ObservableCollection<Span> LocalSpans { get; set; }
        public static Measure Measure { get; set; }
        private const string SpanFormatter = "M {5} {0} L {5} {1} L {2} {4} L {2} {3} L {5} {0} Z";
        public static ObservableCollection<Span> GlobalSpans { get; set; }
	    public static double thresholdStarttime = 0;

        static SpanManager()
        {
            Initialize();
        }

        private static void Initialize()
        {
            GlobalSpans = new ObservableCollection<Span>();
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
            _ea.GetEvent<SpanMeasure>().Subscribe(OnSpanMeasure);
			_ea.GetEvent<SetThreshholdStarttime>().Subscribe(OnSetThreshholdStarttime);
        }

		public static void OnSetThreshholdStarttime(Tuple<Guid, double> payload)
		{
			thresholdStarttime = payload.Item2;
		}

        public static void OnSpanMeasure(Guid id)
        {
            Measure = Utils.GetMeasure(id);
            NotegroupManager.Measure = Measure;
            mEcHnGs = NotegroupManager.ParseMeasure(out ChordStarttimes);
            Array.Sort(ChordStarttimes);
            // TODO: no need to delete all spans everytime. add 'smart' code so that only 
            // chords that are new, shifted or are added too have their spans destroyed and re=created
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
            // TODO: we pass the entire measure in the payload when all we need is the measure id.
            _ea.GetEvent<SpanUpdate>().Publish(new SpanPayload(Measure, LocalSpans));
            _ea.GetEvent<FlagMeasure>().Publish(Measure.Id);
        }

        private static List<Notegroup> nGs2;
        private static List<Notegroup> nGs1;

        private static void Span(int cHpointer1, int cHpointer2)
        {
            var increment = 1;

            nGs1 = GetNotegroups(cHpointer1);
            nGs2 = GetNotegroups(cHpointer2);

            if (!ValidSpan(cHpointer1, cHpointer2)) return;
            if (!nGs2.Any())
                Span(cHpointer2 + 1, cHpointer2 + 2);

            foreach (var nG1 in nGs1)
            {
                var nT1 = nG1.Root;
                if (Spannable(nG1, nT1))
                {
                    foreach (var nG2 in nGs2)
                    {
                        var nT2 = nG2.Root;
                        if (Spannable(nG2, nT2) &&
                            Math.Abs(nT1.Location_Y - nT2.Location_Y) <= Preferences.MediumOkToSpanThreshhold &&
                            nT2.Orientation == nT1.Orientation)
                        {
                            _ea.GetEvent<RemoveNotegroupFlag>().Publish(nG1);
                            _ea.GetEvent<RemoveNotegroupFlag>().Publish(nG2);
                            Render(nG1, nG2, nT1, nT2);
                            increment = 2;
                        }
                        else
                        {
                            FlagNotegroups(new[] { nG1, nG2 }.ToList());
                            increment = 2;
                        }
                        break;
                    }
                }
                else
                {
                    _ea.GetEvent<FlagNotegroup>().Publish(nG1);
                }
            }
            Span(cHpointer1 + increment, cHpointer2 + increment);
        }

        private static bool ValidSpan(int cHpointer1, int cHpointer2)
        {
            // even though I thought it up, this logic is foreign to me.
            if (ChordStarttimes.Length <= 1 || cHpointer1 > cHpointer2 || cHpointer2 > ChordStarttimes.Length - 1)
            {
                if (nGs1 != null) FlagNotegroups(nGs1);
                if (nGs2 != null) FlagNotegroups(nGs2);
                return false;
            }
            return mEcHnGs.Any();
        }

        private static bool Spannable(Notegroup nG, Note root)
        {
            Func<Notegroup, bool> isSpannable =
                a => !nG.IsRest &&
                     !nG.IsSpanned &&
                      nG.Duration < 1 &&
					  /* ng.StartTime >= thresholdStarttime && */
                      nG.Orientation < 2 && // if orientation < 2, it's a note. //TODO: we need to use one way to distinguish a rest from a note. 
                      CollaborationManager.IsActive(root);
            return isSpannable(nG);
        }

		private static List<Notegroup> GetNotegroups(Guid cHiD)
		{
			var cH = Utils.GetChord(cHiD);
			var sT = (decimal)cH.StartTime;
			var nGs = new List<Notegroup>();
			var a = (from x in mEcHnGs
					 where x.Key == sT
					 select x.Value);
			var e = a as List<List<Notegroup>> ?? a.ToList();
			if (e.Any())
			{
				nGs = e.First();
			}
			return nGs;
		}

        private static List<Notegroup> GetNotegroups(int cHpointer)
        {
            var nGs = new List<Notegroup>();
            if (cHpointer >= ChordStarttimes.Length) return nGs;
            var a = (from x in mEcHnGs
                where x.Key == ChordStarttimes[cHpointer]
                select x.Value);
            var e = a as List<List<Notegroup>> ?? a.ToList();
            if (e.Any())
            {
                nGs = e.First();
            }
            return nGs;
        }

        private static void FlagNotegroups(ICollection<Notegroup> nGs)
        {
            if (nGs == null) return;
            if (nGs.Count == 0) return;
            foreach (var nG in nGs)
            {
                FlagNotegroup(nG);
            }
        }
        private static void FlagNotegroup(Notegroup nG)
        {
            if (nG.IsSpanned) return;
            _ea.GetEvent<FlagNotegroup>().Publish(nG);
            foreach (var nT in nG.Notes)
            {
                nT.IsSpanned = false;
            }
        }

        public static void RemoveNotegroupFlags(List<Notegroup> nGs)
        {
            foreach (var nG in nGs)
            {
                foreach (var nT in nG.Notes.Where(a => !NoteController.IsRest(a)))
                {
                    nT.Vector_Id = 8;
                    nT.IsSpanned = true;
                }
            }
        }

        public static void OnRemoveNotegroupFlag(Object obj)
        {
            var nG = (Notegroup)obj;
            RemoveNotegroupFlags(new[] { nG }.ToList());
        }

        public static void OnRemoveNoteFlag(Note nT)
        {
            if (!NoteController.IsRest(nT))
            {
                nT.Vector_Id = 8;
            }
        }

        private static void Render(Notegroup nG1, Notegroup nG2, Note nT1, Note nT2)
        {
            nG1.IsSpanned = true;
            nG2.IsSpanned = true;
            var sP = new Span();
            var pathBuilder = new StringBuilder();

            var ch = (from obj in Cache.Chords where obj.Id == nT1.Chord_Id select obj).First();
	        sP.FirstChord = ch;
            sP.Location_X = ch.Location_X + 19;
            sP.Location_Y = nT1.Location_Y + 25;
            sP.MeasureIndex = Measure.Index;
            sP.Measure_Id = Measure.Id;
            var dX = nG2.GroupX - nG1.GroupX - 1;
            var dY = (nG2.GroupY - nG1.GroupY);
            var dy = Math.Abs(dY);
            var n1Duration = nT1.Duration;
            var n2Duration = nT2.Duration;

            if (nT1.Orientation != null) sP.Orientation = (short)nT1.Orientation;

            #region Span Logic

            //here we handle full spanners
            var w = 5;
            const int lineWidth = 2;
            var step = 0;

            pathBuilder.AppendFormat(SpanFormatter, w * step, w * step - lineWidth, dX, (dY + w * step), (dY - w * step - lineWidth), 0);

            if (n1Duration <= .25M && n2Duration <= .25M)
            {
                step = 1;
                switch (sP.Orientation)
                {
                    case (short)_Enum.Orientation.Up:
                        pathBuilder.AppendFormat(SpanFormatter, w * step, w * step - lineWidth, dX, (dY + w * step), (dY + w * step - lineWidth), 0);
                        break;
                    case (short)_Enum.Orientation.Down:
                        pathBuilder.AppendFormat(SpanFormatter, -w * step, -(w * step + lineWidth), dX, (dY - w * step), (dY - w * step - lineWidth), 0);
                        break;
                }
            }
            if (n1Duration + n2Duration == .25M)
            {
                step = 2;
                switch (sP.Orientation)
                {
                    case (short)_Enum.Orientation.Up:
                        pathBuilder.AppendFormat(SpanFormatter, w * step, w * step - lineWidth, dX, (dY + w * step), (dY + w * step - lineWidth), 0);
                        break;
                    case (short)_Enum.Orientation.Down:
                        pathBuilder.AppendFormat(SpanFormatter, -w * step, -(w * step + lineWidth), dX, (dY - w * step), (dY - w * step - lineWidth), 0);
                        break;
                }
            }

            step = dy / 2;
            //here we handle partial spans
            if (n1Duration == .5M && n2Duration == .25M)
            {
                if (nG1.GroupY >= nG2.GroupY)
                {
                    w = ((short)_Enum.Orientation.Up == sP.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    pathBuilder.AppendFormat(SpanFormatter, w - step, w - step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                }
                else
                {
                    w = ((short)_Enum.Orientation.Up == sP.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    pathBuilder.AppendFormat(SpanFormatter, w + step, w + step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                }
            }
            else if (n1Duration == .25M && n2Duration == .5M)
            {
                if (nG1.GroupY >= nG2.GroupY)
                {
                    w = ((short)_Enum.Orientation.Up == sP.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    pathBuilder.AppendFormat(SpanFormatter, w, w - lineWidth, dX / 2, dY + w + step, dY + w + step - lineWidth, 0);
                }
                else
                {
                    w = ((short)_Enum.Orientation.Up == sP.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    pathBuilder.AppendFormat(SpanFormatter, w, w - lineWidth, dX / 2, dY + w - step, dY + w - step - lineWidth, 0);
                }
            }

            if (n1Duration == .5M && n2Duration == .125M)
            {
                if (nG1.GroupY >= nG2.GroupY)
                {
                    switch (sP.Orientation)
                    {
                        case (short)_Enum.Orientation.Up:
                            w = Math.Abs(w);
                            pathBuilder.AppendFormat(SpanFormatter, w - step, w - step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                            pathBuilder.AppendFormat(SpanFormatter, w * 2 - step, w * 2 - step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                            break;
                        case (short)_Enum.Orientation.Down:
                            w = -Math.Abs(w);
                            pathBuilder.AppendFormat(SpanFormatter, w - step, w - step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                            pathBuilder.AppendFormat(SpanFormatter, w * 2 - step, w * 2 - step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                            break;
                    }
                }
                else
                {
                    switch (sP.Orientation)
                    {
                        case (short)_Enum.Orientation.Up:
                            w = Math.Abs(w);
                            pathBuilder.AppendFormat(SpanFormatter, w + step, w + step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                            pathBuilder.AppendFormat(SpanFormatter, w * 2 + step, w * 2 + step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                            break;
                        case (short)_Enum.Orientation.Down:
                            w = -Math.Abs(w);
                            pathBuilder.AppendFormat(SpanFormatter, w + step, w + step - lineWidth, dX, dY + w, dY + w - lineWidth, dX / 2);
                            pathBuilder.AppendFormat(SpanFormatter, w * 2 + step, w * 2 + step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                            break;
                    }
                }
            }

            else if (n1Duration == .125M && n2Duration == .5M)
            {
                if (nG1.GroupY >= nG2.GroupY)
                {
                    w = ((short)_Enum.Orientation.Up == sP.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    pathBuilder.AppendFormat(SpanFormatter, w, w - lineWidth, dX / 2, dY + w + step, dY + w + step - lineWidth, 0);
                    pathBuilder.AppendFormat(SpanFormatter, w * 2, w * 2 - lineWidth, dX / 2, dY + w * 2 + step, dY + w * 2 + step - lineWidth, 0);
                }
                else
                {
                    w = ((short)_Enum.Orientation.Up == sP.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    pathBuilder.AppendFormat(SpanFormatter, w, w - lineWidth, dX / 2, dY + w - step, dY + w - step - lineWidth, 0);
                    pathBuilder.AppendFormat(SpanFormatter, w * 2, w * 2 - lineWidth, dX / 2, dY + w * 2 - step, dY + w * 2 - step - lineWidth, 0);
                }
            }

            if (n1Duration == .25M && n2Duration == .125M)
            {
                if (nG1.GroupY >= nG2.GroupY)
                {
                    w = ((short)_Enum.Orientation.Up == sP.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    pathBuilder.AppendFormat(SpanFormatter, w * 2 - step, w * 2 - step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                }
                else
                {
                    w = ((short)_Enum.Orientation.Up == sP.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    pathBuilder.AppendFormat(SpanFormatter, w * 2 + step, w * 2 + step - lineWidth, dX, dY + w * 2, dY + w * 2 - lineWidth, dX / 2);
                }
            }

            else if (n1Duration == .125M && n2Duration == .25M)
            {
                if (nG1.GroupY >= nG2.GroupY)
                {
                    w = ((short)_Enum.Orientation.Up == sP.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    pathBuilder.AppendFormat(SpanFormatter, w * 2, w * 2 - lineWidth, dX / 2, dY + w * 2 + step, dY + w * 2 + step - lineWidth, 0);
                }
                else
                {
                    w = ((short)_Enum.Orientation.Up == sP.Orientation) ? Math.Abs(w) : -Math.Abs(w);
                    pathBuilder.AppendFormat(SpanFormatter, w * 2, w * 2 - lineWidth, dX / 2, dY + w * 2 - step, dY + w * 2 - step - lineWidth, 0);
                }
            }

            #endregion span logic

            sP.Path = pathBuilder.ToString();
            if (LocalSpans == null)
                LocalSpans = new ObservableCollection<Span>();
            LocalSpans.Add(sP);
            GlobalSpans.Add(sP);
        }
    }
}
