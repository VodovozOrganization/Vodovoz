using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Contacts
{
	public static class Phone
	{
		/// <summary>
		/// Телефон не найден
		/// </summary>
		public static Error NotFound =>
			new Error(
				typeof(Phone),
				nameof(NotFound),
				"Телефон не найден");
	}
}
