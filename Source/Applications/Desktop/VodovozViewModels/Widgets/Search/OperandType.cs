using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Widgets.Search
{
	public enum OperandType
	{
		Disabled,
		[Display(Name = "И")]
		And,
		[Display(Name = "Или")]
		Or
	}
}
