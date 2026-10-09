using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.ExternalCounterparties
{
	public static class ExternalCounterpartyErrors
	{
		/// <summary>
		/// Внешний пользователь не найден
		/// </summary>
		public static Error NotFound =>
			new Error(
				typeof(ExternalCounterpartyErrors),
				nameof(NotFound),
				"Внешний пользователь не найден");
	}
}
