using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Domain.Receipts
{
	/// <summary>
	/// Планируемый или созданный фискальный документ в рамках процесса корректировки.
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ процесса корректировки",
		NominativePlural = "документы процесса корректировки")]
	public class ReceiptCorrectionProcessDocument : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private ReceiptCorrectionProcess _process;
		private FiscalDocumentType _plannedDocumentType;
		private Guid _documentGuid;
		private int? _edoFiscalDocumentId;
		private ReceiptCorrectionProcessStatus _status;
		private string _errorDescription;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Процесс")]
		public virtual ReceiptCorrectionProcess Process
		{
			get => _process;
			set => SetField(ref _process, value);
		}

		[Display(Name = "Тип документа")]
		public virtual FiscalDocumentType PlannedDocumentType
		{
			get => _plannedDocumentType;
			set => SetField(ref _plannedDocumentType, value);
		}

		[Display(Name = "GUID документа")]
		public virtual Guid DocumentGuid
		{
			get => _documentGuid;
			set => SetField(ref _documentGuid, value);
		}

		[Display(Name = "Фискальный документ ЭДО")]
		public virtual int? EdoFiscalDocumentId
		{
			get => _edoFiscalDocumentId;
			set => SetField(ref _edoFiscalDocumentId, value);
		}

		[Display(Name = "Статус")]
		public virtual ReceiptCorrectionProcessStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Описание ошибки")]
		public virtual string ErrorDescription
		{
			get => _errorDescription;
			set => SetField(ref _errorDescription, value);
		}
	}
}
