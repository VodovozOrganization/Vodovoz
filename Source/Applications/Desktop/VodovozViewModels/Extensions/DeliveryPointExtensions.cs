using QS.Osrm;
using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Extensions
{
	public static class DeliveryPointExtensions
	{
		public static PointOnEarth GetPointOnEarth(this DeliveryPoint deliveryPoint)
		{
			if(deliveryPoint.Latitude is null || deliveryPoint.Longitude is null)
			{
				throw new InvalidOperationException($"Невозможно получить координаты из DeliveryPoint, координаты не заданы для Точки доставки: {deliveryPoint.Id}");
			}

			return new PointOnEarth(
				Convert.ToDouble(deliveryPoint.Latitude),
				Convert.ToDouble(deliveryPoint.Longitude));
		}
	}
}
