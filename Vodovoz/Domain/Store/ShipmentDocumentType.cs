using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;

namespace Vodovoz.Domain.Store
{
	public enum ShipmentDocumentType
	{
		[ItemTitleAttribute ("Заказ")]
		[Display(Name = "Заказ")]
		Order,
		[ItemTitleAttribute ("Маршрутный лист")]
		[Display(Name = "Маршрутный лист")]
		RouteList
	}
}
