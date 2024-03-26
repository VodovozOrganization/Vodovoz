using System.ComponentModel.DataAnnotations;

namespace CustomerAppsApi.Library.Dto
{
	public class DeliveryPointInfoDto
	{
		[Display(Name = "Тип населенного пункта")]
		public string LocalityType { get; set; }
		[Display(Name = "Тип населенного пункта(сокр.)")]
		public string LocalityTypeShort { get; set; }
		[Display(Name = "Город")]
		public string City { get; set; }
		[Display(Name = "Тип улицы")]
		public string StreetType { get; set; }
		[Display(Name = "Тип улицы(сокр.)")]
		public string StreetTypeShort { get; set; }
		[Display(Name = "Улица")]
		public string Street { get; set; }
		[Display(Name = "Дом/Строение")]
		public string Building { get; set; }
		[Display(Name = "Этаж")]
		public string Floor { get; set; }
		[Display(Name = "Подъезд")]
		public string Entrance { get; set; }
		[Display(Name = "Квартира")]
		public string Room { get; set; }
		[Display(Name = "Широта")]
		public decimal Latitude { get; set; }
		[Display(Name = "Долгота")]
		public decimal Longitude { get; set; }
		[Display(Name = "Тип объекта")]
		public int DeliveryPointCategoryId { get; set; }
		public string OnlineComment { get; set; }
		[Display(Name = "Домофон")]
		public string Intercom { get; set; }
	}
}
