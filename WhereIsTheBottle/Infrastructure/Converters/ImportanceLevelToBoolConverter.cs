using System;
using System.Globalization;
using System.Windows.Data;
using QS.Dialog;

namespace WhereIsTheBottle.Infrastructure
{
	public class ImportanceLevelToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			if(parameter == null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}

			if(!(value is ImportanceLevel))
			{
				throw new InvalidOperationException($"The value must be of type {nameof(ImportanceLevel)}");
			}
			if(!(parameter is ImportanceLevel))
			{
				throw new InvalidOperationException($"The parameter must be of type {nameof(ImportanceLevel)}");
			}

			return (ImportanceLevel)value == (ImportanceLevel)parameter;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}