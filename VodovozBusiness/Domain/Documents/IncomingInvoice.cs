using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "входящие накладные",
		Nominative = "входящая накладная")]
	//[HistoryTrace]
	public class IncomingInvoice : Document, IValidatableObject
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
		[Required(ErrorMessage = "Склад должен быть указан.")]
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

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
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

		public virtual string Title => String.Format("Поступление №{0} от {1:d}", Id, TimeStamp);

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

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(Items.Count == 0)
				yield return new ValidationResult (String.Format("Табличная часть документа пустая."),
					new[] { this.GetPropertyName (o => o.Items) });

			foreach(var item in Items)
			{
				if(item.Amount <= 0)
					yield return new ValidationResult (String.Format("Для номенклатуры <{0}> не указано количество.", item.Nomenclature.Name),
						new[] { this.GetPropertyName (o => o.Items) });
			}
		}
	}
}

