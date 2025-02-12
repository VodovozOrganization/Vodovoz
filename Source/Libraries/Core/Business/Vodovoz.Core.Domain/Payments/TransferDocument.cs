using System;
using System.Data.Bindings;

namespace Vodovoz.Core.Domain.Payments
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

		public string DocNum { get; set; }

		public DateTime Date { get; set; }

		public decimal Total { get; set; }

		#region Плательщик

		public string Payer { get; set; }

		public string PayerAccount { get; set; } // р/сч плательщика

		public string PayerCurrentAccount { get; set; } // р/сч плательщика

		public DateTime? WriteOffDate { get; set; } // дата списания

		public string PayerInn { get; set; }

		public string PayerKpp { get; set; }

		public string PayerName { get; set; }

		public string PayerBank { get; set; }

		public string CityOfPayerBank { get; set; }

		public string PayerBik { get; set; }

		public string PayerCorrespondentAccount { get; set; }

		#endregion Плательщик

		#region Получатель

		public string Recipient { get; set; }

		public string RecipientAccount { get; set; } // р/сч получателя

		public string RecipientCurrentAccount { get; set; } // р/сч получателя

		public DateTime? ReceiptDate { get; set; } // дата поступления

		public string RecipientInn { get; set; }

		public string RecipientKpp { get; set; }

		public string RecipientName { get; set; }

		public string RecipientBank { get; set; }

		public string CityOfRecipientBank { get; set; }

		public string RecipientBik { get; set; }

		public string RecipientCorrespondentAccount { get; set; }

		#endregion Получатель

		public string PaymentPurpose { get; set; }

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
			switch(type)
			{
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
