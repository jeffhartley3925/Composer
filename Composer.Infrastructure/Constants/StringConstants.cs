namespace Composer.Infrastructure.Constants
{
    public static class PlaybackFunction
    {
        public const string Play 		  	   = "Play";
        public const string Pause 		  	   = "Pause";
        public const string Stop 		  	   = "Stop";
    }

    public static class _Accidental
    {
        public const string Flat 		  	   = "Flat";
        public const string Sharp 		  	   = "Sharp";
        public const string Natural 	  	   = "Natural";
    }

	public static class Tool
	{
		public const string Clone 		  	   = "Clone";
		public const string Tie 		  	   = "Tie";
		public const string Slur 		  	   = "Slur";
		public const string Span 		  	   = "Span";
		public const string Select 		  	   = "Select";
		public const string AreaSelect 	  	   = "AreaSelect";
		public const string Stem 		  	   = "Stem";
		public const string Erase 		  	   = "Erase";
		public const string Edit 		  	   = "Edit";
	}

    public static class ClefName
    {
        public const string Treble 		  	   = "Treble";
        public const string Bass 		  	   = "Bass";
        public const string Alto 		  	   = "Alto";
    }

    public static class DimensionName
    {
        public const string Key 		  	   = "Key";
        public const string Clef 		  	   = "Clef";
        public const string Instrument 	  	   = "Instrument";
        public const string Bar 		  	   = "Bar";
        public const string TimeSignature 	   = "TimeSignature";
    }

    public static class DurationName
    {
        public const string Thirtysecond 	   = "Thirtysecond";
        public const string Sixteenth 		   = "Sixteenth";
        public const string Eighth 			   = "Eighth";
        public const string Quarter 		   = "Quarter";
        public const string Half 			   = "Half";
        public const string Whole 			   = "Whole";
        public const string DottedThirtysecond = "DottedThirtysecond";
        public const string DottedSixteenth    = "DottedSixteenth";
        public const string DottedEighth 	   = "DottedEighth";
        public const string DottedQuarter 	   = "DottedQuarter";
        public const string DottedHalf 		   = "DottedHalf";
        public const string DottedWhole 	   = "DottedWhole";
    }

	public static class EditActions
	{
		public const string Copy 			= "Copy";
		public const string Paste 			= "Paste";
		public const string InsertBar 	    = "Bar";
		public const string Delete 			= "Delete";
        public const string Flip = "Flip";
	}

    public static class ObjectName
    {
        public const string Measure 		   = "Measure";
        public const string Staff 			   = "Staff";
        public const string Composition 	   = "Composition";
        public const string Chord 			   = "Chord";
        public const string Note 			   = "Note";
        public const string Span 			   = "Span";
        public const string Rest 			   = "Rest";
        public const string Staffgroup 		   = "Staffgroup";
        public const string Slur 			   = "Slur";
        public const string Tie 			   = "Tie";
    }

    public class RegionNames
    {
        public const string Collaborations 	   = "CollaborationRegion";
        public const string Provenance 		   = "ProvenanceRegion";
        public const string BusyIndicator 	   = "BusyIndicatorRegion";
        public const string Composition 	   = "CompositionRegion";
        public const string UIScale 		   = "UIScaleRegion";
        public const string Staffgroup 		   = "StaffgroupRegion";
        public const string Staff 			   = "StaffRegion";
        public const string Measure 		   = "MeasureRegion";
		public const string Print 			   = "PrintRegion";
        public const string EditPopup 		   = "EditPopupRegion";
        public const string LyricsPanel        = "LyricsPanelRegion";
        public const string SavePanel          = "SavePanelRegion";
        public const string NewComposition     = "NewCompositionPanelRegion";
        public const string Hub                = "HubRegion";
        public const string Bars               = "BarsRegion";
        public const string Transposition      = "TranspositionRegion";
        public const string NoteEditor         = "NoteEditorRegion";
    }

    public class Lyrics
    {
        public const string SplitChordHyphen = "--";
        public const string SplitWordHyphen = "-";
    }

    public static class StyleTarget
    {
        public const string StaffLines_Measure      = "StaffLines_Measure";
        public const string StaffLines_Staff        = "StaffLines_Staff";
        public const string Selection  		        = "Selection";
    }

    public static class Messages
    {
        public const string NewCompositionPanelTitlePrompt = "Type the Composition Title here";
        public const string NewCompositionPanelTitleValidationPrompt = "Give Your Composition a Title";
        public const string NewCompositionPanelTitleLengthPrompt = "Maximum Title length is 128 characters.";
    }
}