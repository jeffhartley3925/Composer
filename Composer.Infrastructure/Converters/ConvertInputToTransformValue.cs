using System;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
	public class ConvertInputToTransformValue : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			double result = 0; //default to no translation
			double scale;
			if (double.TryParse(value.ToString(), out scale))
			{
				string input = parameter.ToString();
				string[] arr = input.Split(',');
				if (arr.Length == 2)
				{
					try
					{
						string transformType = arr[1];
						string position = arr[0];
						if (scale < 1)
						{

							switch (position)
							{
								case "UpperLeft":
									switch (transformType)
									{
										case "Angle":
											result = -225;
											break;
										case "TranslateX":
											result = 6;
											break;
										case "TranslateY":
											result = 2;
											break;
									}
									break;
								case "UpperRight":
									switch (transformType)
									{
										case "Angle":
											result = 225;
											break;
										case "TranslateX":
											result = 14;
											break;
										case "TranslateY":
											result = 6;
											break;
									}
									break;
								case "LowerLeft":
									switch (transformType)
									{
										case "Angle":
											result = 45;
											break;
										case "TranslateX":
											result = 2;
											break;
										case "TranslateY":
											result = 10;
											break;
									}
									break;
								case "LowerRight":
									switch (transformType)
									{
										case "Angle":
											result = -45;
											break;
										case "TranslateX":
											result = 10;
											break;
										case "TranslateY":
											result = 14;
											break;
									}
									break;
							}
						}
						else
						{
							switch (position)
							{
								case "UpperLeft":
									switch (transformType)
									{
										case "Angle":
											result = -45;
											break;
										case "TranslateX":
											result = 3;
											break;
										case "TranslateY":
											result = 7;
											break;
									}
									break;
								case "UpperRight":
									switch (transformType)
									{
										case "Angle":
											result = 45;
											break;
										case "TranslateX":
											result = 9;
											break;
										case "TranslateY":
											result = 3;
											break;
									}
									break;
								case "LowerLeft":
									switch (transformType)
									{
										case "Angle":
											result = 225;
											break;
										case "TranslateX":
											result = 7;
											break;
										case "TranslateY":
											result = 13;
											break;
									}
									break;
								case "LowerRight":
									switch (transformType)
									{
										case "Angle":
											result = 135;
											break;
										case "TranslateX":
											result = 13;
											break;
										case "TranslateY":
											result = 9;
											break;
									}
									break;
							}
						}
					}
					catch
					{
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
