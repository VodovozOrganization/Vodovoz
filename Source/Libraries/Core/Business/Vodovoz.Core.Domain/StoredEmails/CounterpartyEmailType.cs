using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.StoredEmails
{
	/// <summary>
	/// Типы писем для клиента
	/// </summary>
	public enum CounterpartyEmailType
	{
		/// <summary>
		/// Счёт
		/// </summary>
		[Display(Name = "Счёт")]
		BillDocument,

		/// <summary>
		/// Общий счёт
		/// </summary>
		[Display(Name = "Общий счёт")]
		GeneralBillDocument,

		/// <summary>
		/// УПД
		/// </summary>
		[Display(Name = "УПД")]
		UpdDocument,

		/// <summary>
		/// Долг
		/// </summary>
		[Display(Name = "Долг")]
		DebtBill,

		/// <summary>
		/// Массовая рассылка
		/// </summary>
		[Display(Name = "Массовая рассылка")]
		Bulk,

		/// <summary>
		/// Претензионное письмо
		/// </summary>
		[Display(Name = "Претензионное письмо")]
		LetterOfClaim,

		/// <summary>
		/// Учётные данные
		/// </summary>
		[Display(Name = "Учётные данные")]
		Credential,

		/// <summary>
		/// Код авторизации
		/// </summary>
		[Display(Name = "Код авторизации")]
		AuthorizationCode,

		/// <summary>
		/// Счёт без отгрузки на долг
		/// </summary>
		[Display(Name = "Счёт без отгрузки на долг")]
		OrderWithoutShipmentForDebt,

		/// <summary>
		/// Счёт без отгрузки на постоплату
		/// </summary>
		[Display(Name = "Счёт без отгрузки на постоплату")]
		OrderWithoutShipmentForPayment,

		/// <summary>
		/// Счёт без отгрузки на предоплату
		/// </summary>
		[Display(Name = "Счёт без отгрузки на предоплату")]
		OrderWithoutShipmentForAdvancePayment,

		/// <summary>
		/// Акт приёма-передачи оборудования
		/// </summary>
		[Display(Name = "Акт приёма-передачи оборудования")]
		EquipmentTransfer,

		/// <summary>
		/// Закрытие поставок
		/// </summary>
		[Display(Name = "Закрытие поставок")]
		ClosingDeliveries,

		/// <summary>
		/// Информационное письмо
		/// </summary>
		[Display(Name = "Информационное письмо")]
		InformationLetter,

		/// <summary>
		/// Напоминание о необходимости принятия УПД
		/// </summary>
		[Display(Name = "Напоминание о необходимости принятия УПД")]
		ReminderToAcceptUpd
	}
}
