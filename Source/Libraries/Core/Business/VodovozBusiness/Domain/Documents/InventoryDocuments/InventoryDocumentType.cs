using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents.InventoryDocuments
{
	/// <summary>
	/// Тип инвентаризации
	/// </summary>
	public enum InventoryDocumentType
	{
		/// <summary>
		/// По складу
		/// </summary>
		[Display(Name = "Для склада")]
		WarehouseInventory,
		/// <summary>
		/// По сотруднику
		/// </summary>
		[Display(Name = "Для сотрудника")]
		EmployeeInventory,
		/// <summary>
		/// По автомобилю
		/// </summary>
		[Display(Name = "Для автомобиля")]
		CarInventory
	}
}
