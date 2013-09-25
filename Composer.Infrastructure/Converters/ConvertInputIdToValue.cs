using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Linq;
using Composer.Infrastructure.Support;
using Composer.Infrastructure.Constants;

namespace Composer.Infrastructure.Converters
{
    public class ConvertInputIdToValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = string.Empty;
            var target = ((string)parameter).Trim();
            try
            {
                int disposition;
                string uiHelper;
                switch (target)
                {
                    case "StaffDimensionPanelMargin":
                        return (Preferences.MeasureDebugInfoVisibility == Visibility.Visible) ? "0,20,0,0" : "0,2,0,0";
                    case "MeasureDebugInfoVisibility":
                        return (Preferences.MeasureDebugInfoVisibility);
                    case "DispositionVisibility":
                        var visible = (Visibility)value;
                        return (Preferences.MergeGlobally && visible == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
                    case "LyricsMargin":
                        result = string.Format("0,{0},0,0", Preferences.LyricsTopMargin);
                        break;
                    case "VerseMargin":
                        //MeasureView and PrintMeasureView VerseMargin are handled differently. need to normalize.
                        result = "8,-5,0,0";
                        break;
                    case "VerseNumbersMargin":
                        result = string.Format("{0},{1},0,0", Preferences.VerseNumbersLeftMargin, Preferences.LyricsTopMargin);
                        break;
                    case "MeasureVerseTrestle":
                        result = Math.Abs(Defaults.VerseHeight) * EditorState.VerseCount;
                        break;
                    case "MeasureTopDeadSpace":
                        result = Preferences.MeasureTopDeadSpace;
                        break;
                    case "MeasureBottomDeadSpace":
                        result = Preferences.MeasureBottomDeadSpace;
                        break;
                    case "VerseLineHeight":
                        result = Defaults.VerseHeight;
                        break;
                    case "VerseFontFamily":
                        result = Preferences.VerseFontFamily;
                        break;
                    case "VerseFontSize":
                        result = Preferences.VerseFontSize;
                        break;
                    case "VerseForeground":
                        result = Preferences.VerseForeground;
                        break;
                    case "VerseFontWeight":
                        result = Preferences.VerseFontWeight;
                        break;
                    case "SelectAreaBackground":
                        result = Preferences.SelectAreaBackground;
                        break;
                    case "SelectAreaBorderThickness":
                        result = Preferences.SelectAreaBorderThickness;
                        break;
                    case "SelectAreaBorderColor":
                        result = Preferences.SelectAreaBorderColor;
                        break;
                    case "SelectAreaForegroundVisibility":
                        result = Preferences.SelectAreaForegroundVisibility;
                        break;
                    case "SelectAreaOpacity":
                        result = Preferences.SelectAreaOpacity;
                        break;
                    case "NoteSelectorColor":
                        result = Preferences.NoteSelectorColor;
                        break;
                    case "AcceptButtonPathVector":
                        result = (
                            from a in Vectors.VectorList
                            where a.Class == "Disposition" && a.Name == _Enum.Disposition.Accept.ToString()
                            select a.Path).First().ToString(CultureInfo.InvariantCulture);
                        break;
                    case "RejectButtonPathVector":
                        result = (
                           from a in Vectors.VectorList
                           where a.Class == "Disposition" && a.Name == _Enum.Disposition.Reject.ToString()
                           select a.Path).First().ToString(CultureInfo.InvariantCulture);
                        break;
                    case "LyricsPanelVerseForeground":
                        uiHelper = (string)value;
                        result = (string.IsNullOrEmpty(uiHelper)) ? Preferences.LyricsPanelVerseForeground : Preferences.LyricsPanelSelectedVerseForeground;
                        break;
                    case "LyricsPanelVerseBackground":
                        uiHelper = (string)value;
                        result = (string.IsNullOrEmpty(uiHelper)) ? Preferences.LyricsPanelVerseBackground : Preferences.LyricsPanelSelectedVerseBackground;
                        break;
                    case "LyricsPanelVerseDisposition":
                        disposition = int.Parse(value.ToString());
                        result = (disposition == 1) ? "Remove" : "Include";
                        break;
                    case "LyricsPanelVerseOpacity":
                        disposition = int.Parse(value.ToString());
                        result = (disposition == 1) ? 1 : .3;
                        break;
                    case "VerseDisposition":
                        disposition = int.Parse(value.ToString());
                        result = (disposition == 1) ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case "LyricPanelVerseEditorForeground":
                        var enabled = (bool)value;
                        result = (enabled) ? Preferences.LyricsPanelEditorEnabledForeground : Preferences.LyricsPanelEditorDisabledForeground;
                        break;
                    case "LyricsPanelAbbreviatedVerse":
                        string verseText = value.ToString();
                        result = (verseText.Length <= 63) ? verseText : verseText.Substring(0, 62) + "...";
                        result = verseText;
                        break;
                    case "SharpVectorId":
                        result = (from a in Dimensions.Accidentals.AccidentalList where a.Id == Preferences.SharpVectorId select a.Vector).First();
                        break;
                    case "FlatVectorId":
                        result = (from a in Dimensions.Accidentals.AccidentalList where a.Id == Preferences.FlatVectorId select a.Vector).First();
                        break;
                    case "SimpleStaffConfiguration":
                        result = (Preferences.DefaultStaffConfiguration == _Enum.StaffConfiguration.Simple || Preferences.DefaultStaffConfiguration == _Enum.StaffConfiguration.None);
                        break;
                    case "GrandStaffConfiguration":
                        result = (Preferences.DefaultStaffConfiguration == _Enum.StaffConfiguration.Grand);
                        break;
                    case "MultiInstrumentStaffConfiguration":
                        result = (Preferences.DefaultStaffConfiguration == _Enum.StaffConfiguration.MultiInstrument);
                        break;
                    case "StaffgroupBracketVisibility":
                        return (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand) ? Visibility.Visible : Visibility.Collapsed;
                    case "StaffgroupBracketMargin":
                        result = Finetune.Staffgroup.staffgroupBracketMargin;
                        break;
                    case "BarBackground":
                        result = Dimensions.Bars.BarStaffLinesPathComplement;
                        break;
                    case "AddStaffHyperlinkMargin":
                        result = string.Format("25,0,0,0");
                        break;
                    case "ManageLyricsHyperlinkMargin":
                        result = string.Format("25,0,0,0");
                        break;
                    case "TransposeHyperlinksMargin":
                        result = string.Format("25,0,0,0");
                        break;
                    case "CollaborateHyperlinkMargin":
                        result = string.Format("25,0,0,0");
                        break;
                    case "ProvenanceHyperlinkMargin":
                        result = string.Format("25,0,0,0");
                        break;
                    case "PrintHyperlinkMargin":
                        result = string.Format("25,0,0,0");
                        break;
                    case "SaveHyperlinkMargin":
                        result = string.Format("10,0,0,0");
                        break;
                    case "HubHyperlinkMargin":
                        result = string.Format("25,0,0,0");
                        break;
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, string.Format("{0} {1}", value, parameter));
            }
            return result ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}