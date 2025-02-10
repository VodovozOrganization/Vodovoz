using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Edo
{
	public class OutgoingEdoDocument : PropertyChangedBase
	{
		private int _id;
		private DateTime _creationTime;
		private OutgoingEdoDocumentType _type;
		private EdoType _edoType;
		private EdoDocumentType _documentType;
		private EdoDocumentStatus _status;
		private DateTime? _sendTime;
		private DateTime? _acceptTime;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}

		[Display(Name = "Тип")]
		public virtual OutgoingEdoDocumentType Type
		{
			get => _type;
			set => SetField(ref _type, value);
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
		public virtual DateTime? SendTime
		{
			get => _sendTime;
			set => SetField(ref _sendTime, value);
		}

		[Display(Name = "Время приема")]
		public virtual DateTime? AcceptTime
		{
			get => _acceptTime;
			set => SetField(ref _acceptTime, value);
		}
	}

	

	//public class EdoReceipt : PropertyChangedBase, IDomainObject
	//{
	//	private int _id;
	//	private DateTime _creationTime;
	//	private DateTime _version;
	//	private ReceiptEdoTask _receiptEdoTask;


	//	[Display(Name = "Код")]
	//	public virtual int Id
	//	{
	//		get => _id;
	//		set => SetField(ref _id, value);
	//	}

	//	[Display(Name = "Дата создания")]
	//	public virtual DateTime CreationTime
	//	{
	//		get => _creationTime;
	//		set => SetField(ref _creationTime, value);
	//	}

	//	[Display(Name = "Версия")]
	//	public virtual DateTime Version
	//	{
	//		get => _version;
	//		set => SetField(ref _version, value);
	//	}

	//	[Display(Name = "ЭДО задача")]
	//	public virtual ReceiptEdoTask ReceiptEdoTask
	//	{
	//		get => _receiptEdoTask;
	//		set => SetField(ref _receiptEdoTask, value);
	//	}





	//	public static string GetDocumentId(int orderId, int? innerNumber)
	//	{
	//		return innerNumber is null ? $"vod_{orderId}" : $"vod_{orderId}_{innerNumber}";
	//	}

	//	//public virtual string DocumentId => GetDocumentId(Order.Id, InnerNumber);

	//	public static int MaxMarkCodesInReceipt => 128;
	//}
}
