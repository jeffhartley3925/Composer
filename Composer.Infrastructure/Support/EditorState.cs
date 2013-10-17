using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure.Support;
using Composer.Infrastructure.Constants;
using System.Windows.Controls;

namespace Composer.Infrastructure
{
	public static class EditorState
	{
        private static _Enum.StaffConfiguration _staffConfiguration = _Enum.StaffConfiguration.None;
        public static _Enum.StaffConfiguration StaffConfiguration
        {	
            get { return _staffConfiguration; }
            set
            {
                _staffConfiguration = value;
                SetMeasureResizeScope();
            }
        }

        private static void SetMeasureResizeScope()
        {
            switch (StaffConfiguration)
            {
                case _Enum.StaffConfiguration.Simple:
                    _measureResizeScope = _Enum.MeasureResizeScope.Staff;
                    break;
                case _Enum.StaffConfiguration.Grand:
                    _measureResizeScope = _Enum.MeasureResizeScope.Staffgroup;
                    break;
                case _Enum.StaffConfiguration.MultiInstrument:
                    _measureResizeScope = _Enum.MeasureResizeScope.Staffgroup;
                    break;
                default:
                    _measureResizeScope = _Enum.MeasureResizeScope.Staff;
                    break;
            }
            //HARD CODED VALUE
            _measureResizeScope = _Enum.MeasureResizeScope.Composition;
        }

        private static _Enum.MeasureResizeScope _measureResizeScope;
        public static _Enum.MeasureResizeScope MeasureResizeScope
        {
            get { return _measureResizeScope; }
            set
            {
                _measureResizeScope = value;
            }
        }

        //globalStaffWidth is meaningfully calculable only when meausreResizeScope is Global or Composition. Otherwise,
        //staff width can be found in the ViewModel of each staff. phase 1 forces meausreResizeScope = 'Global' so for now,
        //GlobalStaffWidth will be accurate for every staff. 
        private static double _globalStaffWidth = 0;
        public static double GlobalStaffWidth
        {
            get { return _globalStaffWidth; }
            set
            {
                _globalStaffWidth = 0;
                foreach (var staff in Cache.Staffs)
                {
                    double w = (from a in staff.Measures select double.Parse(a.Width)).Sum() + Defaults.StaffDimensionWidth + Defaults.CompositionLeftMargin - 70;
                    if (w > _globalStaffWidth)
                    {
                        _globalStaffWidth = w;
                    }
                }
            }
        }

        //NoteSpacingRatio only used to increase proportional note spacing so the chords will fill a packed measure.
            //0. sometimes we want to reduce a measures width to match the the chords as they are currently spaced. other 
            //   times we need to increase the spacing of chords to fill the measure as it is.
            //1. widest measure in a sequence is 400
            //2. the packed width of a measure under edit would be 300
            //3. we need to increase the proportional note spacing of the measure so that its packed width is 400.
            //4. in this special case we change NoteSpacingRatio from 1 to 400/300, then immediately back to 1 when the 
            //   operation is over
        public static double NoteSpacingRatio = 1;

        public static bool Dirty = false;
        public static int IdIdToUse = 1;
        public static Guid ActiveMeasureId = Guid.Empty;
        public static double Ratio = 1;
        public static string TargetAccidental = "";
        public static List<string> AccidentalNotes = null;
        public static int AccidentalCount = 0;
        public static int AccidentalWidth = 0;
        public static int BlurRadius = Preferences.DefaultBlurRadius;
        public static int VerseCount = 0;
        public static int  LoadedActiveMeasureCount  = 0;
        public static int ActiveMeasureCount = 0;
        public static string Host = string.Empty;
		public static bool Collaborating = false;
        public static bool Provenancing    = false;
        public static bool IsEditingLyrics   = false;
        public static bool IsCalculatingStatistics = false;
        public static bool IsTransposing = false;
        public static bool IsOpening = false;
        public static int RunningLoadedMeasureCount = 0;
		public static bool Cloning 	   = false;
        public static bool ArcsLoaded 	   = false;
        public static _Enum.ReplaceMode ReplacementMode = _Enum.ReplaceMode.None;
        public static bool UseVerboseMouseTrackers = false;  
        public static bool IsPrinting = false;
        public static bool IsFacebookDataLoaded = true;
        public static bool IsAddingStaffgroup = false;
        public static bool IsLoggedIn = false;
        public static bool IsOverBar = false;
        public static bool IsOverNote = false;
        public static bool IsResizingMeasure = false;
        public static bool IsOverArc = false;
		public static bool IsCollaboration 	   = false;
        public static bool IsNoteEditor = false;
        public static bool IsNewCompositionPanel = false;
		public static bool IsAuthor 		   = false;
        //playback
        public static bool IsPlaying = false;
        public static bool IsPaused = false;
        public static Guid ActivePlaybackControlId = Guid.Empty;
        public static double ResumeStarttime = 0;
        //
		public static bool IsComposing 		   = false;
        public static bool IsPasting           = false;
        public static bool IsSaving = false;
		public static bool IsReturningContributor 	   = false;
		public static bool Purgable 	   = false;
		public static Guid qsId 	   = Guid.Empty;
        public static string qsIndex = "0";
		public static Guid MidQueryString 	   = Guid.Empty;
		public static string UidQueryString   = string.Empty;
		public static int ChordSpacing 		   = 30;
		public static double ViewportWidth 	   = 0;
		public static double ViewportHeight    = 0;

		public static string VectorClass;
		public static string PaletteId;
		public static _Enum.EditContext EditContext;
		public static string ToolType;
		public static string Tool;
		public static bool? Dotted;
		public static bool DoubleClick = false;
		public static bool EnableVerseSynchronization = false;
		public static string Accidental;
		public static string DurationType;
		public static string PlaybackMode;
		public static string DurationCaption;
		public static double? Duration;

		public static int VectorId;
        public static bool IsInternetAccess = true;

		public static Repository.DataService.Chord Chord { get; set; }
		public static _Enum.ClickState ClickState = _Enum.ClickState.None;
		public static string ClickMode = "None";

		public static void Reset()
		{
			IsCollaboration = false;
			IsAuthor 		= false;
			IsComposing 	= false;
			DoubleClick 	= false;
			ToolType 		= null;
			Tool 			= null;
			DurationType 	= null;
			DurationCaption = null;
			Duration 		= null;
			Dotted 			= null;
			Accidental 		= string.Empty;
		}

		public static bool IsQueryStringSource()
		{
			return (qsId != Guid.Empty);
		}

		public static Boolean DurationSelected()
		{
			return IsNote() || IsRest();
		}

		public static Boolean IsNote()
		{
			return DurationType == "Note";
		}

		public static Boolean IsRest()
		{
			return DurationType == "Rest";
		}

		public static Boolean DotSelected()
		{
			return (Dotted != null);
		}

		public static Boolean AccidentalSelected()
		{
			return (!string.IsNullOrEmpty(Accidental));
		}

		public static Boolean ToolSelected()
		{
			return !string.IsNullOrEmpty(Tool);
		}

		public static void SetTool(string tool)
		{
			DurationCaption = null;
			DurationType 	= null;
			Accidental 		= string.Empty;
			Dotted 			= null;
			Tool 			= tool;
		}

		public static void SetContext(string vectorName, string vectorClass1, string vectorClass2)
		{
			DurationCaption = vectorName;
			DurationType 	= vectorClass1;
			VectorClass 	= vectorClass2;
			if (IsRest())
			{
				Accidental = null;
			}
		}

		public static bool SetRestContext()
		{
		    if (Duration != null) DurationCaption = GetDurationCaptionAndVectorNameFromDuration((double)Duration);
		    DurationType = _Enum.VectorClass.Rest.ToString();
			VectorClass 	= _Enum.VectorClass.Rest.ToString();
			if (IsRest())
			{
				Accidental = null;
			}
			var b = (from a in Vectors.VectorList where a.Class == _Enum.VectorClass.Rest.ToString() && a.Name == DurationCaption select a).DefaultIfEmpty(null).First();
            if (b != null)
            {
                VectorId = b.Id;
                return true;
            }
            return false;
		}

		public static string GetDurationCaptionAndVectorNameFromDuration(double duration)
		{
			string vectorName = string.Empty;
			double d = duration * 1000;
			switch (d.ToString(CultureInfo.InvariantCulture))
			{
				case "4000":
					vectorName = "Whole";
					break;
				case "6000":
					vectorName = "DottedWhole";
					break;
				case "2000":
					vectorName = "Half";
					break;
				case "3000":
					vectorName = "DottedHalf";
					break;
				case "1000":
					vectorName = "Quarter";
					break;
				case "1500":
					vectorName = "DottedQuarter";
					break;
				case "500":
					vectorName = "Eighth";
					break;
				case "750":
					vectorName = "DottedEighth";
					break;
				case "250":
					vectorName = "Sixteenth";
					break;
				case "375":
					vectorName = "DottedSixteenth";
					break;
				case "125":
					vectorName = "Thirtysecond";
					break;
				case "187.5":
					vectorName = "DottedThirtysecond";
					break;
			}
			return vectorName;
		}

		public static void SetPlayback(string mode)
		{
			PlaybackMode = mode;
		}
	}
}