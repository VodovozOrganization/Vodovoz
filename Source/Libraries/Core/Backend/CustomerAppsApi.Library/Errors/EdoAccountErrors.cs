using Vodovoz.Core.Domain.Results;

namespace CustomerAppsApi.Library.Errors
{
	public class EdoAccountErrors
	{
		/// <summary>
		/// Клиент имеет такой ЭДО аккаунт
		/// </summary>
		/// <returns></returns>
		public static Error EdoAccountExists()
			=> new Error(
				"400",
				"У клиента уже есть такой ЭДО аккаунт",
				typeof(EdoAccountErrors)
			);
		/// <summary>
		/// У клиента уже есть ЭДО аккаунт
		/// </summary>
		/// <returns></returns>
		public static Error CounterpartyHasEdoAccount()
			=> new Error(
				"400",
				"У клиента уже есть ЭДО аккаунт",
				typeof(EdoAccountErrors)
			);
	}
}
