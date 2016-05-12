using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QSOrmProject;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "инвентаризации",
		Nominative = "инвентаризация")]
	public class InventoryDocument: Document, IValidatableObject
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				foreach (var item in Items) {
					if (item.WarehouseChangeOperation != null && item.WarehouseChangeOperation.OperationTime != TimeStamp)
						item.WarehouseChangeOperation.OperationTime = TimeStamp;
				}
			}
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		Warehouse warehouse;

		[Display (Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set {
				SetField (ref warehouse, value, () => Warehouse);

				foreach (var item in Items) {
					if (item.WarehouseChangeOperation != null && item.WarehouseChangeOperation.WriteoffWarehouse != Warehouse)
						item.WarehouseChangeOperation.WriteoffWarehouse = Warehouse;
				}
			}
		}

		IList<InventoryDocumentItem> items = new List<InventoryDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<InventoryDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<InventoryDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<InventoryDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<InventoryDocumentItem> (Items);
				return observableItems;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Инвентаризация №{0} от {1:d}", Id, TimeStamp); }
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Инвентаризация"; }
		}

		new public virtual string Description {
			get { 
				return "";
			}
		}

		#endregion

		#region Функции

		public virtual void AddItem (Nomenclature nomenclature, decimal amountInDB, decimal amountInFact)
		{
			var item = new InventoryDocumentItem()
			{ 
				Nomenclature = nomenclature,
				AmountInDB = amountInDB,
				AmountInFact = amountInFact,
				Document = this
			};
			ObservableItems.Add (item);
		}

		public virtual void FillItemsFromStock(IUnitOfWork uow){
			var inStock = Repository.StockRepository.NomenclatureInStock(uow, Warehouse.Id);
			if (inStock.Count == 0)
				return;

			var nomenclatures = uow.GetById<Nomenclature>(inStock.Select(p => p.Key).ToArray());

			ObservableItems.Clear();
			foreach(var itemInStock in inStock)
			{
				ObservableItems.Add(
					new InventoryDocumentItem(){
					Nomenclature = nomenclatures.First(x => x.Id == itemInStock.Key),
					AmountInDB = itemInStock.Value,
					AmountInFact = itemInStock.Value,
					Document = this
				}
				);
			}
		}

		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			foreach(var item in Items)
			{
				if(item.Difference == 0 && item.WarehouseChangeOperation != null)
				{
					uow.Delete(item.WarehouseChangeOperation);
					item.WarehouseChangeOperation = null;
				}
				if(item.Difference != 0)
				{
					if(item.WarehouseChangeOperation != null)
					{
						item.UpdateOperation(Warehouse);
					}
					else
					{
						item.CreateOperation(Warehouse, TimeStamp);
					}
				}
				if(item.AmountInDB == 0 && item.AmountInFact == 0)
				{
					uow.Delete(item);
					Items.Remove(item);
				}
			}
		}

		#endregion

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(Items.Any(i => i.AmountInDB <= 0))
				yield return new ValidationResult ("В списке списания присутствуют позиции с нулевым количеством.",
					new[] { this.GetPropertyName (o => o.Items) });
		}

		public InventoryDocument ()
		{
		}

	}
}

