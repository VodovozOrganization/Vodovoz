using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Sales
{
	public static class SalesErrors
	{
		public static Error CounterpartyIsEmpty = new Error(
			typeof(SalesErrors),
			nameof(CounterpartyIsEmpty),
			"Для добавления позиции на продажу должен быть выбран клиент");
	}
}
