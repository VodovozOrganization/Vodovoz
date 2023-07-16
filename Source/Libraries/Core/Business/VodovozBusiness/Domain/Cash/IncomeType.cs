using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	public enum IncomeType
	{
		[Display (Name = "Прочий приход")]
		Common,
		[Display (Name = "Оплата покупателя")]
		Payment,
		[Display (Name = "Приход от водителя")]
		DriverReport,
		[Display (Name = "Возврат от подотчетного лица")]
		Return,
	}
}
