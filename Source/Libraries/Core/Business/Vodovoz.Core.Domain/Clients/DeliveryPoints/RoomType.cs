using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients.DeliveryPoints
{
	/// <summary>
	/// Тип помещения
	/// </summary>
	public enum RoomType
	{
		/// <summary>
		/// Квартира
		/// </summary>
		[Display(Name = "Квартира", ShortName = "кв.")]
		Apartment,
		/// <summary>
		/// Офис
		/// </summary>
		[Display(Name = "Офис", ShortName = "оф.")]
		Office,
		/// <summary>
		/// Склад
		/// </summary>
		[Display(Name = "Склад", ShortName = "склад")]
		Store,
		/// <summary>
		/// Помещение
		/// </summary>
		[Display(Name = "Помещение", ShortName = "пом.")]
		Room,
		/// <summary>
		/// Комната
		/// </summary>
		[Display(Name = "Комната", ShortName = "ком.")]
		Chamber,
		/// <summary>
		/// Секция
		/// </summary>
		[Display(Name = "Секция", ShortName = "сек.")]
		Section
	}
}
