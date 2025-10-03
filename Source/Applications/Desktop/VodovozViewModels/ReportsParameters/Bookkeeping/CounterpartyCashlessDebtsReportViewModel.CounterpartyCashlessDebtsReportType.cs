namespace Vodovoz.ViewModels.ReportsParameters.Bookkeeping
{
	public partial class CounterpartyCashlessDebtsReportViewModel
	{
		/// <summary>
		/// Тип отчёта "Долги по безналу"
		/// </summary>
		private enum CounterpartyCashlessDebtsReportType
		{
			/// <summary>
			/// Баланс компании
			/// </summary>
			DebtBalance,
			/// <summary>
			/// Неоплаченные заказы
			/// </summary>
			NotPaidOrders,
			/// <summary>
			/// Детализация по клиенту
			/// </summary>
			DebtDetails
		}
	}
}
