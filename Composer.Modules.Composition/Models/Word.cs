using System;

namespace Composer.Modules.Composition.Models
{
    public sealed class Word
    {
        public double StartTime { get; set; }

        public double? Location_X { get; set; }

        public int? Index { get; set; }

        public string Text { get; set; }

        public Guid MeasureId { get; set; }

        /// <summary>
        /// Alignment property is used by the WordViewModel to override the elements Canvas.Left
        /// property, not to bind to the elements' HorizontalAligment attribute.
        /// </summary>
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