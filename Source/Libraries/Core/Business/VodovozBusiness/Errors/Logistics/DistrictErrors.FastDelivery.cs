using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Logistics
{
	public static partial class DistrictErrors
	{
		public static class FastDelivery
		{
			public static Error TariffZoneIsMissing =>
				new Error(
					typeof(FastDelivery),
					nameof(TariffZoneIsMissing),
					"Для района точки доставки не указана тарифная зона");
		}
	}
}
