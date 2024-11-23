using QS.Utilities.Numeric;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class PhoneExtensions
	{
		public static string NormalizePhone(this string phone)
			=> new PhoneFormatter(PhoneFormat.DigitsTen).FormatString(phone);
	}
}
