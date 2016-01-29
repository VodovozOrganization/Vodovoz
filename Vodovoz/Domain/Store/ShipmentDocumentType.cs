using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Store
{
	public enum ShipmentDocumentType
	{
		[Display(Name = "Заказ")]
		Order,
		[Display(Name = "Маршрутный лист")]
		RouteList
	}
}
