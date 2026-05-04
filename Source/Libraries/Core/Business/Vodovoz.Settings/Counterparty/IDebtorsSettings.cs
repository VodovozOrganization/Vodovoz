using System;

namespace Vodovoz.Settings.Counterparty
{
	public interface IDebtorsSettings
	{
		/// <summary>
		/// Скрывать приостановленных контрагентов
		/// </summary>
		int GetSuspendedCounterpartyId { get; }

		/// <summary>
		/// Скрывать аннулированных контрагентов
		/// </summary>

		int GetCancellationCounterpartyId { get; }

		/// <summary>
		/// Воркер по рассылке писем о задолженности отключен
		/// </summary>
		bool DebtNotificationWorkerIsDisabled { get; set; }

		/// <summary>
		/// Интервал срабатывания воркера по рассылке писем о задолженности в секундах
		/// </summary>
		int DebtNotificationWorkerIntervalSeconds { get; }

		/// <summary>
		/// Количество дней сверх ПДЗ до отправки претензионного письма
		/// </summary>
		int LettersOfClaimTimeoutDays { get; }

		/// <summary>
		/// Интервал работы воркера по рассылке претензионных писем
		/// </summary>
		TimeSpan LettersOfClaimWorkerInterval { get; }

		/// <summary>
		/// Максимальное количество претензионных писем, которое может быть отправлено за один цикл воркера
		/// </summary>
		int LettersOfClaimMaxCountPerInterval { get; }

		/// <summary>
		/// Максимальное количество претензионных писем, которое может быть отправлено за один день
		/// </summary>
		int LettersOfClaimMaxCountPerDay { get; }

		/// <summary>
		/// Интервал повторной отправки письма о претензии, если долг не был погашен после предыдущей отправки, в днях
		/// </summary>
		int LettersOfClaimResendIntervalDays { get; }

		/// <summary>
		/// Исполнитель, указанный в документе претензия по долгу
		/// </summary>
		string ClaimDocumentCreatedBy { get; }

		/// <summary>
		/// Номер телефона исполнителя, указанный в документе претензия по долгу
		/// </summary>
		string ClaimDocumentCreatorPhone { get; }

		/// <summary>
		/// Установка значения количества дней сверх ПДЗ до отправки претензионного письма
		/// </summary>
		/// <param name="value">Количество дней сверх ПДЗ</param>
		void SetLettersOfClaimTimeoutDays(int value);

		/// <summary>
		/// Установить исполнителя указанного в документе претензия по долгу
		/// </summary>
		/// <param name="value">Исполнитель</param>
		void SetClaimDocumentCreatedBy(string value);

		/// <summary>
		/// Установить номер телефона исполнителя, указанного в документе претензия по долгу
		/// </summary>
		/// <param name="value">Номер телефона исполнителя</param>
		void SetClaimDocumentCreatorPhone(string value);
	}
}
