using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Warehouses
{
	/// <summary>
	/// Тип использования склада.
	/// </summary>
	public enum WarehouseUsing
	{
		/// <summary>
		/// Отгрузка
		/// </summary>
		[Display(Name = "Отгрузка")]
		Shipment,
		/// <summary>
		/// Производство
		/// </summary>
		[Display(Name = "Производство")]
		Production
	}
}
