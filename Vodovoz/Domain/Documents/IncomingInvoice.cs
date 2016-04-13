using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "входящие накладные",
		Nominative = "входящая накладная")]
	public class IncomingInvoice: Document
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				foreach (var item in Items) {
					if (item.IncomeGoodsOperation.OperationTime != TimeStamp)
						item.IncomeGoodsOperation.OperationTime = TimeStamp;
				}
			}
		}

		string invoiceNumber;

		[Display (Name = "Номер счета-фактуры")]
		public virtual string InvoiceNumber {
			get { return invoiceNumber; }
			set { SetField (ref invoiceNumber, value, () => InvoiceNumber); }
		}

		string waybillNumber;

		[Display (Name = "Номер входящей накладной")]
		public virtual string WaybillNumber {
			get { return waybillNumber; }
			set { SetField (ref waybillNumber, value, () => WaybillNumber); }
		}

		Counterparty contractor;

		[Display (Name = "Контрагент")]
		public virtual Counterparty Contractor {
			get { return contractor; }
			set { SetField (ref contractor, value, () => Contractor); }
		}

		Warehouse warehouse;

		[Display (Name = "Склад")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set {
				SetField (ref warehouse, value, () => Warehouse); 
				foreach (var item in Items) {
					if (item.IncomeGoodsOperation.IncomingWarehouse != warehouse)
						item.IncomeGoodsOperation.IncomingWarehouse = warehouse;
				}
			}
		}

		//TODO Map invoice item to database

		IList<IncomingInvoiceItem> items = new List<IncomingInvoiceItem> ();

		[Display (Name = "Строки")]
		public virtual IList<IncomingInvoiceItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<IncomingInvoiceItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IncomingInvoiceItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<IncomingInvoiceItem> (Items);
				return observableItems;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Поступление №{0} от {1:d}", Id, TimeStamp); }
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Входящая накладная"; }
		}

		new public virtual string Description {
			get {
				return String.Format ("Поставщик: {0}; Склад поступления: {1};", 
					Contractor != null ? Contractor.Name : "Не указан",
					Warehouse != null ? Warehouse.Name : "Не указан");
			}
		}

		#endregion

		public virtual void AddItem (IncomingInvoiceItem item)
		{
			item.IncomeGoodsOperation.IncomingWarehouse = warehouse;
			item.IncomeGoodsOperation.OperationTime = TimeStamp;
			item.Document = this;
			ObservableItems.Add (item);
		}

		public IncomingInvoice ()
		{
			WaybillNumber = String.Empty;
			InvoiceNumber = String.Empty;
		}
	}
}

