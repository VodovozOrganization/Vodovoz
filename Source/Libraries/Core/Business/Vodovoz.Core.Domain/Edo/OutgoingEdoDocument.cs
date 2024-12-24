using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class OutgoingEdoDocument : PropertyChangedBase
	{
		private int _id;
		private OutgoingEdoDocumentType _type;
		private Guid _documentId;
		private EdoType _edoType;
		private EdoDocumentType _documentType;
		private EdoDocumentStatus _status;
		private DateTime _sendTime;
		private DateTime _acceptTime;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Тип")]
		public virtual OutgoingEdoDocumentType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		[Display(Name = "Номер документа")]
		public virtual Guid DocumentId
		{
			get => _documentId;
			set => SetField(ref _documentId, value);
		}

		[Display(Name = "Тип ЭДО")]
		public virtual EdoType EdoType
		{
			get => _edoType;
			set => SetField(ref _edoType, value);
		}

		[Display(Name = "Тип документа")]
		public virtual EdoDocumentType DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}

		[Display(Name = "Статус")]
		public virtual EdoDocumentStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Время отправки")]
		public virtual DateTime SendTime
		{
			get => _sendTime;
			set => SetField(ref _sendTime, value);
		}

		[Display(Name = "Время приема")]
		public virtual DateTime AcceptTime
		{
			get => _acceptTime;
			set => SetField(ref _acceptTime, value);
		}
	}

	public enum OutgoingEdoDocumentType
	{
		Transfer,
		Customer
	}

	public class TransferEdoDocument : OutgoingEdoDocument
	{
		public int TransferTaskId { get; set; }
	}

	public class CustomerEdoDocument : OutgoingEdoDocument
	{
		public int DocumentTaskId { get; set; }
	}
}
