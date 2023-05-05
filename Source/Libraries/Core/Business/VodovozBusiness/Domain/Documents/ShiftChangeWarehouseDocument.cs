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

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "акты передачи остатков",
		Nominative = "акт передачи остатков",
		Prepositional = "акте передачи остатков")]
	[EntityPermission]
	[HistoryTrace]
	public class ShiftChangeWarehouseDocument : Document, IValidatableObject, IWarehouseBoundedDocument
	{
		private string _comment;
		private Employee _sender;
		private Employee _receiver;
		private Warehouse _warehouse;
		private Car _car;
		private ShiftChangeResidueDocumentType _shiftChangeResidueDocumentType;
		private IList<ShiftChangeWarehouseDocumentItem> _nomenclatureItems = new List<ShiftChangeWarehouseDocumentItem>();
		private GenericObservableList<ShiftChangeWarehouseDocumentItem> _observableNomenclatureItems;
		private IList<InstanceShiftChangeWarehouseDocumentItem> _instanceItems = new List<InstanceShiftChangeWarehouseDocumentItem>();
		private GenericObservableList<InstanceShiftChangeWarehouseDocumentItem> _observableInstanceItems;

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}
		
		[Display(Name = "Передающий остатки")]
		public virtual Employee Sender
		{
			get => _sender;
			set => SetField(ref _sender, value);
		}
		
		[Display(Name = "Принимающий остатки")]
		public virtual Employee Receiver
		{
			get => _receiver;
			set => SetField(ref _receiver, value);
		}

		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set
			{
				if(SetField(ref _warehouse, value) && value != null)
				{
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
				}
			}
		}

		[Display(Name = "Тип передачи остатков")]
		public virtual ShiftChangeResidueDocumentType ShiftChangeResidueDocumentType
		{
			get => _shiftChangeResidueDocumentType;
			set => SetField(ref _shiftChangeResidueDocumentType, value);
		}

		[Display(Name = "Строки объемного учета")]
		public virtual IList<ShiftChangeWarehouseDocumentItem> NomenclatureItems
		{
			get => _nomenclatureItems;
			set
			{
				SetField(ref _nomenclatureItems, value);
				_observableNomenclatureItems = null;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ShiftChangeWarehouseDocumentItem> ObservableNomenclatureItems => 
			_observableNomenclatureItems ??
			(_observableNomenclatureItems = new GenericObservableList<ShiftChangeWarehouseDocumentItem>(NomenclatureItems));
		
		[Display(Name = "Строки экземплярного учета")]
		public virtual IList<InstanceShiftChangeWarehouseDocumentItem> InstanceItems
		{
			get => _instanceItems;
			set
			{
				SetField(ref _instanceItems, value);
				_observableInstanceItems = null;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<InstanceShiftChangeWarehouseDocumentItem> ObservableInstanceItems => 
			_observableInstanceItems ??
			(_observableInstanceItems = new GenericObservableList<InstanceShiftChangeWarehouseDocumentItem>(InstanceItems));

		public virtual string Title => $"Акт передачи склада №{Id} от {TimeStamp:d}";

		#region Функции

		public virtual void AddItem(Nomenclature nomenclature, decimal amountInDb, decimal amountInFact)
		{
			var item = new ShiftChangeWarehouseDocumentItem
			{
				Nomenclature = nomenclature,
				AmountInDB = amountInDb,
				AmountInFact = amountInFact,
				Document = this
			};
			ObservableNomenclatureItems.Add(item);
		}
		
		public virtual void AddInstanceItem(InventoryNomenclatureInstance instance, bool canChangeIsMissing = true)
		{
			var instanceItem = new InstanceShiftChangeWarehouseDocumentItem
			{
				Document = this,
				InventoryNomenclatureInstance = instance,
				IsMissing = true,
				CanChangeIsMissing = canChangeIsMissing
			};
			ObservableInstanceItems.Add(instanceItem);
		}

		public virtual void FillItemsFromStock(
			IUnitOfWork uow, IStockRepository stockRepository, IList<NomenclatureCategory> categories = null)
		{
			var inStock = new Dictionary<int, decimal>();

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
			{
				return;
			}

			var nomenclatures = uow.GetById<Nomenclature>(inStock.Select(p => p.Key).ToArray());

			ObservableNomenclatureItems.Clear();
			foreach(var itemInStock in inStock)
			{
				ObservableNomenclatureItems.Add(
					new ShiftChangeWarehouseDocumentItem
					{
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
			var inStock = new Dictionary<int, decimal>();

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

			foreach(var itemInStock in inStock)
			{
				var item = NomenclatureItems.FirstOrDefault(x => x.Nomenclature.Id == itemInStock.Key);
				if(item != null)
				{
					item.AmountInDB = itemInStock.Value;
				}
				else
				{
					ObservableNomenclatureItems.Add(
						new ShiftChangeWarehouseDocumentItem
						{
							Nomenclature = uow.GetById<Nomenclature>(itemInStock.Key),
							AmountInDB = itemInStock.Value,
							AmountInFact = 0,
							Document = this
						});
				}
			}
			var itemsToRemove = new List<ShiftChangeWarehouseDocumentItem>();

			foreach(var item in NomenclatureItems)
			{
				if(!inStock.ContainsKey(item.Nomenclature.Id))
				{
					itemsToRemove.Add(item);
				}
			}

			foreach(var item in itemsToRemove)
			{
				ObservableNomenclatureItems.Remove(item);
			}
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
			Dictionary<int, decimal> inStock;

			if((ShiftChangeResidueDocumentType == ShiftChangeResidueDocumentType.Warehouse && Warehouse == null)
			   || (ShiftChangeResidueDocumentType == ShiftChangeResidueDocumentType.Car && Car == null))
			{
				return;
			}

			switch(ShiftChangeResidueDocumentType)
			{
				case ShiftChangeResidueDocumentType.Warehouse:
					inStock = stockRepository.NomenclatureInStock(
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
				case ShiftChangeResidueDocumentType.Car:
					inStock = stockRepository.NomenclatureInStock(
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

			if(inStock.Count == 0)
			{
				return;
			}

			var nomenclatures = uow.GetById<Nomenclature>(inStock.Select(p => p.Key).ToArray());

			ObservableNomenclatureItems.Clear();
			foreach(var itemInStock in inStock)
			{
				ObservableNomenclatureItems.Add(
					new ShiftChangeWarehouseDocumentItem
					{
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
			IEnumerable<int> nomenclaturesToInclude,
			IEnumerable<int> nomenclaturesToExclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToInclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToExclude,
			IEnumerable<int> productGroupToInclude,
			IEnumerable<int> productGroupToExclude)
		{
			Dictionary<int, decimal> inStock;

			if((ShiftChangeResidueDocumentType == ShiftChangeResidueDocumentType.Warehouse && Warehouse == null)
			   || (ShiftChangeResidueDocumentType == ShiftChangeResidueDocumentType.Car && Car == null))
			{
				return;
			}
			
			switch(ShiftChangeResidueDocumentType)
			{
				case ShiftChangeResidueDocumentType.Warehouse:
					inStock = stockRepository.NomenclatureInStock(
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
				case ShiftChangeResidueDocumentType.Car:
					inStock = stockRepository.NomenclatureInStock(
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

			foreach(var itemInStock in inStock)
			{
				var item = NomenclatureItems.FirstOrDefault(x => x.Nomenclature.Id == itemInStock.Key);
				if(item != null)
				{
					item.AmountInDB = itemInStock.Value;
				}
				else {
					ObservableNomenclatureItems.Add(
						new ShiftChangeWarehouseDocumentItem
						{
							Nomenclature = uow.GetById<Nomenclature>(itemInStock.Key),
							AmountInDB = itemInStock.Value,
							AmountInFact = 0,
							Document = this
						});
				}
			}
			var itemsToRemove = new List<ShiftChangeWarehouseDocumentItem>();

			foreach(var item in NomenclatureItems)
			{
				if(!inStock.ContainsKey(item.Nomenclature.Id))
				{
					itemsToRemove.Add(item);
				}
			}

			foreach(var item in itemsToRemove)
			{
				ObservableNomenclatureItems.Remove(item);
			}
		}

		#endregion

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(NomenclatureItems.Count == 0)
			{
				yield return new ValidationResult("Табличная часть документа объемного учета пустая.",
					new[] { nameof(NomenclatureItems) });
			}

			if(TimeStamp == default(DateTime))
			{
				yield return new ValidationResult("Дата документа должна быть указана.",
					new[] { nameof(TimeStamp) });
			}

			if(Sender is null)
			{
				yield return new ValidationResult("Нужно указать передающего остатки",
					new[] { nameof(Sender) });
			}

			if(Receiver is null)
			{
				yield return new ValidationResult("Нужно указать принимающего остатки",
					new[] { nameof(Receiver) });
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
					new[] { nameof(NomenclatureItems) });
			}
		}

		public ShiftChangeWarehouseDocument() { }
	}

	public enum ShiftChangeResidueDocumentType
	{
		[Display(Name = "По складу")]
		Warehouse,
		[Display(Name = "По автомобилю")]
		Car
	}
}
