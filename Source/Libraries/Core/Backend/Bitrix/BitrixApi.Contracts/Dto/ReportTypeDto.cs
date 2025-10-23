namespace BitrixApi.Contracts.Dto
{
	/// <summary>
	/// Тип отчета
	/// </summary>
	public enum ReportTypeDto
	{
		/// <summary>
		/// Отчет "Акт-сверки"
		/// </summary>
		ReconciliationStatement,
		/// <summary>
		/// Отчёт "Счета по неоплаченным заказам"
		/// </summary>
		UnpaidOrdersAccount,
		/// <summary>
		/// Отчет "Общий счет"
		/// </summary>
		TotalAccount
	}
}
