using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	/// <summary>
	/// Тип заявки на выдачу денежных средств
	/// </summary>
	public enum PayoutRequestDocumentType
	{
		/// <summary>
		/// Заявка на выдачу наличных ДС -
		/// Заявка на выдачу наличных денежных средств
		/// </summary>
		[Display(Name = "Заявка на выдачу наличных ДС")]
		CashRequest,

		/// <summary>
		/// Заявка на оплату по Б/Н - 
		/// Заявка на выдачу денежных средств по безналичному расчету
		/// </summary>
		[Display(Name = "Заявка на оплату по Б/Н")]
		CashlessRequest
	}
}
