using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum DriverCallType
	{
		[Display(Name = "Водитель не звонил")]
		NoCall,
		[Display(Name = "Водитель отзвонился с адреса")]
		CallFromAddress,
		[Display(Name = "Водитель отзвонился не с адреса")]
		CallFromAnywhere,
		[Display(Name = "Комментарий загружен из приложения")]
		CommentFromMobileApp,
	}
}
