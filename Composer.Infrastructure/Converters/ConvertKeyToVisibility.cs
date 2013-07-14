using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
	public class ConvertKeyToVisibility : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
		    var result = Visibility.Collapsed;
			if (value != null)
			{
			    int keyId;
			    if (int.TryParse(value.ToString(), out keyId))
				{
					if (parameter != null)
					{
                   
						var bindingHelpers = parameter.ToString().Split(',');

						var bindingHelper = new Tuple<string, int>(bindingHelpers[0], int.Parse(bindingHelpers[1]));
						Dimensions.Key key = (from a in Dimensions.Keys.KeyList where a.Id == keyId select a).DefaultIfEmpty(null).Single();
						if (key != null && bindingHelper.Item2 <= key.BindingHelper.Item2 &&
										   bindingHelper.Item1 == key.BindingHelper.Item1)
						{
							result = Visibility.Visible;
						}
					}
				}
			}
		    return result;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
