namespace VodovozBusiness.Domain.Payments
{
	/// <summary>
	/// Номера колонок выписки, которые можно выгрузить в онлайн платеж
	/// </summary>
	public interface IOnlinePaymentRegisterColumns
	{
		/// <summary>
		/// Колонка с суммой
		/// </summary>
		int PaymentSumColumn { get; }

		/// <summary>
		/// Колонка с датой и временем
		/// </summary>
		int DateAndTimeColumn { get; }

		/// <summary>
		/// Колонка с номером
		/// </summary>
		int PaymentNumberColumn { get; }

		/// <summary>
		/// Колонка с email
		/// </summary>
		int? EmailColumn { get; }
	}
}
