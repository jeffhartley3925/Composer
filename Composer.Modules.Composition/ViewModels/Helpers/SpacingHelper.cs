using System;
using System.Collections.Generic;
using System.Linq;
using Composer.Infrastructure;
using Composer.Repository.DataService;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public class SpacingHelper
    {
        private Chord leftChord = null;
        public Chord LeftChord
        {
            get { return leftChord; }
            set 
            { 
                leftChord = value;
                if (leftChord != null)
                {
                    ChordsWithSameLeftChordStarttime = GetChordsWithSameLeftChordStarttime(ActiveMeasure, leftChord.StartTime);
                    var ch = (this.ChordsWithSameLeftChordStarttime.Where(b => b.Measure_Id == ActiveMeasure.Id)).DefaultIfEmpty(null).First();
                    if (ch != null && leftChord.Id != ch.Id) leftChord = ch;
                }
            }
        }

        public SpacingHelper(Repository.DataService.Measure activeM, double? st)
        {
            ActiveMeasure = activeM;
            Sequence = activeM.Sequence;
            Starttime = st;
            Mode = _Enum.NotePlacementMode.Append;
            RightChord = null;
            LeftChord = null;
            ShiftDirection = _Enum.Direction.None;
            ActiveStaffgroup = Utils.GetStaffgroup(activeM);
            ChordsWithSameLeftChordStarttime  = null;
        }

        private List<Chord> GetChordsWithSameLeftChordStarttime(Measure activeM, double? st)
        {
            if (LeftChord == null) return null;
            return Utils.GetMeasureGroupChords(activeM.Id, Guid.Empty, _Enum.Filter.Indistinct).Where(b => b.StartTime == LeftChord.StartTime).ToList();
        }

        public Repository.DataService.Staffgroup ActiveStaffgroup;
        public Repository.DataService.Measure ActiveMeasure;
        public _Enum.NotePlacementMode Mode { get; set; }
        public int Sequence { get; set; }
        public double? Starttime { get; set; }
        public _Enum.Direction ShiftDirection { get; set; }
        public Repository.DataService.Chord RightChord { get; set; }
        public List<Chord> ChordsWithSameLeftChordStarttime { get; set; }
    }
}