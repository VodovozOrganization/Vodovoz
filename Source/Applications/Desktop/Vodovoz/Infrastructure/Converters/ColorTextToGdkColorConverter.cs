using System;
using System.Globalization;
using Gamma.Binding;
namespace Vodovoz.Infrastructure.Converters
{
	public class ColorTextToGdkColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Gdk.Color color = new Gdk.Color();
			if(string.IsNullOrEmpty((string)value)) {
				return Gdk.Color.Zero;
			}
			if(!Gdk.Color.Parse((string)value, ref color)) {
				throw new InvalidCastException("Ошибка в распознавании цвета тега");
			}
			return color;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Gdk.Color color = (Gdk.Color)value;
			return string.Format("#{0:x4}{1:x4}{2:x4}", color.Red, color.Green, color.Blue);
		}
	}
}
