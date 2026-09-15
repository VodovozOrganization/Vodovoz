using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Receipts
{
	/// <summary>
	/// Пачка фискальных документов корректировки по изменению заказа.
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "процесс корректировки чека",
		NominativePlural = "процессы корректировки чеков")]
	public class ReceiptCorrectionProcess : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _orderId;
		private int? _sourceEdoFiscalDocumentId;
		private int? _receiptEdoTaskId;
		private string _baselineFiscalDocumentNumber;
		private DateTime? _baselineFiscalDocumentDate;
		private decimal _baselineSum;
		private ReceiptCorrectionScenarioType _scenarioType;
		private ReceiptCorrectionProcessStatus _status;
		private string _changeFingerprint;
		private string _errorDescription;
		private DateTime _createdDate;
		private DateTime? _completedDate;
		private int? _organizationId;
		private IList<ReceiptCorrectionProcessDocument> _documents = new List<ReceiptCorrectionProcessDocument>();
		private IList<ReceiptCorrectionExplanatoryNote> _explanatoryNotes = new List<ReceiptCorrectionExplanatoryNote>();

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Заказ")]
		public virtual int OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		[Display(Name = "Исходный фискальный документ ЭДО")]
		public virtual int? SourceEdoFiscalDocumentId
		{
			get => _sourceEdoFiscalDocumentId;
			set => SetField(ref _sourceEdoFiscalDocumentId, value);
		}

		[Display(Name = "Задача ЭДО корректировки")]
		public virtual int? ReceiptEdoTaskId
		{
			get => _receiptEdoTaskId;
			set => SetField(ref _receiptEdoTaskId, value);
		}

		[Display(Name = "Номер исходного чека")]
		public virtual string BaselineFiscalDocumentNumber
		{
			get => _baselineFiscalDocumentNumber;
			set => SetField(ref _baselineFiscalDocumentNumber, value);
		}

		[Display(Name = "Дата исходного чека")]
		public virtual DateTime? BaselineFiscalDocumentDate
		{
			get => _baselineFiscalDocumentDate;
			set => SetField(ref _baselineFiscalDocumentDate, value);
		}

		[Display(Name = "Сумма исходного чека")]
		public virtual decimal BaselineSum
		{
			get => _baselineSum;
			set => SetField(ref _baselineSum, value);
		}

		[Display(Name = "Сценарий")]
		public virtual ReceiptCorrectionScenarioType ScenarioType
		{
			get => _scenarioType;
			set => SetField(ref _scenarioType, value);
		}

		[Display(Name = "Статус")]
		public virtual ReceiptCorrectionProcessStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Отпечаток изменений")]
		public virtual string ChangeFingerprint
		{
			get => _changeFingerprint;
			set => SetField(ref _changeFingerprint, value);
		}

		[Display(Name = "Описание ошибки")]
		public virtual string ErrorDescription
		{
			get => _errorDescription;
			set => SetField(ref _errorDescription, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime CreatedDate
		{
			get => _createdDate;
			set => SetField(ref _createdDate, value);
		}

		[Display(Name = "Дата завершения")]
		public virtual DateTime? CompletedDate
		{
			get => _completedDate;
			set => SetField(ref _completedDate, value);
		}

		[Display(Name = "Организация")]
		public virtual int? OrganizationId
		{
			get => _organizationId;
			set => SetField(ref _organizationId, value);
		}

		[Display(Name = "Документы")]
		public virtual IList<ReceiptCorrectionProcessDocument> Documents
		{
			get => _documents;
			set => SetField(ref _documents, value);
		}

		[Display(Name = "Объяснительные записки")]
		public virtual IList<ReceiptCorrectionExplanatoryNote> ExplanatoryNotes
		{
			get => _explanatoryNotes;
			set => SetField(ref _explanatoryNotes, value);
		}
	}
}
