namespace Vodovoz.Errors.Payments
{
	public static class PaymentsDistribution
	{
		public static Error AutomaticDistribution(string message) =>
			new Error(
				typeof(PaymentsDistribution),
				nameof(AutomaticDistribution),
				$"Не удалось завершить автоматическое распределение: {message}");
	}
}
