using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Payments
{
	public static class PaymentsDistributionErrors
	{
		public static Error AutomaticDistribution(string message) =>
			new Error(
				typeof(PaymentsDistributionErrors),
				nameof(AutomaticDistribution),
				$"Не удалось завершить автоматическое распределение: {message}");

		public static Error NoOrdersToDistribute(int counterpartyId) =>
			new Error(
				typeof(PaymentsDistributionErrors),
				nameof(NoOrdersToDistribute),
				$"Нет заказов для распределения у контрагента #{counterpartyId}");

		public static Error NoPaymentsWithPositiveBalance(int counterpartyId) =>
			new Error(
				typeof(PaymentsDistributionErrors),
				nameof(NoPaymentsWithPositiveBalance),
				$"Нет платежей с положительным балансом у контрагента #{counterpartyId}");
	}
}
