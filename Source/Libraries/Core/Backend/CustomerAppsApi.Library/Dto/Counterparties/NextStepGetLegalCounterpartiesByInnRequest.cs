namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Следующий шаг после получения юр лиц по ИНН
	/// </summary>
	public enum NextStepGetLegalCounterpartiesByInnRequest
	{
		/// <summary>
		/// 
		/// </summary>
		ConfirmAccess,
		/// <summary>
		/// 
		/// </summary>
		CreateConnection,
		/// <summary>
		/// 
		/// </summary>
		UserHasAnotherActiveEmail
	}
}
