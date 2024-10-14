using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OnlineOrderErrorState
	{
		[Display(Name = "Неверные параметры скидки или она не применима")]
		WrongDiscountParametersOrIsNotApplicable
	}
}
