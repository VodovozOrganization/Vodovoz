using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Sale
{
	public enum DistrictWaterPrice
	{
		[Display(Name = "По прайсу")]
		Standart,
		[Display(Name = "Специальная цена")]
		FixForDistrict,
		[Display(Name = "По расстоянию")]
		ByDistance,
	}
}
