using System;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Logistics
{
	public static partial class TariffZoneErrors
	{
		public static class FastDelivery
		{
			/// <summary>
			/// Не использовать
			/// Создавать через <seealso cref="CreateFastDeliveryIsUnavailableAtCurrentTimeError"/>
			/// </summary>
			public static Error FastDeliveryIsUnavailableAtCurrentTime => throw new NotImplementedException();

			public static Error CreateFastDeliveryIsUnavailableAtCurrentTimeError(TimeSpan fastDeliveryInterval) =>
				new Error(
					typeof(FastDelivery),
					nameof(FastDeliveryIsUnavailableAtCurrentTime),
					$"По данной тарифной зоне не работает доставка за час либо закончилось время работы - попробуйте в {fastDeliveryInterval:hh\\:mm}");
		}
	}
}
