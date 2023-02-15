using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
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

