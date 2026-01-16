using Vodovoz.Core.Domain.Results;

namespace CustomerAppsApi.Library.Errors
{
	public class PhoneErrors
	{
		/// <summary>
		/// Телефон уже существует
		/// </summary>
		/// <returns></returns>
		public static Error PhoneExists()
			=> new Error(
				"400",
				"Телефон уже привязан к данному клиенту",
				typeof(PhoneErrors)
			);
	}
}
