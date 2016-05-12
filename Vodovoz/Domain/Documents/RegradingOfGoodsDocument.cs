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

		public virtual void AddItem (Nomenclature nomenclature, decimal amountInDB, decimal amountInFact)
		{
			var item = new RegradingOfGoodsDocumentItem()
			{ 
				NomenclatureOld = nomenclature,
				Amount = amountInDB,
				AmountInStock = amountInFact,
				Document = this
			};
			ObservableItems.Add (item);
		}

		#endregion

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(Items.Any(i => i.Amount <= 0))
				yield return new ValidationResult ("В списке списания присутствуют позиции с нулевым количеством.",
					new[] { this.GetPropertyName (o => o.Items) });
		}

		public RegradingOfGoodsDocument ()
		{
		}

	}
}

