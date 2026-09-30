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
		
		/// <summary>
		/// Установка фиксы для данной позиции не допускается
		/// </summary>
		public static Error FixedPriceNotAllowed =>
			new Error(
				typeof(FixedPriceErrors),
				nameof(FixedPriceNotAllowed),
				"Установка фиксы для данной позиции не допускается");
		
		/// <summary>
		/// Установка фиксы для промонаборов или пакетов аренды не допускается
		/// </summary>
		public static Error FixedPriceNotAppliedToPromoSetsAndRentPackages =>
			new Error(
				typeof(FixedPriceErrors),
				nameof(FixedPriceNotAllowed),
				"Установка фиксы для промонаборов или пакетов аренды не допускается");
	}
}
