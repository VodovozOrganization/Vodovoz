using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.StoredEmails
{
	/// <summary>
	/// Состояние письма по заказу сети или тендерного клиента в истории писем
	/// </summary>
	public enum ChainStoreDelayLetterStatus
	{
		/// <summary>
		/// Письмо нужно отправить
		/// </summary>
		[Display(Name = "Требуется отправка")]
		SendRequired,

		/// <summary>
		/// Письмо сформировано и передано на отправку
		/// </summary>
		[Display(Name = "Отправлено")]
		Sent,

		/// <summary>
		/// Заказ оплачен до отправки письма
		/// </summary>
		[Display(Name = "Не отправлено: заказ оплачен до отправки")]
		OrderPaidBeforeSending,

		/// <summary>
		/// Клиент отписался от рассылок
		/// </summary>
		[Display(Name = "Пропущено: клиент отписался от рассылок")]
		ClientUnsubscribed,

		/// <summary>
		/// У клиента запрещена рассылка писем о задолженности
		/// </summary>
		[Display(Name = "Пропущено: у клиента запрещена рассылка писем о задолженности")]
		CounterpartyDebtMailingDisabled,

		/// <summary>
		/// У организации запрещена рассылка писем о задолженности
		/// </summary>
		[Display(Name = "Пропущено: у организации запрещена рассылка писем о задолженности")]
		OrganizationDebtMailingDisabled,

		/// <summary>
		/// У клиента нет e-mail
		/// </summary>
		[Display(Name = "Не отправлено: у клиента нет e-mail")]
		NoClientEmail,

		/// <summary>
		/// У организации не заполнен e-mail отправителя
		/// </summary>
		[Display(Name = "Не отправлено: у организации не заполнен e-mail отправителя")]
		NoSenderEmail,

		/// <summary>
		/// Письмо не сформировано до расчётной даты платежа
		/// </summary>
		[Display(Name = "Не отправлено: наступила расчётная дата платежа")]
		PaymentDateReached
	}
}
