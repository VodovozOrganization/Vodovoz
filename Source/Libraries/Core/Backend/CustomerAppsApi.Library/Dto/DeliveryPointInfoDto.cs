using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerAppsApi.Library.Dto
{
	/// <summary>
	/// Информация о точке доставки из ИПЗ
	/// </summary>
	public class DeliveryPointInfoDto
	{
		/// <summary>
		/// Тип населенного пункта
		/// </summary>
		[Display(Name = "Тип населенного пункта")]
		public string LocalityType { get; set; }
		/// <summary>
		/// "Тип населенного пункта(сокр.)
		/// </summary>
		[Display(Name = "Тип населенного пункта(сокр.)")]
		public string LocalityTypeShort { get; set; }
		/// <summary>
		/// Город
		/// </summary>
		[Display(Name = "Город")]
		public string City { get; set; }
		/// <summary>
		/// Тип улицы
		/// </summary>
		[Display(Name = "Тип улицы")]
		public string StreetType { get; set; }
		/// <summary>
		/// Тип улицы(сокр.)
		/// </summary>
		[Display(Name = "Тип улицы(сокр.)")]
		public string StreetTypeShort { get; set; }
		/// <summary>
		/// Улица
		/// </summary>
		[Display(Name = "Улица")]
		public string Street { get; set; }
		/// <summary>
		/// Дом/Строение
		/// </summary>
		[Display(Name = "Дом/Строение")]
		public string Building { get; set; }
		/// <summary>
		/// Этаж
		/// </summary>
		[Display(Name = "Этаж")]
		public string Floor { get; set; }
		/// <summary>
		/// Подъезд
		/// </summary>
		[Display(Name = "Подъезд")]
		public string Entrance { get; set; }
		/// <summary>
		/// Квартира
		/// </summary>
		[Display(Name = "Квартира")]
		public string Room { get; set; }
		/// <summary>
		/// Широта
		/// </summary>
		[Display(Name = "Широта")]
		public decimal Latitude { get; set; }
		/// <summary>
		/// Долгота
		/// </summary>
		[Display(Name = "Долгота")]
		public decimal Longitude { get; set; }
		/// <summary>
		/// Тип объекта
		/// </summary>
		[Display(Name = "Тип объекта")]
		public int DeliveryPointCategoryId { get; set; }
		/// <summary>
		/// Комментарий пользователя
		/// </summary>
		public string OnlineComment { get; set; }
		/// <summary>
		/// Домофон
		/// </summary>
		[Display(Name = "Домофон")]
		public string Intercom { get; set; }
		/// <summary>
		/// Код города из ФИАСа
		/// </summary>
		public Guid? CityFiasGuid { get; set; }
		/// <summary>
		/// Код улицы из ФИАСа
		/// </summary>
		public Guid? StreetFiasGuid { get; set; }
		/// <summary>
		/// Код дома из ФИАСа
		/// </summary>
		public Guid? BuildingFiasGuid { get; set; }
	}
}
