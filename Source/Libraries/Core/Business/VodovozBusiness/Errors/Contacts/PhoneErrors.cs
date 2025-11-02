using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Contacts
{
	public static class PhoneErrors
	{
		/// <summary>
		/// Телефон не найден
		/// </summary>
		public static Error NotFound =>
			new Error(
				typeof(PhoneErrors),
				nameof(NotFound),
				"Телефон не найден");
	}
}
