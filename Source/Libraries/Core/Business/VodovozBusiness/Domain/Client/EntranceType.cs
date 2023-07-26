using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum EntranceType
	{
		[Display(Name = "Парадная", ShortName = "пар.")]
		Entrance,
		[Display(Name = "Торговый центр", ShortName = "ТЦ")]
		TradeCenter,
		[Display(Name = "Торговый комплекс", ShortName = "ТК")]
		TradeComplex,
		[Display(Name = "Бизнес-центр", ShortName = "БЦ")]
		BusinessCenter,
		[Display(Name = "Школа", ShortName = "шк.")]
		School,
		[Display(Name = "Общежитие", ShortName = "общ.")]
		Hostel
	}
}
