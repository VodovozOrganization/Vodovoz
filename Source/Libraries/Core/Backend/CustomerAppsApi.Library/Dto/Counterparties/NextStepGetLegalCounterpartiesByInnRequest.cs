using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Следующий шаг после получения юр лиц по ИНН
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum NextStepGetLegalCounterpartiesByInnRequest
	{
		/// <summary>
		/// Почты из запроса НЕТ у юр лица и у К/А нет активной учетной записи
		/// </summary>
		ConfirmAccess,
		/// <summary>
		/// Почта из запроса ЕСТЬ у юр лица и у К/А нет активной учетной записи
		/// </summary>
		CreateConnection,
		/// <summary>
		/// У юр лица есть другая активная почта
		/// </summary>
		UserHasAnotherActiveEmail,
		/// <summary>
		/// В БД нет юр лиц с таким ИНН
		/// </summary>
		CounterpartiesNotExists
	}
}
