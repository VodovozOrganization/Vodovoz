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
			return !(value is Employee employee) ? "" : employee.GetPersonNameWithInitials();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
