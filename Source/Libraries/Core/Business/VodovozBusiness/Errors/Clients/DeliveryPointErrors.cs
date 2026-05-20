using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Clients
{
	public static partial class DeliveryPointErrors
	{
		/// <summary>
		/// Точка доставки не найдена
		/// </summary>
		public static Error NotFound =>
			new Error(
				typeof(DeliveryPointErrors),
				nameof(NotFound),
				"Точка доставки не найдена");
		
		/// <summary>
		/// Точка доставки не найдена
		/// </summary>
		public static Error CouldNotCalculateDeliveryBecauseDistrictNotFound(int deliveryPointId) =>
			new Error(
				typeof(DeliveryPointErrors),
				nameof(CouldNotCalculateDeliveryBecauseDistrictNotFound),
				$"Невозможно рассчитать доставку, т.к. не найден логистический район в точке доставки {deliveryPointId}");
	}
}
