using System.ComponentModel.DataAnnotations;

namespace Mango.Core.Dto
{
	public enum MangoCallDctType
	{
		[Display(Name = "Не относится к коллтрекингу")]
		None = 0,

		[Display(Name = "Динамический номер")]
		Dynamic = 1,

		[Display(Name = "Статический номер")]
		Static = 2
	}
}
