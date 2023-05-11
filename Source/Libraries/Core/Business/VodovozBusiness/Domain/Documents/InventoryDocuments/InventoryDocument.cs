using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Stock;

namespace Vodovoz.Domain.Documents.InventoryDocuments
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "инвентаризации",
		Nominative = "инвентаризация",
		Prepositional = "инвентаризации")]
	[EntityPermission]
	[HistoryTrace]
	public class InventoryDocument : Document, IValidatableObject
	{
		private string _comment;
		private bool _sortedByNomenclatureName;
		private Warehouse _warehouse;
		private Employee _employee;
		private Car _car;
		private InventoryDocumentType _inventoryDocumentType;
		private InventoryDocumentStatus _inventoryDocumentStatus;
		private IList<InventoryDocumentItem> _nomenclatureItems = new List<InventoryDocumentItem>();
		private IList<InstanceInventoryDocumentItem> _instanceItems = new List<InstanceInventoryDocumentItem>();
		private GenericObservableList<InventoryDocumentItem> _observableNomenclatureItems;
		private GenericObservableList<InstanceInventoryDocumentItem> _observableInstanceItems;

		public InventoryDocument()
		{
			InventoryDocumentStatus = InventoryDocumentStatus.InProcess; 
			InventoryDocumentType = InventoryDocumentType.WarehouseInventory;
		}

		public override DateTime TimeStamp
		{
			get => base.TimeStamp;
			set
			{
				base.TimeStamp = value;
				foreach(var item in NomenclatureItems)
				{
					if(item.WarehouseChangeOperation != null && item.WarehouseChangeOperation.OperationTime != TimeStamp)
					{
						item.WarehouseChangeOperation.OperationTime = TimeStamp;
					}
				}
			}
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set
			{
				if(SetField(ref _warehouse, value) && value != null)
				{
					Employee = null;
					Car = null;
				}
			}
		}

		[Display(Name = "Сотрудник")]
		public virtual Employee Employee
		{
			get => _employee;
			set
			{
				if(SetField(ref _employee, value) && value != null)
				{
					Warehouse = null;
					Car = null;
				}
			}
		}

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set
			{
				if(SetField(ref _car, value) && value != null)
				{
					Warehouse = null;
					Employee = null;
				}
			}
		}

		[Display(Name = "Строки объемного учета")]
		public virtual IList<InventoryDocumentItem> NomenclatureItems
		{
			get => _nomenclatureItems;
			set
			{
				SetField(ref _nomenclatureItems, value);
				_observableNomenclatureItems = null;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<InventoryDocumentItem> ObservableNomenclatureItems =>
			_observableNomenclatureItems ??
			(_observableNomenclatureItems = new GenericObservableList<InventoryDocumentItem>(NomenclatureItems));

		[Display(Name = "Строки экземплярного учета")]
		public virtual IList<InstanceInventoryDocumentItem> InstanceItems
		{
			get => _instanceItems;
			set
			{
				SetField(ref _instanceItems, value);
				_observableInstanceItems = null;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<InstanceInventoryDocumentItem> ObservableInstanceItems =>
			_observableInstanceItems ??
			(_observableInstanceItems = new GenericObservableList<InstanceInventoryDocumentItem>(InstanceItems));

		public virtual string Title => $"Инвентаризация №{Id} от {TimeStamp:d}";

		public virtual InventoryDocumentType InventoryDocumentType
		{
			get => _inventoryDocumentType;
			set => SetField(ref _inventoryDocumentType, value);
		}
		
		public virtual InventoryDocumentStatus InventoryDocumentStatus
		{
			get => _inventoryDocumentStatus;
			set => SetField(ref _inventoryDocumentStatus, value);
		}

		public virtual bool SortedByNomenclatureName
		{
			get => _sortedByNomenclatureName;
			set => SetField(ref _sortedByNomenclatureName, value);
		} 

		public virtual void FillNomenclatureItemsFromStock(
			IUnitOfWork uow,
			IStockRepository stockRepository,
			IEnumerable<int> nomenclaturesToInclude,
			IEnumerable<int> nomenclaturesToExclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToInclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToExclude,
			IEnumerable<int> productGroupToInclude,
			IEnumerable<int> productGroupToExclude)
		{
			if(Warehouse == null)
			{
				return;
			}

			Dictionary<int, decimal> nomenclaturesInStock;
			
			switch(InventoryDocumentType)
			{
				case InventoryDocumentType.WarehouseInventory:
					nomenclaturesInStock = stockRepository.NomenclatureInStock(
						uow,
						Warehouse.Id,
						OperationType.WarehouseBulkGoodsAccountingOperation,
						nomenclaturesToInclude,
						nomenclaturesToExclude,
						nomenclatureTypeToInclude,
						nomenclatureTypeToExclude,
						productGroupToInclude,
						productGroupToExclude,
						TimeStamp);
					break;
				case InventoryDocumentType.EmployeeInventory:
					nomenclaturesInStock = stockRepository.NomenclatureInStock(
						uow,
						Employee.Id,
						OperationType.EmployeeBulkGoodsAccountingOperation,
						nomenclaturesToInclude,
						nomenclaturesToExclude,
						nomenclatureTypeToInclude,
						nomenclatureTypeToExclude,
						productGroupToInclude,
						productGroupToExclude,
						TimeStamp);
					break;
				case InventoryDocumentType.CarInventory:
					nomenclaturesInStock = stockRepository.NomenclatureInStock(
						uow,
						Car.Id,
						OperationType.CarBulkGoodsAccountingOperation,
						nomenclaturesToInclude,
						nomenclaturesToExclude,
						nomenclatureTypeToInclude,
						nomenclatureTypeToExclude,
						productGroupToInclude,
						productGroupToExclude,
						TimeStamp);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			FillNomenclatureItemsFromStock(uow, nomenclaturesInStock);
		}
		
		public virtual void FillNomenclatureItemsFromStock(IUnitOfWork uow, Dictionary<int, decimal> nomenclaturesInStock)
		{
			if(nomenclaturesInStock.Count == 0)
			{
				return;
			}

			var nomenclatures = uow.GetById<Nomenclature>(nomenclaturesInStock.Select(p => p.Key).ToArray());

			ObservableNomenclatureItems.Clear();

			foreach(var itemInStock in nomenclaturesInStock)
			{
				var item = CreateNewNomenclatureItem();
				FillBulkItem(item, nomenclatures.First(x => x.Id == itemInStock.Key), itemInStock.Value, 0);
				ObservableNomenclatureItems.Add(item);
			}
		}
		
		public virtual void UpdateNomenclatureItemsFromStock(
			IUnitOfWork uow,
			IStockRepository stockRepository,
			IEnumerable<int> nomenclaturesToInclude,
			IEnumerable<int> nomenclaturesToExclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToInclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToExclude,
			IEnumerable<int> productGroupToInclude,
			IEnumerable<int> productGroupToExclude)
		{
			if(Warehouse == null)
			{
				return;
			}

			Dictionary<int, decimal> nomenclaturesInStock;
			
			switch(InventoryDocumentType)
			{
				case InventoryDocumentType.WarehouseInventory:
					nomenclaturesInStock = stockRepository.NomenclatureInStock(
						uow,
						Warehouse.Id,
						OperationType.WarehouseBulkGoodsAccountingOperation,
						nomenclaturesToInclude,
						nomenclaturesToExclude,
						nomenclatureTypeToInclude,
						nomenclatureTypeToExclude,
						productGroupToInclude,
						productGroupToExclude,
						TimeStamp);
					break;
				case InventoryDocumentType.EmployeeInventory:
					nomenclaturesInStock = stockRepository.NomenclatureInStock(
						uow,
						Employee.Id,
						OperationType.EmployeeBulkGoodsAccountingOperation,
						nomenclaturesToInclude,
						nomenclaturesToExclude,
						nomenclatureTypeToInclude,
						nomenclatureTypeToExclude,
						productGroupToInclude,
						productGroupToExclude,
						TimeStamp);
					break;
				case InventoryDocumentType.CarInventory:
					nomenclaturesInStock = stockRepository.NomenclatureInStock(
						uow,
						Car.Id,
						OperationType.CarBulkGoodsAccountingOperation,
						nomenclaturesToInclude,
						nomenclaturesToExclude,
						nomenclatureTypeToInclude,
						nomenclatureTypeToExclude,
						productGroupToInclude,
						productGroupToExclude,
						TimeStamp);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			foreach(var itemInStock in nomenclaturesInStock)
			{
				var item = NomenclatureItems.FirstOrDefault(x => x.Nomenclature.Id == itemInStock.Key);
				if(item != null)
				{
					item.AmountInDB = itemInStock.Value;
				}
				else
				{
					item = CreateNewNomenclatureItem();
					FillBulkItem(item, uow.GetById<Nomenclature>(itemInStock.Key), itemInStock.Value, 0);
					ObservableNomenclatureItems.Add(item);
				}
			}

			var itemsToRemove = new List<InventoryDocumentItem>();
			foreach(var item in NomenclatureItems)
			{
				if(!nomenclaturesInStock.ContainsKey(item.Nomenclature.Id))
				{
					itemsToRemove.Add(item);
				}
			}

			foreach(var item in itemsToRemove)
			{
				ObservableNomenclatureItems.Remove(item);
			}
		}
		
		public virtual void AddNomenclatureItem(Nomenclature nomenclature, decimal amountInDb, decimal amountInFact)
		{
			var item = CreateNewNomenclatureItem();
			FillBulkItem(item, nomenclature, amountInDb, amountInFact);
			ObservableNomenclatureItems.Add(item);
		}
		
		public virtual void AddInstanceItem(InstanceInventoryDocumentItem instanceInventoryDocumentItem)
		{
			ObservableInstanceItems.Add(instanceInventoryDocumentItem);
		}
		
		//TODO проверить маппинг, возможно стоит настроить там удаление строк и операций
		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			IList<InventoryDocumentItem> itemsToDelete = new List<InventoryDocumentItem>();
			foreach(var item in NomenclatureItems)
			{
				if(item.Difference == 0 && item.WarehouseChangeOperation != null)
				{
					uow.Delete(item.WarehouseChangeOperation);
					item.WarehouseChangeOperation = null;
				}
				
				UpdateOperation(item);

				if(item.AmountInDB == 0 && item.AmountInFact == 0)
				{
					itemsToDelete.Add(item);
				}
			}

			foreach(var item in itemsToDelete)
			{
				uow.Delete(item);
				NomenclatureItems.Remove(item);
			}
		}

		public virtual void SortItems(bool byName = false)
		{
			var sortedNomenclatureItems = NomenclatureItems.ToList();
			var sortedInstanceItems = InstanceItems.ToList();
			
			if(!byName)
			{
				sortedNomenclatureItems.Sort((x,y) => x.Nomenclature.Id.CompareTo(y.Nomenclature.Id));
				sortedInstanceItems.Sort((x, y) => x.InventoryNomenclatureInstance.Id.CompareTo(y.InventoryNomenclatureInstance.Id));
			}
			else
			{
				sortedNomenclatureItems.Sort((x, y) => x.Nomenclature.Name.CompareTo(y.Nomenclature.Name));
				sortedInstanceItems.Sort((x, y) => x.Name.CompareTo(y.Name));
			}
			
			ObservableNomenclatureItems.Clear();
			ObservableInstanceItems.Clear();

			foreach(var nomenclatureItem in sortedNomenclatureItems)
			{
				ObservableNomenclatureItems.Add(nomenclatureItem);
			}
			
			foreach(var instanceItem in sortedInstanceItems)
			{
				ObservableInstanceItems.Add(instanceItem);
			}
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(NomenclatureItems.Count == 0 /*&&*/)
			{
				yield return new ValidationResult("Табличная часть документа пустая.",
					new[] {nameof(NomenclatureItems)});
			}

			if(TimeStamp == default(DateTime))
			{
				yield return new ValidationResult("Дата документа должна быть указана.",
					new[] {nameof(TimeStamp)});
			}

			foreach(var item in NomenclatureItems)
			{
				foreach(var result in item.Validate(new ValidationContext(item)))
				{
					yield return result;
				}
			}

			var needWeightOrVolume = NomenclatureItems
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
					new[] {nameof(NomenclatureItems)});
			}
		}
		
		private void FillBulkItem(InventoryDocumentItem item, Nomenclature nomenclature, decimal amountInDb, decimal amountInFact)
		{
			item.Nomenclature = nomenclature;
			item.AmountInDB = amountInDb;
			item.AmountInFact = amountInFact;
			item.Document = this;
		}

		private InventoryDocumentItem CreateNewNomenclatureItem()
		{
			switch(InventoryDocumentType)
			{
				case InventoryDocumentType.WarehouseInventory:
					return new WarehouseBulkInventoryDocumentItem();
				case InventoryDocumentType.EmployeeInventory:
					return new EmployeeBulkInventoryDocumentItem();
				case InventoryDocumentType.CarInventory:
					return new CarBulkInventoryDocumentItem();
				default:
					throw new InvalidOperationException("Неизвестный тип документа");
			}
		}
		
		private void UpdateOperation(InventoryDocumentItem item)
		{
			if(item.Difference != 0)
			{
				item.UpdateOperation(TimeStamp);
			}
		}
	}

	public enum InventoryDocumentStatus
	{
		[Display(Name = "В работе")]
		InProcess,
		[Display(Name = "Подтвержден")]
		Confirmed
	}

	public enum InventoryDocumentType
	{
		[Display(Name = "Для склада")]
		WarehouseInventory,
		[Display(Name = "Для сотрудника")]
		EmployeeInventory,
		[Display(Name = "Для автомобиля")]
		CarInventory
	}
}

