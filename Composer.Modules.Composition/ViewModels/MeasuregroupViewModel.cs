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
				//if (this._activeChs == null) return;
				this._activeChs = new List<Chord>(this._activeChs.OrderBy(p => p.StartTime));
			}
		}

		public void OnUpdateActiveChords(Tuple<Guid, Guid, int?, _Enum.Scope> payload)
		{
			var mEiD = payload.Item1;
			var mGiD = payload.Item2;
			var scope = payload.Item4;
			if (IsTargetVM(mGiD, scope))
			{
				var cHs = Utils.GetMeasureGroupChords(mEiD, Guid.Empty, _Enum.Filter.Indistinct);
				if (cHs == null) return;
				this.ActiveChs = cHs;
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
				Chord pVcH = null;
				var cHs = this.ActiveChs.GroupBy(p => p.StartTime).Select(g => g.First()).ToList();
				foreach (var cH in cHs)
				{
					if (cH.StartTime < threshholdStarttime)
					{
						pVcH = cH;
						continue;
					}
					var x = (pVcH == null) ? 7 : ChordManager.GetProportionalLocationX(pVcH, spacingRatio);
					if (cH.StartTime != null)
					{
						EA.GetEvent<SetChordLocationX>().Publish(new Tuple<Guid, int, double>(cH.Id, x, (double)cH.StartTime));
					}
					pVcH = cH;
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

		/// </summary>
		/// <param name="payload">Item1 = Measuregroup.Id, Item2 = ratio</param>
		public void OnUpdateMeasuregroupSpacingRatio(Tuple<Guid, double> payload)
		{
			Guid mGiD = payload.Item1;
			if (IsTargetVM(mGiD))
			{
				this.spacingRatio = payload.Item2;
			}
		}

		public void OnBumpMeasuregroupWidth(Tuple<Guid, double?, int> payload)
		{
			Guid mGiD = payload.Item1;
			if (!IsTargetVM(mGiD)) return;
			var mG = MeasuregroupManager.GetMeasuregroup(mGiD);
			foreach (var mE in mG.Measures)
			{
				payload = new Tuple<Guid, double?, int>(mE.Id, payload.Item2, payload.Item3);
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
			EA.GetEvent<UpdateMeasuregroupSpacingRatio>().Subscribe(this.OnUpdateMeasuregroupSpacingRatio);
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
