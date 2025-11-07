using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Clients
{
	public static partial class DeliveryPointErrors
	{
		public static class FastDelivery
		{
			public static Error CoordinatesIsMissing =>
				new Error(
					typeof(FastDelivery),
					nameof(CoordinatesIsMissing),
					"Для выбора доставки за час необходимо корректно заполнить координаты точки доставки");

			public static Error DistrictIsMissing =>
				new Error(
					typeof(FastDelivery),
					nameof(DistrictIsMissing),
					"Для точки доставки не указан район");
		}
	}
}
