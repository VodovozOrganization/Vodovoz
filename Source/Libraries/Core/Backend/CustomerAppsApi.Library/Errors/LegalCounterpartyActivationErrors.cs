using Vodovoz.Core.Domain.Results;

namespace CustomerAppsApi.Library.Errors
{
	public static class LegalCounterpartyActivationErrors
	{
		/// <summary>
		/// Онлайн активация юр лица не найдена
		/// </summary>
		/// <returns></returns>
		public static Error ActivationNotExists()
			=> new Error(
				"404",
				"Нет данных по онлайн активации указанного юр лица",
				typeof(CounterpartyErrors)
			);
		
		/// <summary>
		/// Онлайн активация юр лица не в том состоянии
		/// </summary>
		/// <returns></returns>
		public static Error ActivationInWrongState()
			=> new Error(
				"422",
				"Нельзя обновить данные онлайн активации юр лица в текущем состоянии",
				typeof(CounterpartyErrors)
			);
		
		/// <summary>
		/// Найдено несколько одинаковых онлайн аккаунтов юр лиц
		/// </summary>
		/// <returns></returns>
		public static Error MoreThanOneExternalLegalCounterpartyAccounts()
			=> new Error(
				"500",
				"Найдено несколько действующих аккаунтов, обратитесь в техподдержку",
				typeof(CounterpartyErrors)
			);
	}
}
