using Vodovoz.Domain.FastPayments;

namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Сообщение со статусом оплаты и ошибкой
	/// </summary>
	public interface IFastPaymentStatusDto : IErrorResponse
	{
		/// <summary>
		/// Статус платежа
		/// </summary>
		FastPaymentStatus? FastPaymentStatus { get; set; }
		/// <summary>
		/// Сообщение об ошибке/проблеме
		/// </summary>
		string ErrorMessage { get; set; }
	}
}
