using System;
using System.Linq;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Common;

namespace Vodovoz.Validation
{
	public class FastDeliveryValidator : IFastDeliveryValidator
	{
		private readonly IGeneralSettings _generalSettings;

		public FastDeliveryValidator(IGeneralSettings generalSettings)
		{
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
		}
		
		public Result ValidateOrder(Order order)
		{
			if(!order.DeliveryDate.HasValue || order.DeliveryDate.Value.Date != DateTime.Now.Date)
			{
				return Result.Failure(Errors.Orders.FastDeliveryErrors.InvalidDate);
			}

			if(!Order.PaymentTypesFastDeliveryAvailableFor.Contains(order.PaymentType))
			{
				return Result.Failure(Errors.Orders.FastDeliveryErrors.CreateInvalidPaymentTypeError(order.PaymentType));
			}

			if(order.DeliveryPoint == null)
			{
				return Result.Failure(Errors.Orders.FastDeliveryErrors.DeliveryPointIsMissing);
			}

			if(order.DeliveryPoint.Longitude == null || order.DeliveryPoint.Latitude == null)
			{
				return Result.Failure(Errors.Clients.DeliveryPointErrors.FastDelivery.CoordinatesIsMissing);
			}

			var district = order.DeliveryPoint.District;

			if(district == null)
			{
				return Result.Failure(Errors.Clients.DeliveryPointErrors.FastDelivery.DistrictIsMissing);
			}

			if(district.TariffZone == null)
			{
				return Result.Failure(Errors.Logistics.DistrictErrors.FastDelivery.TariffZoneIsMissing);
			}

			if(!district.TariffZone.IsFastDeliveryAvailableAtCurrentTime)
			{
				return Result.Failure(Errors.Logistics.TariffZoneErrors.FastDelivery.CreateFastDeliveryIsUnavailableAtCurrentTimeError(district.TariffZone.FastDeliveryTimeFrom));
			}

			var total19LBottlesToDeliver = order.Total19LBottlesToDeliver;
			
			if(total19LBottlesToDeliver == 0)
			{
				return Result.Failure(Errors.Orders.FastDeliveryErrors.Water19LIsMissing);
			}
			
			var isFastDelivery19LBottlesLimitActive = _generalSettings.IsFastDelivery19LBottlesLimitActive;

			if(isFastDelivery19LBottlesLimitActive)
			{
				var fastDelivery19LBottlesLimitCount = _generalSettings.FastDelivery19LBottlesLimitCount;

				if(total19LBottlesToDeliver > fastDelivery19LBottlesLimitCount)
				{
					return Result.Failure(Errors.Orders.OrderErrors.FastDelivery19LBottlesLimitError(
						total19LBottlesToDeliver, fastDelivery19LBottlesLimitCount));
				}
			}

			return Result.Success();
		}
	}
}
