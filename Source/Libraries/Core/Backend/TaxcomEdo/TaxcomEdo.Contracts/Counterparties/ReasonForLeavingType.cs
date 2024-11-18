namespace TaxcomEdo.Contracts.Counterparties
{
	/// <summary>
	/// Причины приобретения воды клиентом(вывод из оборота)
	/// </summary>
	public enum ReasonForLeavingType
	{
		/// <summary>
		/// Неизвестно
		/// </summary>
		Unknown,
		/// <summary>
		/// Для собственных нужд
		/// </summary>
		ForOwnNeeds,
		/// <summary>
		/// Перепродажа
		/// </summary>
		Resale,
		/// <summary>
		/// Иная
		/// </summary>
		Other
	}
}
