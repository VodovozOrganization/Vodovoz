using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	public enum DriverTerminalRelation
	{
		[Display(Name = "Водители с терминалами")]
		WithTerminal,
		[Display(Name = "Водители без терминалов")]
		WithoutTerminal
	}
}
