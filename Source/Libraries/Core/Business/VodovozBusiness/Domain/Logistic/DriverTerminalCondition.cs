using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	public enum DriverTerminalCondition
	{
		[Display(Name = "Исправен")]
		Workable,
		[Display(Name = "Неисправен")]
		Broken
	}
}
