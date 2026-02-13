using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	public enum OwnTypes
	{
		[Display(Name = "")] None,
		[Display(Name = "Клиент")] Client,
		[Display(Name = "Дежурный")] Duty,
		[Display(Name = "Аренда")] Rent
	}
}
