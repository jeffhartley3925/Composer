using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Composer.Silverlight.UI
{
    public static class QueryString
    {
        public static string compositionId = string.Empty;
        public static string measureId = string.Empty;
        public static string collaborationIndex = "0"; //TODO: refactor to 'Index'

        public static Guid CompositionId = Guid.Empty;
        public static Guid MeasureId = Guid.Empty;
        public static string UserId;

        public static void Initialize()
        {
            if (compositionId != string.Empty)
            {
                if (Guid.TryParse(compositionId, out CompositionId))
                {
                }
                else
                {
                    CompositionId = Guid.Empty;
                }
            }

            if (measureId != string.Empty)
            {
                if (Guid.TryParse(measureId, out MeasureId))
                {
                }
                else
                {
                    MeasureId = Guid.Empty;
                }
            }
        }
    }
}
