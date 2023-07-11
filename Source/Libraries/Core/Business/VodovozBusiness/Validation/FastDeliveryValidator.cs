using System;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors;

namespace Vodovoz.Validation
{
	public class FastDeliveryValidator : IFastDeliveryValidator
	{
		public Result ValidateOrder(Order order)
		{
			if(!order.DeliveryDate.HasValue || order.DeliveryDate.Value.Date != DateTime.Now.Date)
			{
				return Result.Failure(Errors.Orders.Order.FastDelivery.InvalidDate);
			}

			if(!Order.PaymentTypesFastDeliveryAvailableFor.Contains(order.PaymentType))
			{
				return Result.Failure(Errors.Orders.Order.FastDelivery.CreateInvalidPaymentTypeError(order.PaymentType));
			}

			if(order.DeliveryPoint == null)
			{
				return Result.Failure(Errors.Orders.Order.FastDelivery.DeliveryPointIsMissing);
			}

			if(order.DeliveryPoint.Longitude == null || order.DeliveryPoint.Latitude == null)
			{
				return Result.Failure(Errors.Clients.DeliveryPoint.FastDelivery.CoordinatesIsMissing);
			}

			var district = order.DeliveryPoint.District;

			if(district == null)
			{
				return Result.Failure(Errors.Clients.DeliveryPoint.FastDelivery.DistrictIsMissing);
			}

			if(district.TariffZone == null)
			{
				return Result.Failure(Errors.Logistics.District.FastDelivery.TariffZoneIsMissing);
			}

			if(!district.TariffZone.IsFastDeliveryAvailableAtCurrentTime)
			{
				return Result.Failure(Errors.Logistics.TariffZone.FastDelivery.CreateFastDeliveryIsUnavailableAtCurrentTimeError(district.TariffZone.FastDeliveryTimeFrom));
			}

			if(order.Total19LBottlesToDeliver == 0)
			{
				return Result.Failure(Errors.Orders.Order.FastDelivery.Water19LIsMissing);
			}

			return Result.Success();
		}
	}
}
