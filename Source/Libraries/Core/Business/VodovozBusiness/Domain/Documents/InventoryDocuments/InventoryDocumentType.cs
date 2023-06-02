using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents.InventoryDocuments
{
	public enum InventoryDocumentType
	{
		[Display(Name = "Для склада")]
		WarehouseInventory,
		[Display(Name = "Для сотрудника")]
		EmployeeInventory,
		[Display(Name = "Для автомобиля")]
		CarInventory
	}
}
