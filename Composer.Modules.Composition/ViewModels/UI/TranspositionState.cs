using System.Globalization;
using System.Linq;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class TranspositionState
    {
        public string Slot { get; set; }
        public string Pitch { get; set; }
        public string Accidental { get; set; }
        public int? Accidental_Id { get; set; }
        public int DeltaSlot { get; set; }
        public int SlotPosition { get; set; }
        public int SlotIndex { get; set; }
        public int Octave { get; set; }
        public int Location_Y { get; set; }
        public Repository.DataService.Note Note { get; set; }
        public Infrastructure.Dimensions.Key Key { get; set; }

        public TranspositionState(Repository.DataService.Note note)
        {
            Pitch = GetPitch(note);
            Accidental_Id = note.Accidental_Id;
            Accidental = (Accidental_Id == null || Accidental == "") ? "" : (from a in Infrastructure.Dimensions.Accidentals.AccidentalList where a.Id == Accidental_Id select a.Name).DefaultIfEmpty("").Single();
            Octave = GetOctave(note.Pitch);
            Slot = note.Slot;
            Location_Y = note.Location_Y;
            SlotIndex = (from a in Infrastructure.Slot.Slots where a.Octave == Octave && a.Pitch == Pitch select a.Index).Single();
            SlotPosition = int.Parse((from a in Infrastructure.Slot.Slots where a.Octave == Octave && a.Pitch == Pitch select a.Value).Single().ToString(CultureInfo.InvariantCulture).ToCharArray()[1].ToString(CultureInfo.InvariantCulture));
        }

        private void SetPitch(_Enum.TranspositionMode mode, TranspositionState before, int interval)
        {
            string[] pitches = (from a in Infrastructure.Dimensions.Keys.KeyList
                                where (a.Name == before.Pitch)
                                select a.Scale).Single().Split(',')[interval].Split('-');

            if (mode == _Enum.TranspositionMode.Octave)
                Pitch = before.Pitch;
            else
                Pitch = (pitches.Length == 1) ? pitches[0] : (EditorState.TargetAccidental == "f") ? pitches[1] : pitches[0];
        }

        public TranspositionState(TranspositionState before, int rawInterval, int deltaOctave, _Enum.TranspositionMode mode, int interval)
        {
            SlotIndex = before.SlotIndex + rawInterval;

            SetOctave(before, mode, deltaOctave);

            SlotIndex = (SlotIndex >= 0) ? SlotIndex % 12 : (SlotIndex + 12) % 12;

            SetPitch(mode, before, interval);
            SetAccidental();
            
            SetSlot();
            DeltaSlot = int.Parse(Slot) - int.Parse(before.Slot);
            SetLocationY();
            Pitch = (Pitch.Length == 1) ? string.Format("{0}{1}", Pitch, Octave) : string.Format("{0}{1}{2}", Pitch.ToCharArray()[0], Octave, Pitch.ToCharArray()[1]);
        }

        private void SetSlot()
        {
            SlotPosition = int.Parse((from a in Infrastructure.Slot.Slots where a.Octave == Octave && a.Pitch == Pitch select a.Value).Single().ToString(CultureInfo.InvariantCulture).ToCharArray()[1].ToString(CultureInfo.InvariantCulture));
            Slot = string.Format("{0}{1}", Octave, SlotPosition);
        }

        private void SetOctave(TranspositionState before,_Enum.TranspositionMode mode, int deltaOctave)
        {
            Octave = before.Octave;
            if (mode == _Enum.TranspositionMode.Octave)
            {
                Octave = Octave + deltaOctave;
            }
            else
            {
                if (SlotIndex > 11)
                {
                    Octave++;
                }
                else if (SlotIndex < 0) Octave--;
            }
        }

        private string GetPitch(Repository.DataService.Note note)
        {
            var result = string.Empty;
            var pitch = note.Pitch;
            switch (pitch.Length)
            {
                case 1 :
                    result = pitch;
                    break;
                case 2:
                    result = pitch.ToCharArray()[0].ToString(CultureInfo.InvariantCulture).ToUpper();
                    break;
                case 3:
                    result = string.Format("{0}{1}", pitch.ToCharArray()[0].ToString(CultureInfo.InvariantCulture).ToUpper(), pitch.ToCharArray()[2].ToString(CultureInfo.InvariantCulture).ToLower());
                    break;
            }
            return result;
        }

        private int GetOctave(string pitch)
        {
            return int.Parse(pitch.ToCharArray()[1].ToString(CultureInfo.InvariantCulture));
        }

        private void SetLocationY()
        {
            var qry = (from a in Infrastructure.Slot.Slots
                       where (a.Octave.ToString(CultureInfo.InvariantCulture) == Octave.ToString(CultureInfo.InvariantCulture) && a.Pitch == Pitch)
                       select a);

            var locationY = qry.Single().Location_Y;

            locationY = locationY - Infrastructure.Constants.Measure.NoteHeight;
            Location_Y = locationY;
        }

        private void SetAccidental()
        {
            Accidental = (Pitch.Length == 2) ? Pitch.Substring(1) : "";
            Accidental_Id = null;
            if (Accidental != "")
            {
                Accidental_Id = (from a in Infrastructure.Dimensions.Accidentals.AccidentalList where a.Name == Accidental select a.Id).Single();
            }
        }
    }
}
