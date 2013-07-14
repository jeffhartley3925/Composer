
using System.Globalization;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class TranspositionData
    {
        public string Start_Y { get; set; }
        public string End_Y { get; set; }
        public string StartSlot { get; set; }
        public string EndSlot { get; set; }
        public string StartPitch { get; set; }
        public string EndPitch { get; set; }
        public string StartOctave { get; set; }
        public string EndOctave { get; set; }
        public string Message { get; set; }

        public TranspositionData AddTranspositionRecord(TranspositionData data, string i, string c, string s, Repository.DataService.Note note)
        {
            data = new TranspositionData
                       {
                           StartOctave = note.Octave_Id.ToString(),
                           Start_Y = note.Location_Y.ToString(CultureInfo.InvariantCulture),
                           StartSlot = note.Slot,
                           StartPitch = note.Pitch
                       };

            return data;
        }

        public TranspositionData AddTranspositionRecord(TranspositionData data, string d, Repository.DataService.Note note, string l, string o, string s, int deltaSlot)
        {
            data.EndOctave = string.Format("{0}{1}", note.Octave_Id, o);
            data.End_Y = string.Format("{0}{1}", note.Location_Y, l);
            data.EndSlot = string.Format("{0}{1} ({2})", note.Slot, s, deltaSlot);
            data.EndPitch = note.Pitch;
            return data;
        }

        public TranspositionData Initialize()
        {
            var data = new TranspositionData
                           {
                               StartPitch = "Pitch0",
                               EndPitch = "Pitch1",
                               StartOctave = "Octave0",
                               EndOctave = "Octave1",
                               Start_Y = "Y0",
                               End_Y = "Y1",
                               StartSlot = "Slot0",
                               EndSlot = "Slot1"
                           };
            return data;
        }
    }
}