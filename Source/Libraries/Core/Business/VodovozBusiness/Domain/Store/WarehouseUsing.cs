using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Store
{
	public enum WarehouseUsing
	{
		[Display(Name = "Отгрузка")]
		Shipment,
		[Display(Name = "Производство")]
		Production
	}
}
