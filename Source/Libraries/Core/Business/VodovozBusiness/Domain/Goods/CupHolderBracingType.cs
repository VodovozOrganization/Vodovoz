using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum CupHolderBracingType
	{
		[Display(Name = "Магнит")]
		Magnet,
		[Display(Name = "Стаканодержатель")]
		CupHolder
	}
}
