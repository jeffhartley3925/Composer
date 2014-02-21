using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels
{
    public class SpacingHelper
    {
        public SpacingHelper(Repository.DataService.Measure activeM, double? st)
        {
            ActiveMeasure = activeM;
            Sequence = activeM.Sequence;
            Starttime = st;
            Mode = _Enum.NotePlacementMode.Append;
            LeftChord = null;
            RightChord = null;
            ShiftDirection = _Enum.Direction.None;
        }

        public Repository.DataService.Measure ActiveMeasure;
        public _Enum.NotePlacementMode Mode { get; set; }
        public int Sequence { get; set; }
        public double? Starttime { get; set; }
        public _Enum.Direction ShiftDirection { get; set; }
        public Repository.DataService.Chord LeftChord { get; set; }
        public Repository.DataService.Chord RightChord { get; set; }
    }
}
