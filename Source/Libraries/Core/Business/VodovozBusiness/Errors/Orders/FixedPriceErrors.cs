using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	public static partial class FixedPriceErrors
	{
		public static Error NotFound =>
			new Error(
				typeof(FixedPriceErrors),
				nameof(NotFound),
				"Фикса не найдена");
	}
}
