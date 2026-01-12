using Vodovoz.Core.Domain.Results;

namespace CustomerAppsApi.Library.Errors
{
	public static class CounterpartyErrors
	{
		/// <summary>
		/// Не найден клиент с таким идентификатором
		/// </summary>
		/// <returns></returns>
		public static Error CounterpartyNotExists()
			=> new Error(
				"404",
				"Не найден контрагент с таким идентификатором",
				typeof(CounterpartyErrors)
			);
		
		/// <summary>
		/// Не найден пользователь с таким идентификатором
		/// </summary>
		/// <returns></returns>
		public static Error ExternalCounterpartyNotExists()
			=> new Error(
				"404",
				"Не найден зарегистрированный пользователь",
				typeof(CounterpartyErrors)
			);
		
		/// <summary>
		/// Найдено больше одного действующих клиентов с таким ИНН
		/// </summary>
		/// <returns></returns>
		public static Error MoreThanOneCounterpartyWithInn()
			=> new Error(
				"500",
				"Найдено несколько действующих контрагентов с таким ИНН, обратитесь в техподдержку",
				typeof(CounterpartyErrors)
			);
	}
}
