using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients.DeliveryPoints
{
	/// <summary>
	/// Тип входа
	/// </summary>
	public enum EntranceType
	{
		/// <summary>
		/// Парадная
		/// </summary>
		[Display(Name = "Парадная", ShortName = "пар.")]
		Entrance,
		/// <summary>
		/// Торговый центр
		/// </summary>
		[Display(Name = "Торговый центр", ShortName = "ТЦ")]
		TradeCenter,
		/// <summary>
		/// Торговый комплекс
		/// </summary>
		[Display(Name = "Торговый комплекс", ShortName = "ТК")]
		TradeComplex,
		/// <summary>
		/// Бизнес-центр
		/// </summary>
		[Display(Name = "Бизнес-центр", ShortName = "БЦ")]
		BusinessCenter,
		/// <summary>
		/// Школа
		/// </summary>
		[Display(Name = "Школа", ShortName = "шк.")]
		School,
		/// <summary>
		/// Общежитие
		/// </summary>
		[Display(Name = "Общежитие", ShortName = "общ.")]
		Hostel
	}
}
