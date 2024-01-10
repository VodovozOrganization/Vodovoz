using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	public enum GoodsOnlineAvailability
	{
		[Display(Name = "Передавать и продавать")]
		ShowAndSale,
		[Display(Name = "Передавать и не продавать")]
		Show
	}
}
