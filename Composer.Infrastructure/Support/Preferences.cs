using System;
using System.Windows;
using System.Windows.Media;

namespace Composer.Infrastructure
{
    public static class Preferences
    {
        //General
        public static string ListBoxSelectedItemBackground = "#74AD5A"; // must be light enough for dark text to be readable.
        public static int FacebookPictureSize = 24;
        public static bool MergeGlobally = true;

		//Composition
        public static string SelectorColor = "#99ccff"; // must be dark enough for white text to be readable.
        public static string DisabledColor = "#CCCCCC";
        public static _Enum.StaffConfiguration DefaultStaffConfiguration = _Enum.StaffConfiguration.Grand;
        public static string CompositionBackground = "Transparent";
        public static string CompositionScrollBackground = CompositionBackground;

		//Note
        public static string NoteSelectorColor = SelectorColor;
        public static short NoteStrokeThickness = 0;
        public static string NoteForeground = "#000000";
        public static string NoteStroke = "#000000";
        public static double NoteOpacity = 1;

        //Bar
        public static string BarSelectorColor = SelectorColor;
        public static string BarForeground = NoteForeground;
        public static string BarBackground = CompositionBackground;

		//Arc
        public static string ArcSelectorColor = NoteSelectorColor;
        public static short ArcStrokeThickness = 1;
        public static string ArcForeground = NoteForeground;
        public static string ArcBackground = "Transparent";
        public static string ArcHighlightBackground = "Transparent";
        public static string ArcStroke = NoteStroke;
        public static double ArcOpacity = NoteOpacity;
        public static string ArcHighlightStrokeWidth = "4";
        public static int ArcFlareSize = 12;
        public static int ArcDefaultDepth = 160;

        //Ledger
        public static string LedgerStroke = NoteStroke;
        public static double LedgerOpacity = 1;

		//Span
        public static string SpanSelectorColor = NoteSelectorColor;
        public static short SpanStrokeThickness = 0;
        public static string SpanFill = NoteForeground;
        public static string SpanForeground = NoteForeground;
        public static string SpanStroke = NoteStroke;
        public static double SpanOpacity = NoteOpacity;
        public static int LargeOkToSpanThreshhold = 32;
        public static int MediumOkToSpanThreshhold = 30;
        public static int SmallOkToSpanThreshhold = 34;

        //Verse
        public static int LyricsTopMargin = 0;
        public static int VerseWordTopMargin = 0;
        public static int VerseWordLeftMargin = 4;

        //these 2 are inversely related. eg: increase one, decrease the other by same amount.
        public static int VerseNumbersLeftMargin = 13;
        //end

        public static string VerseFontFamily = "Lucida Sans Unicode";
        public static string VerseFontWeight = "Normal";
        public static int VerseFontSize = 12;
        public static string VerseForeground = Preferences.NoteForeground;

        public static string LyricsPanelSelectedVerseBackground = Application.Current.Resources["DarkBlue"].ToString();
        public static string LyricsPanelVerseBackground = "#efefef";

        public static string LyricsPanelSelectedVerseForeground = "#FFFFFF";
        public static string LyricsPanelVerseForeground = "#3d599a";
        public static string LyricsPanelRemovedVerseForeground = DisabledColor;

        public static string LyricsPanelEditorEnabledForeground = "#3b5998";
        public static string LyricsPanelEditorDisabledForeground = "#222222";

        //Measure

        public static string MeasureForeground = NoteForeground;
        public static string MeasureBackground = "Transparent";
        public static string ActiveMeasureBackground = "#cdcdcd";
        public static int NewComppositionPanelMeasureWidth = 200;
        public static int CompositionMeasureWidth = 270;
        public static int MeasureWidth = CompositionMeasureWidth;

        public static int MeasureMaximumEditingSpace = 50; //amount added to measure width when a measure is not packed but there's no more room to place chords. 
        public static int MeasureMinimumEditingSpace = 50;
        public static int NewComppositionPanelStaffDimensionAreaWidth = 201;
        public static int CompositionStaffDimensionAreaWidth = 104;
        public static int StaffDimensionAreaWidth = CompositionStaffDimensionAreaWidth;
        
        public static int MeasureTopDeadSpace = 5;
        public static int MeasureBottomDeadSpace = 13;
        public static _Enum.MeasureSpacingMode SpacingMode = _Enum.MeasureSpacingMode.Proportional;
        public static _Enum.MeasureArrangementMode MeasureArrangeMode = _Enum.MeasureArrangementMode.DecreaseMeasureWidth;
        public static Visibility MeasureDebugInfoVisibility = Visibility.Collapsed;

        //Staff
        public static string HyperlinkSelectedForeground = ((SolidColorBrush)Application.Current.Resources["HyperlinkSelectedForeground"]).Color.ToString();
        public static string HyperlinkSelectedBackground = ((SolidColorBrush)Application.Current.Resources["HyperlinkSelectedBackground"]).Color.ToString();
        public static string HyperlinkForeground = ((SolidColorBrush)Application.Current.Resources["HyperlinkForeground"]).Color.ToString();
        public static string HyperlinkBackground = ((SolidColorBrush)Application.Current.Resources["HyperlinkBackground"]).Color.ToString();
        public static string StaffForeground = NoteForeground;

        //Playback controls
        public static string PlaybackControlColor_Default = HyperlinkForeground;
        public static string PlaybackControlColor_Selected = "#99ccff";
        public static string PlaybackControlColor_Hover = "Pink";
        
        public static double PlaybackControlStrokeThickness_Default = 1;
        public static double PlaybackControlStrokeThickness_Selected = 1;
        public static double PlaybackControlStrokeThickness_Hover = 1;

        public static string PlaybackControlStrokeColor_Default = "#000000";
        public static string PlaybackControlStrokeColor_Selected = "#336699";
        public static string PlaybackControlStrokeColor_Hover = "Red";
        
        public static double PlaybackControlOpacity_Disabled = .4;
        public static double PlaybackControlOpacity_Enabled = 1;

        public static double PlaybackControlButtonScale_Default = 1;
        public static double PlaybackControlButtonScale_Selected = 1.5;

        //Provenance
        public static string ProvenanceFontFamily = "Lucida Sans Unicode";
		public static string ProvenanceSmallFontFamily = "Lucida Sans Unicode";
        public static string ProvenanceTitleFontSize = "32";
        public static string ProvenanceSmallFontSize = "14";
        public static string ProvenanceForeground = NoteForeground;

        //Collaboration
        public static bool EnableOverlays = false;
        public static int AddOverlayFontSize = 6;
        public static int DeleteOverlayFontSize = 9;
        public static string OverlayForeground = "Transparent";
        public static string AddedColor = ((SolidColorBrush)Application.Current.Resources["Green"]).Color.ToString();
        public static string DeletedColor = ((SolidColorBrush)Application.Current.Resources["Red"]).Color.ToString();
        public static string PendingColor = ((SolidColorBrush)Application.Current.Resources["SteelBlue"]).Color.ToString();
        public static string PurgedColor = "Transparent";
        public static double MeasureFooterOpacity = .2;

		//Area SelectC:\Projects\Composer\Composer\Composer.Infrastructure\Support\Preferences.cs
        public static string SelectAreaBackground = NoteSelectorColor;
        public static double SelectAreaBorderThickness = 2;
        public static string SelectAreaBorderColor = "#99ccff";
        public static Visibility SelectAreaForegroundVisibility = Visibility.Collapsed;
        public static double SelectAreaOpacity = .3;

        public static int DefaultBlurRadius = 14;
        public static double FirstNoteOnStaffX = 221;

        public static string PasteOverlayColor = Application.Current.Resources["DarkRed"].ToString();
        public static string PasteInsertColor = Application.Current.Resources["DarkBlue"].ToString();
        public static string PasteAppendColor = Application.Current.Resources["DarkGreen"].ToString();

		//Vectors
		public static int SharpVectorId = 25;
		public static int FlatVectorId = 26;
        public static int NaturalVectorId = 27;
        public static int NullVectorId = 29;

		//Printing
		public static int PrintItemsPerPage = 3;
		public static string PrintBackground = "#FFFFFF";
		public static string PrintForeground = NoteForeground;

        //Dimensions
        public static string DefaultKey = "C";
        public static string DefaultInterval = "Major third";
        public static string MeasureBar = "Standard";
        public static string StaffBar = "Standard";
        public static string DefaultInstrument = "Piano";
        public static int DefaultTimeSignatureId = 4;
        public static int DefaultClefId = 1; //Treble
        public static int DefaultGrandStaffClefId = 0; //Bass - Used in NewCompositionPanel;

        //Palettes
        public static string PaletteButtonForeground = "#FFFFFF";
        public static string PaletteButtonBackground = ((SolidColorBrush)Application.Current.Resources["SteelBlue"]).Color.ToString();
        public static string PaletteButtonBackgroundDisabled = Preferences.DisabledColor;
        public static string PaletteButtonForegroundDisabled = "#DDDDDD";

        //Hub
        public static class Hub
        {
            public static double ScrollHeight = 295;
			public static string LyricsCaption = "Lyrics";
            public static class CompositionImage
            {
                public static string BorderColor = "#336699";
                public static double BorderWidth = 4;
                public static string Margin = "10,34,10,10";
                public static double Width = 900;
                public static double Height = 12500; 
                public static double Scale = .4;
            }
        }
    }
}