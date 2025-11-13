using ClosedXML.Report.Utils;
using Gamma.Binding;
using System;
using System.Globalization;

namespace Vodovoz.Infrastructure.Converters
{
	public class TextToBoldTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return $"<b>{value}</b>";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var str = value as string;
			if(str.IsNullOrWhiteSpace())
			{
				return string.Empty;
			}

			if(str.StartsWith("<b>") && str.EndsWith("</b>"))
			{
				return str.Substring(3, str.Length - 7);
			}
			else
			{
				return str;
			}
		}
	}
}
