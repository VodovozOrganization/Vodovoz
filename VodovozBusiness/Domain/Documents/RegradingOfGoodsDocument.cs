using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "пересортицы товаров",
		Nominative = "пересортица товаров")]
	public class RegradingOfGoodsDocument: Document, IValidatableObject
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				foreach (var item in Items) {
					if (item.WarehouseWriteOffOperation != null && item.WarehouseWriteOffOperation.OperationTime != TimeStamp)
						item.WarehouseWriteOffOperation.OperationTime = TimeStamp;
					if (item.WarehouseIncomeOperation != null && item.WarehouseIncomeOperation.OperationTime != TimeStamp)
						item.WarehouseIncomeOperation.OperationTime = TimeStamp;
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
					if (item.WarehouseWriteOffOperation != null && item.WarehouseWriteOffOperation.WriteoffWarehouse != Warehouse)
						item.WarehouseWriteOffOperation.WriteoffWarehouse = Warehouse;
					if (item.WarehouseIncomeOperation != null && item.WarehouseIncomeOperation.IncomingWarehouse != Warehouse)
						item.WarehouseIncomeOperation.IncomingWarehouse = Warehouse;
					
				}
			}
		}

		IList<RegradingOfGoodsDocumentItem> items = new List<RegradingOfGoodsDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<RegradingOfGoodsDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<RegradingOfGoodsDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<RegradingOfGoodsDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<RegradingOfGoodsDocumentItem> (Items);
				return observableItems;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Пересортица товаров №{0} от {1:d}", Id, TimeStamp); }
		}

		#region Функции

		public virtual void AddItem (RegradingOfGoodsDocumentItem item)
		{
			item.Document = this;
			item.WarehouseIncomeOperation.OperationTime = item.WarehouseWriteOffOperation.OperationTime
				= TimeStamp;
			item.WarehouseIncomeOperation.IncomingWarehouse = item.WarehouseWriteOffOperation.WriteoffWarehouse
				= Warehouse;
			ObservableItems.Add (item);
		}

		#endregion

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(Items.Count == 0)
				yield return new ValidationResult (String.Format("Табличная часть документа пустая."),
					new[] { this.GetPropertyName (o => o.Items) });

			foreach(var item in Items)
			{
				if(item.Amount > item.AmountInStock)
					yield return new ValidationResult (String.Format("На складе недостаточное количество <{0}>", item.NomenclatureOld.Name),
						new[] { this.GetPropertyName (o => o.Items) });

				if(item.NomenclatureOld.Category == NomenclatureCategory.bottle
				   && item.NomenclatureNew.Category == NomenclatureCategory.water
				   && item.Amount > 39)
					yield return new ValidationResult(String.Format("Пересортица из {0} ед. '{1}' в {0} ед. '{2}' невозможна!", item.Amount, item.NomenclatureOld.Name, item.NomenclatureNew.Name), 
					                                  new[] { this.GetPropertyName(o => o.Items) });
			}
		}

		public RegradingOfGoodsDocument ()
		{
		}

	}
}

