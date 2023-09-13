using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	public enum NomenclatureOnlineMarker
	{
		[Display(Name = "Товар недели")]
		ProductOfWeek,
		[Display(Name = "Скидка")]
		Sale
	}
}