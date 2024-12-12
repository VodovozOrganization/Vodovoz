using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Operations
{
	public enum DepositType
	{
		[Display(Name = "Отсутствует")] None,
		[Display(Name = "Тара")] Bottles,
		[Display(Name = "Оборудование")] Equipment
	}
}
