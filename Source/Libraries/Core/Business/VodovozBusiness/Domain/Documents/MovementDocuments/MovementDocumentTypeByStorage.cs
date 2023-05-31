using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents.MovementDocuments
{
	public enum MovementDocumentTypeByStorage
	{
		[Display(Name = "На склад")]
		ToWarehouse,
		[Display(Name = "На сотрудника")]
		ToEmployee,
		[Display(Name = "На автомобиль")]
		ToCar
	}
}
