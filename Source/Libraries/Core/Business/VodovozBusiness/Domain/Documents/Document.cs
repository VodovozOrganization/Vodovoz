using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.WriteOffDocuments;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Documents
{
	public class Document : PropertyChangedBase, IDomainObject, IDocument
	{
		private DateTime _timeStamp = DateTime.Now;
		private DateTime _version;

		public virtual int Id { get; set; }

		public virtual bool CanEdit { get; set; }

		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}

		public virtual DateTime TimeStamp
		{
			get => _timeStamp;
			set => SetField (ref _timeStamp, value);
		}

		Employee author;

		[Display (Name = "Автор")]
		public virtual Employee Author
		{
			get => author;
			set => SetField(ref author, value);
		}

		Employee lastEditor;

		[Display (Name = "Последний редактор")]
		public virtual Employee LastEditor
		{
			get => lastEditor;
			set => SetField (ref lastEditor, value);
		}

		DateTime lastEditedTime;

		[Display (Name = "Последние изменения")]
		public virtual DateTime LastEditedTime
		{
			get => lastEditedTime;
			set => SetField (ref lastEditedTime, value);
		}

		public virtual string DateString => TimeStamp.ToShortDateString() + " " + TimeStamp.ToShortTimeString();

		public virtual string Number => Id.ToString();

		#region static

		public static Type GetDocClass(DocumentType docType)
		{
			switch(docType)
			{
				case DocumentType.IncomingInvoice:
					return typeof(IncomingInvoice);
				case DocumentType.IncomingWater:
					return typeof(IncomingWater);
				case DocumentType.MovementDocument:
					return typeof(MovementDocument);
				case DocumentType.WriteoffDocument:
					return typeof(WriteOffDocument);
				case DocumentType.SelfDeliveryDocument:
					return typeof(SelfDeliveryDocument);
				case DocumentType.CarLoadDocument:
					return typeof(CarLoadDocument);
				case DocumentType.CarUnloadDocument:
					return typeof(CarUnloadDocument);
				case DocumentType.InventoryDocument:
					return typeof(InventoryDocument);
				case DocumentType.ShiftChangeDocument:
					return typeof(ShiftChangeWarehouseDocument);
				case DocumentType.RegradingOfGoodsDocument:
					return typeof(RegradingOfGoodsDocument);
				case DocumentType.DeliveryDocument:
					return typeof(DeliveryDocument);
				case DocumentType.DriverTerminalMovement:
					return typeof(DriverAttachedTerminalDocumentBase);
				case DocumentType.DriverTerminalGiveout:
					return typeof(DriverAttachedTerminalGiveoutDocument);
				case DocumentType.DriverTerminalReturn:
					return typeof(DriverAttachedTerminalReturnDocument);
			}
			throw new NotSupportedException();
		}

		#endregion
	}

	public enum DocumentType
	{
		[Display(Name = "Входящая накладная")]
		IncomingInvoice,
		[Display(Name = "Документ производства")]
		IncomingWater,
		[Display(Name = "Документ перемещения")]
		MovementDocument,
		[Display(Name = "Акт списания")]
		WriteoffDocument,
		[Display(Name = "Отпуск самовывоза")]
		SelfDeliveryDocument,
		[Display(Name = "Талон погрузки")]
		CarLoadDocument,
		[Display(Name = "Талон разгрузки")]
		CarUnloadDocument,
		[Display(Name = "Инвентаризация")]
		InventoryDocument,
		[Display(Name = "Акт передачи остатков")]
		ShiftChangeDocument,
		[Display (Name = "Пересортица товаров")]
		RegradingOfGoodsDocument,
		[Display (Name = "Документ доставки")]
		DeliveryDocument,
		[Display(Name = "Документы перемещения терминала водителя")]
		DriverTerminalMovement,
		[Display(Name = "Документ выдачи терминала водителя")]
		DriverTerminalGiveout,
		[Display(Name = "Документ возврата терминала водителя")]
		DriverTerminalReturn
	}

	public class DocumentTypeStringType : NHibernate.Type.EnumStringType
	{
		public DocumentTypeStringType () : base (typeof(DocumentType))
		{
		}
	}

	public enum DefectSource
	{
		[Display(Name = "")]
		None,
		[Display(Name = "Водитель")]
		Driver,
		[Display(Name = "Клиент")]
		Client,
		[Display(Name = "Производство")]
		Production,
		[Display(Name = "Склад")]
		Warehouse
	}

	public class DefectSourceStringType : NHibernate.Type.EnumStringType
	{
		public DefectSourceStringType() : base(typeof(DefectSource))
		{
		}
	}
}

