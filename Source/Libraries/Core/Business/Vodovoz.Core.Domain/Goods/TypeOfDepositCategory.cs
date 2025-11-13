using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Подтип категории "Залог"
	/// </summary>
	public enum TypeOfDepositCategory
	{
		[Display(Name = "Залог за бутыли")]
		BottleDeposit,
		[Display(Name = "Залог за оборудование")]
		EquipmentDeposit
	}
}
