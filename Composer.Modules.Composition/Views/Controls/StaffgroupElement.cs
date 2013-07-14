using System.Windows;
using System.Windows.Controls;

namespace Composer.Modules.Composition.Controls
{
	public class StaffgroupElement : Button
	{
		public StaffgroupElement()
		{
			Style = Application.Current.Resources["InvisibleButtonStyle"] as Style;
		}
	}
}
