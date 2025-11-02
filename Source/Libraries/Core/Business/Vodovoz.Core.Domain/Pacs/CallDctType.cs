using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	public enum CallDctType
	{
		[Display(Name = "Не относится к коллтрекингу")]
		None,

		[Display(Name = "Динамический номер")]
		Dynamic,

		[Display(Name = "Статический номер")]
		Static
	}
}
