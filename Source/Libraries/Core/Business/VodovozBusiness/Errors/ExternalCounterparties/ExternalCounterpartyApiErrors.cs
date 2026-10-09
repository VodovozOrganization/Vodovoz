using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.ExternalCounterparties
{
	public static class ExternalCounterpartyApiErrors
	{
		/// <summary>
		/// Внешний пользователь не найден
		/// </summary>
		public static Error NotFound => new Error("400", "Внешний пользователь не найден");
	}
}
