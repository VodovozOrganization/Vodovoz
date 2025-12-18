using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts
{
	/// <summary>
	/// Статус быстрого платежа
	/// </summary>
	public enum FastPaymentDTOStatus
	{
		/// <summary>
		/// Заказ не найден
		/// </summary>
		[Display(Name = "Заказ не найден")]
		[XmlEnum("0")]
		OrderNotFound = 0,
		/// <summary>
		/// Обрабатывается
		/// </summary>
		[Display(Name = "Обрабатывается")]
		[XmlEnum("1")]
		Processing = 1,
		/// <summary>
		/// Отбракован
		/// </summary>
		[Display(Name = "Отбракован")]
		[XmlEnum("2")]
		Rejected = 2,
		/// <summary>
		/// Исполнен
		/// </summary>
		[Display(Name = "Исполнен")]
		[XmlEnum("3")]
		Performed = 3,
		/// <summary>
		/// Частичный возврат
		/// </summary>
		[Display(Name = "Частичный возврат")]
		[XmlEnum("5")]
		PartialRefund = 5,
		/// <summary>
		/// Возврат
		/// </summary>
		[Display(Name = "Возврат")]
		[XmlEnum("6")]
		Refund = 6
	}
}
