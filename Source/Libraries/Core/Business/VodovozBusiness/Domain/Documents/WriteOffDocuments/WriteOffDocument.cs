using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.Documents.WriteOffDocuments
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "акты списания ТМЦ",
		Nominative = "акт списания ТМЦ",
		Prepositional = "акте списания"
	)]
	[EntityPermission]
	[HistoryTrace]
	public class WriteOffDocument : Document, IValidatableObject
	{
		private string _comment;
		private Employee _responsibleEmployee;
		private Warehouse _writeOffFromWarehouse;
		private Employee _writeOffFromEmployee;
		private Car _writeOffFromCar;
		private WriteOffType _writeOffType;
		private IList<WriteOffDocumentItem> _items = new List<WriteOffDocumentItem>();
		private GenericObservableList<WriteOffDocumentItem> _observableItems;

		public WriteOffDocument()
		{
			Comment = string.Empty;
			WriteOffType = WriteOffType.Warehouse;
		}
		
		public override DateTime TimeStamp
		{
			get => base.TimeStamp;
			set
			{
				base.TimeStamp = value;
				foreach(var item in Items)
				{
					if(item.GoodsAccountingOperation != null && item.GoodsAccountingOperation.OperationTime != TimeStamp)
					{
						item.GoodsAccountingOperation.OperationTime = TimeStamp;
					}
				}
			}
		}

		[Display (Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Required (ErrorMessage = "Должен быть указан ответственнй за списание.")]
		[Display (Name = "Ответственный")]
		public virtual Employee ResponsibleEmployee
		{
			get => _responsibleEmployee;
			set => SetField(ref _responsibleEmployee, value);
		}

		[Display (Name = "Склад списания")]
		public virtual Warehouse WriteOffFromWarehouse
		{
			get => _writeOffFromWarehouse;
			set
			{
				if(SetField(ref _writeOffFromWarehouse, value) && value != null)
				{
					WriteOffFromEmployee = null;
					WriteOffFromCar = null;
				}
			}
		}
		
		[Display (Name = "Сотрудник, с которого идет списание")]
		public virtual Employee WriteOffFromEmployee
		{
			get => _writeOffFromEmployee;
			set
			{
				if(SetField(ref _writeOffFromEmployee, value) && value != null)
				{
					WriteOffFromWarehouse = null;
					WriteOffFromCar = null;
				}
			}
		}
		
		[Display (Name = "Автомобиль, с которого идет списание")]
		public virtual Car WriteOffFromCar
		{
			get => _writeOffFromCar;
			set
			{
				if(SetField(ref _writeOffFromCar, value) && value != null)
				{
					WriteOffFromWarehouse = null;
					WriteOffFromEmployee = null;
				}
			}
		}

		[Display (Name = "Тип документа списания")]
		public virtual WriteOffType WriteOffType
		{
			get => _writeOffType;
			set => SetField(ref _writeOffType, value);
		}

		[Display (Name = "Строки")]
		public virtual IList<WriteOffDocumentItem> Items
		{
			get => _items;
			set
			{
				SetField(ref _items, value);
				_observableItems = null;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WriteOffDocumentItem> ObservableItems =>
			_observableItems ?? (_observableItems = new GenericObservableList<WriteOffDocumentItem>(Items));

		public virtual string Title => $"Акт списания №{Id} от {TimeStamp:d}";

		public virtual void AddItem(Nomenclature nomenclature, decimal amount, decimal inStock)
		{
			switch(WriteOffType)
			{
				case WriteOffType.Warehouse:
					if(WriteOffFromWarehouse != null)
					{
						var warehouseBulkItem = new BulkWriteOffFromWarehouseDocumentItem();
						FillBulkItem(nomenclature, amount, inStock, warehouseBulkItem);
						warehouseBulkItem.CreateOperation(WriteOffFromWarehouse, TimeStamp);
						ObservableItems.Add(warehouseBulkItem);
					}
					break;
				case WriteOffType.Employee:
					if(WriteOffFromEmployee != null)
					{
						var employeeBulkItem = new BulkWriteOffFromEmployeeDocumentItem();
						FillBulkItem(nomenclature, amount, inStock, employeeBulkItem);
						employeeBulkItem.CreateOperation(WriteOffFromEmployee, TimeStamp);
						ObservableItems.Add(employeeBulkItem);
					}
					break;
				case WriteOffType.Car:
					if(WriteOffFromCar != null)
					{
						var carBulkItem = new BulkWriteOffFromCarDocumentItem();
						FillBulkItem(nomenclature, amount, inStock, carBulkItem);
						carBulkItem.CreateOperation(WriteOffFromCar, TimeStamp);
						ObservableItems.Add(carBulkItem);
					}
					break;
			}
		}
		
		public virtual void AddItem(InventoryNomenclatureInstance nomenclatureInstance, decimal amount, decimal inStock)
		{
			switch(WriteOffType)
			{
				case WriteOffType.Warehouse:
					if(WriteOffFromWarehouse != null)
					{
						var warehouseInstanceItem = new InstanceWriteOffFromWarehouseDocumentItem();
						FillInstanceItem(nomenclatureInstance, amount, inStock, warehouseInstanceItem);
						warehouseInstanceItem.CreateOperation(WriteOffFromWarehouse, TimeStamp);
						ObservableItems.Add(warehouseInstanceItem);
					}
					break;
				case WriteOffType.Employee:
					if(WriteOffFromEmployee != null)
					{
						var employeeInstanceItem = new InstanceWriteOffFromEmployeeDocumentItem();
						FillInstanceItem(nomenclatureInstance, amount, inStock, employeeInstanceItem);
						employeeInstanceItem.CreateOperation(WriteOffFromEmployee, TimeStamp);
						ObservableItems.Add(employeeInstanceItem);
					}
					break;
				case WriteOffType.Car:
					if(WriteOffFromCar != null)
					{
						var carInstanceItem = new InstanceWriteOffFromCarDocumentItem();
						FillInstanceItem(nomenclatureInstance, amount, inStock, carInstanceItem);
						carInstanceItem.CreateOperation(WriteOffFromCar, TimeStamp);
						ObservableItems.Add(carInstanceItem);
					}
					break;
			}
		}

		public virtual void DeleteItem(WriteOffDocumentItem item)
		{
			if(ObservableItems.Contains(item))
			{
				ObservableItems.Remove(item);
			}
		}

		public virtual decimal TotalSumOfDamage =>
			ObservableItems.Sum(x => x.SumOfDamage);

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			#region ValidateStorage

			if(WriteOffType == WriteOffType.Warehouse && WriteOffFromWarehouse is null)
			{
				yield return new ValidationResult("Склад списания должен быть указан");
			}
			
			if(WriteOffType == WriteOffType.Employee && WriteOffFromEmployee is null)
			{
				yield return new ValidationResult("Сотрудник с которого идет списание должен быть указан");
			}

			if(WriteOffType == WriteOffType.Car && WriteOffFromCar is null)
			{
				yield return new ValidationResult("Автомобиль с которого идет списание должен быть указан");
			}

			#endregion

			if(Items.Count == 0)
			{
				yield return new ValidationResult ("Табличная часть документа пустая.", new[] { nameof(Items) });
			}

			foreach(var item in Items)
			{
				if(item.Amount <= 0)
				{
					yield return new ValidationResult ($"Для номенклатуры <{item.Nomenclature.Name}> не указано количество.",
						new[] { nameof(Items) });
				}

				if(item.Amount > item.AmountOnStock)
				{
					yield return new ValidationResult ($"На складе недостаточное количество <{item.Nomenclature.Name}>",
						new[] { nameof(Items) });
				}

				if((item.Type == WriteOffDocumentItemType.InstanceWriteOffFromCarDocumentItem 
					|| item.Type == WriteOffDocumentItemType.BulkWriteOffFromCarDocumentItem) 
						&& item.CullingCategory is null)
				{
					yield return new ValidationResult($"Для номенклатуры <{item.Nomenclature.Name}> \nПоле \"Причина выбраковки\" не должно быть пустым", new[] { nameof(item.Type) });
				}
			}
		}

		private void FillInstanceItem(
			InventoryNomenclatureInstance nomenclatureInstance,
			decimal amount,
			decimal inStock,
			InstanceWriteOffDocumentItem instanceWriteOffDocumentItem)
		{
			instanceWriteOffDocumentItem.InventoryNomenclatureInstance = nomenclatureInstance;
			instanceWriteOffDocumentItem.Nomenclature = nomenclatureInstance.Nomenclature;
			instanceWriteOffDocumentItem.AmountOnStock = inStock;
			instanceWriteOffDocumentItem.Amount = amount;
			instanceWriteOffDocumentItem.Document = this;
		}
		
		private void FillBulkItem(
			Nomenclature nomenclature,
			decimal amount,
			decimal inStock,
			BulkWriteOffDocumentItem bulkWriteOffDocumentItem)
		{
			bulkWriteOffDocumentItem.Nomenclature = nomenclature;
			bulkWriteOffDocumentItem.AmountOnStock = inStock;
			bulkWriteOffDocumentItem.Amount = amount;
			bulkWriteOffDocumentItem.Document = this;
		}
	}
}

