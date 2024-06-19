using System;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Данные для ответа на запрос из ДВ
	/// </summary>
	public class FastPaymentResponseDTO : IFastPaymentStatusDto
	{
		public FastPaymentResponseDTO()
		{
		}

		public FastPaymentResponseDTO(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}

		/// <summary>
		/// Сообщени еоб ошибке/проблеме
		/// </summary>
		public string ErrorMessage { get; set; }
		/// <summary>
		/// Сессия платежа
		/// </summary>
		public string Ticket { get; set; }
		/// <summary>
		/// Guid для ссылки на оплату
		/// </summary>
		public Guid FastPaymentGuid { get; set; }
		/// <summary>
		/// Статус платежа
		/// </summary>
		public FastPaymentStatus? FastPaymentStatus { get; set; }
	}
}
