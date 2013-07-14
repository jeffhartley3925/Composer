using System;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class Word
    {
        public double StartTime { get; set; }

        public double? Location_X { get; set; }

        public int? Index { get; set; }

        public string Text { get; set; }

        public Guid MeasureId { get; set; }

        //alignment value is only used by the WordViewModel to override the Canvas.Left
        //value, not to bind to the HorizontalAligment attribute.
        public string Alignment { get; set; }

        public Word(Guid measureId, double startTime, int? index, string text, double? locationX, string alignment)
        {
            MeasureId = measureId;
            StartTime = startTime;
            Index = index;
            Text = text;
            Location_X = locationX;
            Alignment = alignment;
        }
    }
}