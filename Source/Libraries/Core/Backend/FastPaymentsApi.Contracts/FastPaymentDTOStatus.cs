using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts
{
	public enum FastPaymentDTOStatus
	{
		[Display(Name = "Заказ не найден")]
		[XmlEnum("0")]
		OrderNotFound = 0,
		[Display(Name = "Обрабатывается")]
		[XmlEnum("1")]
		Processing = 1,
		[Display(Name = "Отбракован")]
		[XmlEnum("2")]
		Rejected = 2,
		[Display(Name = "Исполнен")]
		[XmlEnum("3")]
		Performed = 3,
		[Display(Name = "Частичный возврат")]
		[XmlEnum("5")]
		PartialRefund = 5,
		[Display(Name = "Возврат")]
		[XmlEnum("6")]
		Refund = 6
	}
}
