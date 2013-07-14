using Composer.Modules.Composition.ViewModels;
using System.Collections.Generic;

namespace Composer.Modules.Composition
{
	public static class _ViewModels
	{
		public static List<StaffgroupViewModel> staffgroups;
		public static List<StaffViewModel> staffs;
		public static List<MeasureViewModel> measures;
		public static List<ChordViewModel> chords;
		public static List<NoteViewModel> notes;

		public static void Initialize()
		{
			staffgroups = new List<StaffgroupViewModel>();
			staffs = new List<StaffViewModel>();
			measures = new List<MeasureViewModel>();
			chords = new List<ChordViewModel>();
			notes = new List<NoteViewModel>();
		}
	}

	public class Author
	{
		private string _name = string.Empty;
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		private string _pictureUrl = string.Empty;
		public string PictureUrl
		{
			get { return _pictureUrl; }
			set { _pictureUrl = value; }
		}

		private double _percentContribution = 100;
		public double PercentContribution
		{
			get { return _percentContribution; }
			set { _percentContribution = value; }
		}
	}
}

