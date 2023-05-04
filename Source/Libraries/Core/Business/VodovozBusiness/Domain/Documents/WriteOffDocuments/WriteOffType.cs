using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents.WriteOffDocuments
{
	public enum WriteOffType
	{
		[Display(Name = "Со склада")]
		Warehouse,
		[Display(Name = "От клиента")]
		Counterparty,
		[Display(Name = "С сотрудника")]
		Employee,
		[Display(Name = "С автомобиля")]
		Car
	}
}
