using System.Linq;

using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository.DataService;

namespace Composer.Modules.Composition.ViewModels
{
	using System;
	using System.Collections.Generic;

	using Composer.Infrastructure;

	public class MeasuregroupViewModel : BaseViewModel, IEventCatcher
	{
		private double spacingRatio = 1;

		private double threshholdStarttime = 0;

		private Guid Id;

		public MeasuregroupViewModel(string mGiD)
		{
			if (string.IsNullOrWhiteSpace(mGiD)) return;
			Id = Guid.Parse(mGiD);
			SubscribeEvents();
		}

		public Chord LastCh { get; set; }

		private IEnumerable<Chord> _activeChs;
		public IEnumerable<Chord> ActiveChs
		{
			get { return this._activeChs ?? (this._activeChs = new List<Chord>()); }
			set
			{
				this._activeChs = value;
				this._activeChs = new List<Chord>(this._activeChs.OrderBy(p => p.StartTime));
			}
		}

		public void OnUpdateActiveChords(Tuple<Guid, Guid, int?, _Enum.Scope> payload)
		{
			var iD = payload.Item2;
			var scope = payload.Item4;
			if (IsTargetVM(iD, scope))
			{
				this.ActiveChs = Utils.GetMeasureGroupChords(iD, Guid.Empty, _Enum.Filter.Indistinct);
				if (this.ActiveChs.Any())
				{
					this.LastCh = (from c in this.ActiveChs select c).Last();
				}
			}
		}

		public void OnRespaceMeasuregroup(Guid mGiD)
		{
			if (IsTargetVM(mGiD) && !EditorState.IsOpening)
			{
				Chord prevCh = null;
				var distinctStChs = this.ActiveChs.GroupBy(p => p.StartTime).Select(g => g.First()).ToList();
				foreach (var cH in distinctStChs)
				{
					if (cH.StartTime < threshholdStarttime)
					{
						prevCh = cH;
						continue;
					}
					var x = (prevCh == null) ? 7 : ChordManager.GetProportionalLocationX(prevCh, spacingRatio);
					if (cH.StartTime != null)
						EA.GetEvent<SetChordLocationX>().Publish(new Tuple<Guid, int, double>(cH.Id, x, (double)cH.StartTime));
					prevCh = cH;
				}
				ArrangeMeasure(mGiD);
				threshholdStarttime = 0;
			}
		}

		/// <summary>
		/// Respace lyrics. redraw slurs and ties, etc
		/// </summary>
		/// <param name="mGiD"></param>
		private void ArrangeMeasure(Guid mGiD)
		{
			var mG = MeasuregroupManager.GetMeasuregroup(mGiD);
			foreach (var mE in mG.Measures)
			{
				EA.GetEvent<ArrangeMeasure>().Publish(mE.Id);
			}
		}

		public void OnSetThreshholdStarttime(Tuple<Guid, double> payload)
		{
			var mGiD = payload.Item1;
			if (!IsTargetVM(mGiD)) return;
			threshholdStarttime = payload.Item2;
		}

		/// <summary>
		/// This event is thrown n times where n is the number of measures in the measure group. We only need to catch this
		/// event, once per resize action since all measures in the measure group use the same spacing ratio. so the measure 
		/// group spacingRatio is purposefully taken from the measure containing the last chord in the measure group. see
		/// IsTargetVM(Guid mGiD, Guid mEiD)
		/// </summary>
		/// <param name="payload">Item1 = Measuregroup.Id, Item2 = Measure.Id, Item3 = ratio</param>
		public void OnUpdateMeasureSpacingRatio(Tuple<Guid, Guid, double> payload)
		{
			Guid mGiD = payload.Item1;
			Guid mEiD = payload.Item2;
			if (IsTargetVM(mGiD, mEiD))
			{
				this.spacingRatio = payload.Item3;
			}
		}

		public void OnBumpMeasuregroupWidth(Tuple<Guid, double, int> payload)
		{
			Guid mGiD = payload.Item1;
			if (!IsTargetVM(mGiD)) return;
			var mG = MeasuregroupManager.GetMeasuregroup(mGiD);
			foreach (var mE in mG.Measures)
			{
				payload = new Tuple<Guid, double, int>(mE.Id, payload.Item2, payload.Item3);
				EA.GetEvent<BumpMeasureWidth>().Publish(payload);
			}
		}

		public void OnResizeMeasuregroup(object obj)
		{
			var widthChange = (WidthChange)obj;
			if (!IsTargetVM(widthChange.MeasuregroupId)) return;
			var mG = MeasuregroupManager.GetMeasuregroup(widthChange.MeasuregroupId);
			foreach (var mE in mG.Measures)
			{
				widthChange.MeasureId = mE.Id;
				EA.GetEvent<ResizeMeasure>().Publish(widthChange);
			}
		}

		public void SubscribeEvents()
		{
			EA.GetEvent<BumpMeasuregroupWidth>().Subscribe(OnBumpMeasuregroupWidth);
			EA.GetEvent<UpdateActiveChords>().Subscribe(OnUpdateActiveChords);
			EA.GetEvent<RespaceMeasuregroup>().Subscribe(OnRespaceMeasuregroup);
			EA.GetEvent<ResizeMeasuregroup>().Subscribe(OnResizeMeasuregroup);
			EA.GetEvent<UpdateMeasureSpacingRatio>().Subscribe(OnUpdateMeasureSpacingRatio);
			EA.GetEvent<SetThreshholdStarttime>().Subscribe(OnSetThreshholdStarttime);
		}

		public void DefineCommands()
		{
			throw new NotImplementedException();
		}

		public bool IsTargetVM(Guid mGiD)
		{
			return Id == mGiD;
		}

		public bool IsTargetVM(Guid mGiD, Guid mEiD)
		{
			if (this.LastCh == null) return false;
			return this.Id == mGiD && this.LastCh.Measure_Id == mEiD;
		}

		public bool IsTargetVM(Guid iD, _Enum.Scope scope)
		{
			return this.Id == iD && (scope == _Enum.Scope.All || scope == _Enum.Scope.Measuregroup);
		}
	}
}
