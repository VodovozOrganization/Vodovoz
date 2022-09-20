using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Stock;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "акты передачи склада",
		Nominative = "акт передачи склада",
		Prepositional = "акте передачи склада")]
	[EntityPermission]
	[HistoryTrace]
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

		public virtual string Title => String.Format("Акт передачи склада №{0} от {1:d}", Id, TimeStamp);

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

		public virtual void FillItemsFromStock(
			IUnitOfWork uow, IStockRepository stockRepository, IList<NomenclatureCategory> categories = null)
		{
			Dictionary<int, decimal> inStock = new Dictionary<int, decimal>();

			if(categories != null && categories.Count > 0)
			{
				foreach(var category in categories)
				{
					foreach(var item in stockRepository.NomenclatureInStock(uow, Warehouse.Id, null, category))
					{
						inStock.Add(item.Key, item.Value);
					}
				}
			}
			else
			{
				inStock = stockRepository.NomenclatureInStock(uow, Warehouse.Id);
			}

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

		public virtual void UpdateItemsFromStock(
			IUnitOfWork uow, IStockRepository stockRepository, IList<NomenclatureCategory> categories = null)
		{
			Dictionary<int, decimal> inStock = new Dictionary<int, decimal>();

			if(categories != null && categories.Count > 0)
			{
				foreach(var category in categories)
				{
					foreach(var item in stockRepository.NomenclatureInStock(uow, Warehouse.Id, TimeStamp, category))
					{
						inStock.Add(item.Key, item.Value);
					}
				}
			}
			else
			{
				inStock = stockRepository.NomenclatureInStock(uow, Warehouse.Id, TimeStamp);
			}

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
			var itemsToRemove = new List<ShiftChangeWarehouseDocumentItem>();

			foreach(var item in Items) {
				if(!inStock.ContainsKey(item.Nomenclature.Id))
					itemsToRemove.Add(item);
			}

			foreach(var item in itemsToRemove) {
				ObservableItems.Remove(item);
			}
		}

		public virtual void FillItemsFromStock(
			IUnitOfWork uow,
			IStockRepository stockRepository,
			int[] nomenclaturesToInclude,
			int[] nomenclaturesToExclude,
			string[] nomenclatureTypeToInclude,
			string[] nomenclatureTypeToExclude,
			int[] productGroupToInclude,
			int[] productGroupToExclude)
		{
			Dictionary<int, decimal> inStock = new Dictionary<int, decimal>();

			if (Warehouse == null)
				return;
			
			inStock = stockRepository.NomenclatureInStock(
				uow,
				Warehouse.Id,
				nomenclaturesToInclude,
				nomenclaturesToExclude,
				nomenclatureTypeToInclude,
				nomenclatureTypeToExclude,
				productGroupToInclude,
				productGroupToExclude,
				TimeStamp);

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
		
		public virtual void UpdateItemsFromStock(
			IUnitOfWork uow,
			IStockRepository stockRepository,
			int[] nomenclaturesToInclude,
			int[] nomenclaturesToExclude,
			string[] nomenclatureTypeToInclude,
			string[] nomenclatureTypeToExclude,
			int[] productGroupToInclude,
			int[] productGroupToExclude)
		{
			Dictionary<int, decimal> inStock = new Dictionary<int, decimal>();

			inStock = stockRepository.NomenclatureInStock(
				uow,
				Warehouse.Id,
				nomenclaturesToInclude,
				nomenclaturesToExclude,
				nomenclatureTypeToInclude,
				nomenclatureTypeToExclude,
				productGroupToInclude,
				productGroupToExclude,
				TimeStamp);

			if (Warehouse == null)
				return;

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
			var itemsToRemove = new List<ShiftChangeWarehouseDocumentItem>();

			foreach(var item in Items) {
				if(!inStock.ContainsKey(item.Nomenclature.Id))
					itemsToRemove.Add(item);
			}

			foreach(var item in itemsToRemove) {
				ObservableItems.Remove(item);
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

			var needWeightOrVolume = Items
				.Select(item => item.Nomenclature)
				.Where(nomenclature =>
					Nomenclature.CategoriesWithWeightAndVolume.Contains(nomenclature.Category)
					&& (nomenclature.Weight == default
						|| nomenclature.Length == default
						|| nomenclature.Width == default
						|| nomenclature.Height == default))
				.ToList();
			if(needWeightOrVolume.Any())
			{
				yield return new ValidationResult(
					"Для всех добавленных номенклатур должны быть заполнены вес и объём.\n" +
					"Список номенклатур, в которых не заполнен вес или объём:\n" +
					$"{string.Join("\n", needWeightOrVolume.Select(x => $"({x.Id}) {x.Name}"))}",
					new[] { nameof(Items) });
			}
		}

		public ShiftChangeWarehouseDocument() { }
	}
}
