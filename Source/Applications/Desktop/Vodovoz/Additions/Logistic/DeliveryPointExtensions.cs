using GMap.NET;
using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.Additions.Logistic
{
	public static class DeliveryPointExtensions
	{
		public static PointLatLng GetPointLatLng(this DeliveryPoint deliveryPoint)
		{
			if(deliveryPoint.Latitude is null || deliveryPoint.Longitude is null)
			{
				throw new InvalidOperationException($"Невозможно получить координаты из DeliveryPoint, координаты не заданы для Точки доставки: {deliveryPoint.Id}");
			}

			return new PointLatLng(
				Convert.ToDouble(deliveryPoint.Latitude),
				Convert.ToDouble(deliveryPoint.Longitude));
		}
	}
}
