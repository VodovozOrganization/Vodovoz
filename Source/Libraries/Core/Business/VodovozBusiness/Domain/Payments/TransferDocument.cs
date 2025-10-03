using System;
using System.Data.Bindings;

namespace Vodovoz.Domain.Payments
{
	public class TransferDocument
	{
		public enum TransferDocumentType
		{
			[ItemTitle("Банковский ордер")]
			BankOrder,
			[ItemTitle("Платежное поручение")]
			PaymentDraft,
			[ItemTitle("Платежное требование")]
			PaymentRequest,
			[ItemTitle("Аккредитив")]
			ApplicationOfCredit,
			[ItemTitle("Инкассовое поручение")]
			IncassDraft,
			[ItemTitle("Платежный ордер")]
			PaymentOrder
		}

		public TransferDocumentType docType;
		/// <summary>
		/// Номер документа
		/// </summary>
		public string DocNum { get; set; }
		/// <summary>
		/// Дата документа
		/// </summary>
		public DateTime Date { get; set; }
		/// <summary>
		/// КвитанцияДата
		/// </summary>
		public DateTime? ReceiptDate { get; set; }
		/// <summary>
		/// Сумма
		/// </summary>
		public decimal Total { get; set; }

		#region Плательщик
		/// <summary>
		/// Плательщик
		/// </summary>
		public string Payer { get; set; }
		/// <summary>
		/// р/сч плательщика
		/// </summary>
		public string PayerAccount { get; set; }
		/// <summary>
		/// р/сч плательщика
		/// </summary>
		public string PayerCurrentAccount { get; set; }
		/// <summary>
		/// дата списания
		/// </summary>
		public DateTime? WriteOffDate { get; set; }
		/// <summary>
		/// ИНН плательщика
		/// </summary>
		public string PayerInn { get; set; }
		/// <summary>
		/// КПП плательщика
		/// </summary>
		public string PayerKpp { get; set; }
		/// <summary>
		/// Плательщик1
		/// </summary>
		public string PayerName { get; set; }
		/// <summary>
		/// Банк плательщика
		/// </summary>
		public string PayerBank { get; set; }
		/// <summary>
		/// Город банка плательщика
		/// </summary>
		public string CityOfPayerBank { get; set; }
		/// <summary>
		/// БИК банка плательщика
		/// </summary>
		public string PayerBik { get; set; }
		/// <summary>
		/// К/сч плательщика
		/// </summary>
		public string PayerCorrespondentAccount { get; set; }

		#endregion Плательщик

		#region Получатель
		/// <summary>
		/// Получатель
		/// </summary>
		public string Recipient { get; set; }
		/// <summary>
		/// ПолучательСчет(р/сч получателя)
		/// </summary>
		public string RecipientAccount { get; set; }
		/// <summary>
		/// Получатель2 или ПолучательРасчСчет(р/сч получателя)
		/// </summary>
		public string RecipientCurrentAccount { get; set; }
		/// <summary>
		/// Дата поступления
		/// </summary>
		public DateTime? ReceivedDate { get; set; }
		/// <summary>
		/// ИНН получателя
		/// </summary>
		public string RecipientInn { get; set; }
		/// <summary>
		/// КПП получателя
		/// </summary>
		public string RecipientKpp { get; set; }
		/// <summary>
		/// Получатель1
		/// </summary>
		public string RecipientName { get; set; }
		/// <summary>
		/// Банк получателя
		/// </summary>
		public string RecipientBank { get; set; }
		/// <summary>
		/// Город банка получателя
		/// </summary>
		public string CityOfRecipientBank { get; set; }
		/// <summary>
		/// Бик банка получателя
		/// </summary>
		public string RecipientBik { get; set; }
		/// <summary>
		/// К/сч получателя
		/// </summary>
		public string RecipientCorrespondentAccount { get; set; }

		#endregion Получатель

		/// <summary>
		/// Основание
		/// </summary>
		public string PaymentPurpose { get; set; }
		/// <summary>
		/// Тип платежа
		/// </summary>
		public string PaymentType { get; set; }

		/// <summary>
		/// КодНазПлатежа
		/// </summary>
		public string PaymentCode { get; set; }

		/// <summary>
		/// ВидОплаты
		/// </summary>
		public string OperationType { get; set; }

		/// <summary>
		/// Код
		/// </summary>
		public string PaymentId { get; set; }

		/// <summary>
		/// СтатусСоставителя
		/// </summary>
		public string AuthorStatus { get; set; }

		/// <summary>
		/// ПоказательКБК
		/// </summary>
		public string BudgetСlassificationСode { get; set; }

		/// <summary>
		/// ПоказательОснования
		/// </summary>
		public string BaseIndicator { get; set; }

		/// <summary>
		/// ОКАТО
		/// </summary>
		public string Okato { get; set; }

		/// <summary>
		/// ПоказательПериода
		/// </summary>
		public string PeriodIndicator { get; set; }

		/// <summary>
		/// ПоказательНомера
		/// </summary>
		public string NumIndicator { get; set; }

		/// <summary>
		/// ПоказательДаты
		/// </summary>
		public string DateIndicator { get; set; }

		/// <summary>
		/// ПоказательТипа
		/// </summary>
		public string TypeIndicator { get; set; }

		/// <summary>
		/// Очередность
		/// </summary>
		public string Priority { get; set; }

		/// <summary>
		/// СрокАкцепта
		/// </summary>
		public string AcceptancePeriod { get; set; }

		/// <summary>
		/// ВидАккредитива
		/// </summary>
		public string ApplicationOfCreditType { get; set; }

		/// <summary>
		/// СрокПлатежа
		/// </summary>
		public string PaymentTerm { get; set; }

		/// <summary>
		/// УсловиеОплаты1
		/// </summary>
		public string PaymentConditionStr1 { get; set; }

		/// <summary>
		/// УсловиеОплаты2
		/// </summary>
		public string PaymentConditionStr2 { get; set; }

		/// <summary>
		/// УсловиеОплаты3
		/// </summary>
		public string PaymentConditionStr3 { get; set; }

		/// <summary>
		/// ПлатежПоПредст
		/// </summary>
		public string PaymentBySubmission { get; set; }

		/// <summary>
		/// ДополнУсловия
		/// </summary>
		public string AdditionalTerms { get; set; }

		/// <summary>
		/// НомерСчетаПоставщика
		/// </summary>
		public string VendorAccount { get; set; }

		/// <summary>
		/// ДатаОтсылкиДок
		/// </summary>
		public string DocSendDate { get; set; }

		public static TransferDocumentType GetDocTypeFromString(string type)
		{
			switch(type) {
				case "Банковский ордер":
					return TransferDocumentType.BankOrder;
				case "Платежное поручение":
					return TransferDocumentType.PaymentDraft;
				case "Платежное требование":
					return TransferDocumentType.PaymentRequest;
				case "Аккредитив":
					return TransferDocumentType.ApplicationOfCredit;
				case "Инкассовое поручение":
					return TransferDocumentType.IncassDraft;
				case "Платежный ордер":
					return TransferDocumentType.PaymentOrder;
				default:
					throw new NotSupportedException($"Тип банковского документа \"{type}\" неизвестен. Обратитесь в РПО.");
			}
		}
	}
}
