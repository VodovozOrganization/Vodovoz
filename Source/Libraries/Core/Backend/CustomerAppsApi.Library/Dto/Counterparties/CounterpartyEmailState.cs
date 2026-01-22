namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Состояние почты клиента
	/// </summary>
	public enum CounterpartyEmailState
	{
		/// <summary>
		/// У клиента нет указанной почты и нет активной учетной записи
		/// </summary>
		EmailNotExistsAndNotExistsActiveEmails,
		/// <summary>
		/// У клиента есть указанная почта и нет активной учетной записи
		/// </summary>
		EmailExistsAndNotExistsActiveEmails,
		/// <summary>
		/// У клиента уже есть другая активная учетная запись
		/// </summary>
		HasAnotherActiveEmail,
		/// <summary>
		/// У клиента уже есть почта и она активна
		/// </summary>
		HasEmailAndActive
	}
}
