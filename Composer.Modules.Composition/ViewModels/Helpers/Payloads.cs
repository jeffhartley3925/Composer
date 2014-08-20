using System;
using System.Collections.ObjectModel;
using Composer.Modules.Composition.Models;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public struct WidthChange
    {
		public Guid MeasuregroupId;
        public Guid MeasureId;
		public int MeasureIndex;
        public Guid StaffId;
        public Guid StaffgroupId;
        public int Width;
        public int? Sequence;
		
        public bool IsResizeStartMeasure;

		public WidthChange(Guid mGiD, Guid mEiD, Guid sFiD, Guid sGiD, int? sQ, int wI, int mEiX)
		{
			MeasuregroupId = mGiD;
            MeasureId = mEiD;
			MeasureIndex = mEiX;
            StaffgroupId = sGiD;
            StaffId = sFiD;
            IsResizeStartMeasure = false;
            Sequence = sQ;
            Width = wI;
        }
    }

    public class SpanPayload
    {
        public Repository.DataService.Measure Measure { get; set; }

        public ObservableCollection<Span> LocalSpans { get; set; }

        public SpanPayload(Repository.DataService.Measure measure, ObservableCollection<Span> localSpans)
        {
            Measure = measure;
            LocalSpans = localSpans;
        }
    }
}
