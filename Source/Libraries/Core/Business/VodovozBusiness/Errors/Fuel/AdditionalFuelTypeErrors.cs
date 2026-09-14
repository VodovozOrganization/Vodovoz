using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Errors.Fuel
{
	public static class AdditionalFuelTypeErrors
	{
		/// <summary>
		/// Дополнительный вид топлива уже является основным
		/// </summary>
		public static Error AlreadyMainFuelType => new Error(
			typeof(AdditionalFuelTypeErrors),
			nameof(AlreadyMainFuelType),
			"Дополнительный вид топлива уже является основным");

		/// <summary>
		/// Дополнительный вид топлива уже был добавлен ранее
		/// </summary>
		public static Error AdditionalFuelTypeAlreadyAdded => new Error(
			typeof(AdditionalFuelTypeErrors),
			nameof(AdditionalFuelTypeAlreadyAdded),
			"Дополнительный вид топлива уже был добавлен ранее");

		/// <summary>
		/// Дополнительный вид топлива не найден
		/// </summary>
		public static Error AdditionalFuelTypeNotFound => new Error(
			typeof(AdditionalFuelTypeErrors),
			nameof(AdditionalFuelTypeNotFound),
			"Дополнительный вид топлива не найден");
	}
}
