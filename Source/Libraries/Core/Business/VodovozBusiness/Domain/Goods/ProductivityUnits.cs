using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum ProductivityUnits
	{
		[Display(ShortName = "л/ч", Name = "литр/час")]
		LiterPerHour
	}
}
