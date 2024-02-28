using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public enum BreakChangedBy
	{
		[Display(Name = "Оператор")]
		Operator,
		[Display(Name = "Администратор")]
		Admin
	}
}
