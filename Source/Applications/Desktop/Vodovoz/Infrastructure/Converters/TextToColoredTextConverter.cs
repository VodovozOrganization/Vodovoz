using Gamma.Binding;
using System;
using System.Globalization;

namespace Vodovoz.Infrastructure.Converters
{
	public class TextToColoredTextConverter : IValueConverter
	{
		private readonly Func<string> _colorFactory;

		public TextToColoredTextConverter(Func<string> colorFactory)
		{
			_colorFactory = colorFactory ?? throw new ArgumentNullException(nameof(colorFactory));
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var color = _colorFactory.Invoke();
			var outputString = string.Format("<span foreground=\"{0}\">{1}</span>", color, value);
			return outputString;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("Не поддерживается обратное преобразование текста с цветовой разметкой в исходный текст");
		}
	}
}
