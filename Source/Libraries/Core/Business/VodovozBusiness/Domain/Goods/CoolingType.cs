using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum CoolingType
	{
		[Display(Name = "Компрессорное")]
		Compressor,
		[Display(Name = "Электронное")]
		Electronic
	}
}
