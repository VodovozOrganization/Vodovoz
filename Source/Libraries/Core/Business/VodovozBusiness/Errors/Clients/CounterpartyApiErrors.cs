using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Clients
{
	public static class CounterpartyApiErrors
	{
		/// <summary>
		/// Контрагент не найден
		/// </summary>
		public static Error NotFound => new Error("400", "Контрагент не найден");
	}
}
