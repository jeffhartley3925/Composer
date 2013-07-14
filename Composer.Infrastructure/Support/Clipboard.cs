using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Composer.Infrastructure.Support
{
	public static class Clipboard
	{
		private static List<Composer.Repository.DataService.Note> notes = null;
		public static List<Composer.Repository.DataService.Note> Notes 
		{ 
			get { return notes; }
			set
			{
				notes = value;
				GroupNotesIntoChords();
			}
		}

		public static List<Composer.Repository.DataService.Chord> Chords 
        { 
            get; 
            set; 
        }

        private static void GroupNotesIntoChords()
		{
			//Note: this adds a chord to the Clipboard.Chords List if ANY note in the chord is selected.
			//So any chord in the Clipboard.Chords List may contain a note that was not selected.
			//this is OK, but just be aware that this is the way it is.

			//this would be easy to remedy by making another pass over each chord in the Clipboard.Chords list,
			//and remove all notes in the chord that are not in Clipboard.Notes list
			foreach (Repository.DataService.Note _note in Notes)
			{
				Repository.DataService.Chord _chord = (from a
													  in Cache.Chords
													  where a.Id == _note.Chord_Id
													  select a).DefaultIfEmpty(null).Single();
				if (_chord != null)
				{
					if (Clipboard.Chords == null)
					{
                        Clipboard.Chords = new List<Repository.DataService.Chord>();
					}
                    if (!Clipboard.Chords.Contains(_chord))
					{
                        Clipboard.Chords.Add(_chord);
					}
				}
			}
            Chords.Sort(
                delegate(Repository.DataService.Chord x, Repository.DataService.Chord y)
                {
                    return x.StartTime.Value.CompareTo(y.StartTime.Value);
                });
		}

		
	}
}
