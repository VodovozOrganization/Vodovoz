﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
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
		//private Counterparty _client;
		//private DeliveryPoint _deliveryPoint;
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
				/*foreach(var item in Items)
				{
					if(item.WarehouseWriteoffOperation != null && item.WarehouseWriteoffOperation.OperationTime != TimeStamp)
					{
						item.WarehouseWriteoffOperation.OperationTime = TimeStamp;
					}
				}*/
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

		/*[Display (Name = "Клиент списания")]
		public virtual Counterparty Client
		{
			get => _client;
			set
			{
				_client = value;
				if(Client != null)
				{
					WriteOffFromWarehouse = null;
				}

				if(Client == null || !Client.DeliveryPoints.Contains(DeliveryPoint))
				{
					DeliveryPoint = null;
				}
			}
		}*/

		/*[Display (Name = "Точка доставки списания")]
		public virtual DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => _deliveryPoint = value;
		}*/

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
			var bulkWriteOffItem = new BulkWriteOffDocumentItem();
			FillBulkItem(nomenclature, amount, inStock, bulkWriteOffItem);

			switch(WriteOffType)
			{
				case WriteOffType.Warehouse:
					if(WriteOffFromWarehouse != null)
					{
						bulkWriteOffItem.CreateWarehouseBulkOperation(WriteOffFromWarehouse, TimeStamp);
					}
					break;
				case WriteOffType.Employee:
					if(WriteOffFromEmployee != null)
					{
						bulkWriteOffItem.CreateEmployeeBulkOperation(WriteOffFromEmployee, TimeStamp);
					}
					break;
				case WriteOffType.Car:
					if(WriteOffFromCar != null)
					{
						bulkWriteOffItem.CreateCarBulkOperation(WriteOffFromCar, TimeStamp);
					}
					break;
			}
			ObservableItems.Add(bulkWriteOffItem);
		}
		
		public virtual void AddItem(InventoryNomenclatureInstance nomenclatureInstance, decimal amount, decimal inStock)
		{
			var instanceWriteOffItem = new InstanceWriteOffDocumentItem();
			FillInstanceItem(nomenclatureInstance, amount, inStock, instanceWriteOffItem);

			switch(WriteOffType)
			{
				case WriteOffType.Warehouse:
					if(WriteOffFromWarehouse != null)
					{
						instanceWriteOffItem.CreateWarehouseInstanceOperation(WriteOffFromWarehouse, TimeStamp);
					}
					break;
				case WriteOffType.Employee:
					if(WriteOffFromEmployee != null)
					{
						instanceWriteOffItem.CreateEmployeeInstanceOperation(WriteOffFromEmployee, TimeStamp);
					}
					break;
				case WriteOffType.Car:
					if(WriteOffFromCar != null)
					{
						instanceWriteOffItem.CreateCarInstanceOperation(WriteOffFromCar, TimeStamp);
					}
					break;
			}
			ObservableItems.Add(instanceWriteOffItem);
		}

		public virtual void DeleteItem(WriteOffDocumentItem item)
		{
			if(ObservableItems.Contains(item))
			{
				ObservableItems.Remove(item);
			}
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			/*if(WriteOffFromWarehouse == null && Client == null)
			{
				yield return new ValidationResult ("Склад списания или контрагент должны быть заполнены.");
			}

			if(Client != null && DeliveryPoint == null)
			{
				yield return new ValidationResult ("Точка доставки должна быть указана.");
			}*/

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

	public enum WriteOffType
	{
		[Display(Name = "Со склада")]
		Warehouse,
		[Display(Name = "От клиента")]
		Counterparty,
		[Display(Name = "С сотрудника")]
		Employee,
		[Display(Name = "С автомобиля")]
		Car
	}
}

