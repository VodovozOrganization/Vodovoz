using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters
{
	/// <summary>
	/// Доступность товара в интернет магазине
	/// </summary>
	public enum GoodsOnlineAvailability
	{
		[Display(Name = "Передавать и продавать")]
		ShowAndSale,
		[Display(Name = "Передавать и не продавать")]
		Show
	}
}
