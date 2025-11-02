using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public enum WorkShiftChangedBy
	{
		[Display(Name = "Оператор")]
		Operator,
		[Display(Name = "Администратор")]
		Admin
	}
}
