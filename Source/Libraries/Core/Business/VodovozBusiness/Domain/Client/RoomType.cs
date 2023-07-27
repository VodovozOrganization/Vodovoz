using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum RoomType
	{
		[Display(Name = "Квартира", ShortName = "кв.")]
		Apartment,
		[Display(Name = "Офис", ShortName = "оф.")]
		Office,
		[Display(Name = "Склад", ShortName = "склад")]
		Store,
		[Display(Name = "Помещение", ShortName = "пом.")]
		Room,
		[Display(Name = "Комната", ShortName = "ком.")]
		Chamber,
		[Display(Name = "Секция", ShortName = "сек.")]
		Section
	}
}
