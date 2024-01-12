namespace Vodovoz.Errors.Payments
{
	public static class PaymentsDistribution
	{
		public static Error AutomaticDistribution(string message) =>
			new Error(
				typeof(PaymentsDistribution),
				nameof(AutomaticDistribution),
				$"Не удалось завершить автоматическое распределение: {message}");

		public static Error NoOrdersToDistribute(int counterpartyId) =>
			new Error(
				typeof(PaymentsDistribution),
				nameof(NoOrdersToDistribute),
				$"Нет заказов для распределения у контрагента #{counterpartyId}");

		public static Error NoPaymentsWithPositiveBalance(int counterpartyId) =>
			new Error(
				typeof(PaymentsDistribution),
				nameof(NoPaymentsWithPositiveBalance),
				$"Нет платежей с положительным балансом у контрагента #{counterpartyId}");
	}
}
