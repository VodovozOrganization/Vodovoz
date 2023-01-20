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
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "инвентаризации",
		Nominative = "инвентаризация",
		Prepositional = "инвентаризации")]
	[EntityPermission]
	[HistoryTrace]
	public class InventoryDocument: Document, IValidatableObject
	{
		private string _comment;
		private Warehouse _warehouse;
		private IList<InventoryDocumentItem> _items = new List<InventoryDocumentItem> ();
		private GenericObservableList<InventoryDocumentItem> _observableItems;

		public override DateTime TimeStamp
		{
			get => base.TimeStamp;
			set
			{
				base.TimeStamp = value;
				foreach (var item in Items)
				{
					if (item.WarehouseChangeOperation != null && item.WarehouseChangeOperation.OperationTime != TimeStamp)
					{
						item.WarehouseChangeOperation.OperationTime = TimeStamp;
					}
				}
			}
		}
		
		[Display (Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField (ref _comment, value);
		}


		[Display (Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField (ref _warehouse, value);
		}


		[Display (Name = "Строки")]
		public virtual IList<InventoryDocumentItem> Items
		{
			get => _items;
			set 
			{
				SetField (ref _items, value);
				_observableItems = null;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<InventoryDocumentItem> ObservableItems =>
			_observableItems ?? (_observableItems = new GenericObservableList<InventoryDocumentItem>(Items));

		public virtual string Title => String.Format("Инвентаризация №{0} от {1:d}", Id, TimeStamp);

		public virtual InventoryDocumentType InventoryDocumentType { get; }

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

		public virtual void FillItemsFromStock(IUnitOfWork uow, Dictionary<int, decimal> selectedNomenclature){
			var inStock = selectedNomenclature;

			if (inStock.Count == 0)
				return;

			var nomenclatures = uow.GetById<Nomenclature>(inStock.Select(p => p.Key).ToArray());

			ObservableItems.Clear();
			foreach(var itemInStock in inStock)
			{
				ObservableItems.Add(
					new InventoryDocumentItem
					{
						Nomenclature = nomenclatures.First(x => x.Id == itemInStock.Key),
						AmountInDB = itemInStock.Value,
						AmountInFact = 0,
						Document = this
					}
				);
			}
		}

		public virtual void FillItemsFromStock(
			IUnitOfWork uow,
			IStockRepository stockRepository,
			int[] nomenclaturesToInclude,
			int[] nomenclaturesToExclude,
			NomenclatureCategory[] nomenclatureTypeToInclude,
			NomenclatureCategory[] nomenclatureTypeToExclude,
			int[] productGroupToInclude,
			int[] productGroupToExclude)
		{
			if(Warehouse == null)
				return;

			var selectedNomenclature = stockRepository.NomenclatureInStock(
				uow,
				Warehouse.Id,
				nomenclaturesToInclude,
				nomenclaturesToExclude,
				nomenclatureTypeToInclude,
				nomenclatureTypeToExclude,
				productGroupToInclude,
				productGroupToExclude);
			
			FillItemsFromStock(uow, selectedNomenclature);
		}

		public virtual void UpdateItemsFromStock(
			IUnitOfWork uow,
			IStockRepository stockRepository,
			int[] nomenclaturesToInclude,
			int[] nomenclaturesToExclude,
			NomenclatureCategory[] nomenclatureTypeToInclude,
			NomenclatureCategory[] nomenclatureTypeToExclude,
			int[] productGroupToInclude,
			int[] productGroupToExclude)
		{
			if(Warehouse == null)
				return;

			var inStock = stockRepository.NomenclatureInStock(
				uow,
				Warehouse.Id,
				nomenclaturesToInclude,
				nomenclaturesToExclude,
				nomenclatureTypeToInclude,
				nomenclatureTypeToExclude,
				productGroupToInclude,
				productGroupToExclude,
				TimeStamp);

			foreach(var itemInStock in inStock)
			{
				var item = Items.FirstOrDefault(x => x.Nomenclature.Id == itemInStock.Key);
				if (item != null)
					item.AmountInDB = itemInStock.Value;
				else
				{
					ObservableItems.Add(
						new InventoryDocumentItem()
						{
							Nomenclature = uow.GetById<Nomenclature>(itemInStock.Key),
							AmountInDB = itemInStock.Value,
							AmountInFact = 0,
							Document = this
						});
				}
			}

			var itemsToRemove = new List<InventoryDocumentItem>();
			foreach(var item in Items) {
				if(!inStock.ContainsKey(item.Nomenclature.Id))
					itemsToRemove.Add(item);
			}

			foreach(var item in itemsToRemove) {
				ObservableItems.Remove(item);
			}
		}

		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			IList<InventoryDocumentItem> itemsToDelete = new List<InventoryDocumentItem>();
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
				if(item.AmountInDB == 0 && item.AmountInFact == 0) {
					itemsToDelete.Add(item);
				}
			}

			foreach(var item in itemsToDelete) {
				uow.Delete(item);
				Items.Remove(item);
			}
		}

		#endregion

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(Items.Count == 0)
				yield return new ValidationResult (String.Format("Табличная часть документа пустая."),
					new[] { this.GetPropertyName (o => o.Items) });
			
			if(TimeStamp == default(DateTime))
				yield return new ValidationResult (String.Format("Дата документа должна быть указана."),
					new[] { this.GetPropertyName (o => o.TimeStamp) });

			foreach(var item in Items)
			{
				foreach(var result in item.Validate(new ValidationContext(item)))
				{
					yield return result;
				}
			}

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

		public InventoryDocument ()
		{
		}

	}

	public enum InventoryDocumentType
	{
		WarehouseInventory,
		EmployeeInventory,
		CarInventory
	}
}

