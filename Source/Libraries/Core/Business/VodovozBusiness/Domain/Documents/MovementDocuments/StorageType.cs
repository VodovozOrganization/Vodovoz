using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents.MovementDocuments
{
	public enum StorageType
	{
		[Display(Name = "Cклад")]
		Warehouse,
		[Display(Name = "Cотрудник")]
		Employee,
		[Display(Name = "Aвтомобиль")]
		Car
	}
}
