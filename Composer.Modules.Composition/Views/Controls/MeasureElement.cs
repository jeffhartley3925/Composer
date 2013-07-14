using System.Windows;
using System.Windows.Controls;

namespace Composer.Modules.Composition.Controls
{
	public class MeasureElement : Button
	{
		public MeasureElement()
		{
			Style = Application.Current.Resources["InvisibleButtonStyle"] as Style;
		}
	}
}
