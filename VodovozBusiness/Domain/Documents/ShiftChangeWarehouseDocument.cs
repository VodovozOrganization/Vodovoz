using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "акты передачи склада",
		Nominative = "акт передачи склада",
		Prepositional = "акте передачи склада")]
	public class ShiftChangeWarehouseDocument : Document, IValidatableObject
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set { base.TimeStamp = value; }
		}

		string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		Warehouse warehouse;

		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set {
				SetField(ref warehouse, value, () => Warehouse);
			}
		}

		IList<ShiftChangeWarehouseDocumentItem> items = new List<ShiftChangeWarehouseDocumentItem>();

		[Display(Name = "Строки")]
		public virtual IList<ShiftChangeWarehouseDocumentItem> Items {
			get { return items; }
			set {
				SetField(ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<ShiftChangeWarehouseDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ShiftChangeWarehouseDocumentItem> ObservableItems {
			get {
				if(observableItems == null)
					observableItems = new GenericObservableList<ShiftChangeWarehouseDocumentItem>(Items);
				return observableItems;
			}
		}

		public virtual string Title {
			get { return String.Format("Акт передачи склада №{0} от {1:d}", Id, TimeStamp); }
		}

		#region Функции

		public virtual void AddItem(Nomenclature nomenclature, decimal amountInDB, decimal amountInFact)
		{
			var item = new ShiftChangeWarehouseDocumentItem() {
				Nomenclature = nomenclature,
				AmountInDB = amountInDB,
				AmountInFact = amountInFact,
				Document = this
			};
			ObservableItems.Add(item);
		}

		public virtual void FillItemsFromStock(IUnitOfWork uow)
		{
			var inStock = Repository.StockRepository.NomenclatureInStock(uow, Warehouse.Id);
			if(inStock.Count == 0)
				return;

			var nomenclatures = uow.GetById<Nomenclature>(inStock.Select(p => p.Key).ToArray());

			ObservableItems.Clear();
			foreach(var itemInStock in inStock) {
				ObservableItems.Add(
					new ShiftChangeWarehouseDocumentItem() {
						Nomenclature = nomenclatures.First(x => x.Id == itemInStock.Key),
						AmountInDB = itemInStock.Value,
						AmountInFact = 0,
						Document = this
					}
				);
			}
		}

		public virtual void UpdateItemsFromStock(IUnitOfWork uow)
		{
			var inStock = Repository.StockRepository.NomenclatureInStock(uow, Warehouse.Id, TimeStamp);

			foreach(var itemInStock in inStock) {
				var item = Items.FirstOrDefault(x => x.Nomenclature.Id == itemInStock.Key);
				if(item != null)
					item.AmountInDB = itemInStock.Value;
				else {
					ObservableItems.Add(
						new ShiftChangeWarehouseDocumentItem() {
							Nomenclature = uow.GetById<Nomenclature>(itemInStock.Key),
							AmountInDB = itemInStock.Value,
							AmountInFact = 0,
							Document = this
						});
				}
			}
			foreach(var item in Items) {
				if(!inStock.ContainsKey(item.Nomenclature.Id))
					item.AmountInDB = 0;
			}
		}

		#endregion

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Items.Count == 0)
				yield return new ValidationResult(String.Format("Табличная часть документа пустая."),
					new[] { this.GetPropertyName(o => o.Items) });

			if(TimeStamp == default(DateTime))
				yield return new ValidationResult(String.Format("Дата документа должна быть указана."),
					new[] { this.GetPropertyName(o => o.TimeStamp) });
		}

		public ShiftChangeWarehouseDocument()
		{

		}
	}
}
