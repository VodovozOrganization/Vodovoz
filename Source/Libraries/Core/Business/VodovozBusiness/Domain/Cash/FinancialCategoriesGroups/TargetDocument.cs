using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash.FinancialCategoriesGroups
{
	public enum TargetDocument
	{
		[Display(Name = "Ордер")]
		Invoice,
		[Display(Name = "Ордер для самовывоза")]
		SelfDelivery,
		[Display(Name = "Ордер для документа перемещения ДС")]
		Transfer
	}
}
