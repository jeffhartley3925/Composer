using System;
using Composer.Infrastructure;
using System.Collections.Generic;

namespace Composer.Modules.Composition.Views
{
	public class PrintPageItem
	{
		private string _titleFontFamily = Preferences.ProvenanceFontFamily;
		public string TitleFontFamily
		{
			get { return _titleFontFamily; }
			set { _titleFontFamily = value; }
		}

		private string _smallFontFamily = Preferences.ProvenanceSmallFontFamily;
		public string SmallFontFamily
		{
			get { return _smallFontFamily; }
			set { _smallFontFamily = value; }
		}

		private string _titleFontSize = Preferences.ProvenanceTitleFontSize;
		public string TitleFontSize
		{
			get { return _titleFontSize; }
			set { _titleFontSize = value; }
		}

		private string _smallFontSize = Preferences.ProvenanceSmallFontSize;
		public string SmallFontSize
		{
			get { return _smallFontSize; }
			set { _smallFontSize = value; }
		}

		private string _title = "";
		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		private Repository.DataService.Staffgroup _staffgroup;
		public Repository.DataService.Staffgroup Staffgroup
		{
			get { return _staffgroup; }
			set { _staffgroup = value; }
		}

		private string _date = DateTime.Now.ToLongDateString();
		public string Date
		{
			get { return _date; }
			set { _date = value; }
		}

		private List<Author> _authors = new List<Author>();
		public List<Author> Authors
		{
			get { return _authors; }
			set { _authors = value; }
		}
	}
}
