using System;
using System.Globalization;
using Gamma.Binding;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Infrastructure.Converters
{
	public class EmployeeToLastNameWithInitialsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null) {
				return "";
			}
			if(!(value is Employee employee))
				throw new InvalidOperationException(
					string.Format(
						"Ожидался тип \"{0}\", но получен \"{1}\"",
						typeof(Employee),
						value?.GetType()
					)
				);

			return employee.GetPersonNameWithInitials();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
