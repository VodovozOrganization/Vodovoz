using System;
using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V6.Requests
{
	/// <summary>
	/// Запрос на изменения типа оплаты
	/// </summary>
	public class ChangeOrderPaymentTypeRequest
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		[Required]
		public int OrderId { get; set; }

		/// <summary>
		/// Тип оплаты на который нужно сменить тип оплаты заказа
		/// </summary>
		[Required]
		public PaymentDtoType NewPaymentType { get; set; }

		/// <summary>
		/// Время операции на стороне мобильного приложения водителя
		/// </summary>
		[Required]
		public DateTime ActionTimeUtc { get; set; }
	}
}
