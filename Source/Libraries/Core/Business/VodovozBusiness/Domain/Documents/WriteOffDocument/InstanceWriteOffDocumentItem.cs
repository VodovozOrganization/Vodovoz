using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания (экземплярный учет)",
		Nominative = "строка списания (экземплярный учет)")]
	[HistoryTrace]
	public class InstanceWriteOffDocumentItem : WriteOffDocumentItem
	{
		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;

		public override AccountingType AccountingType => AccountingType.Instance;

		[Display(Name = "Экземпляр номенклатуры")]
		public virtual InventoryNomenclatureInstance InventoryNomenclatureInstance
		{
			get => _inventoryNomenclatureInstance;
			set
			{
				if(!SetField(ref _inventoryNomenclatureInstance, value))
				{
					return;
				}

				/*if(WarehouseWriteoffOperation != null)
				{
					if(WarehouseWriteoffOperation.InventoryNomenclatureInstance != value)
					{
						WarehouseWriteoffOperation.InventoryNomenclatureInstance = value;
					}
					if(WarehouseWriteoffOperation.Nomenclature != _nomenclature)
					{
						WarehouseWriteoffOperation.Nomenclature = _nomenclature;
					}
				}*/

				/*if(CounterpartyWriteoffOperation != null && CounterpartyWriteoffOperation.Nomenclature != _nomenclature)
				{
					CounterpartyWriteoffOperation.Nomenclature = _nomenclature;
				}*/
			}
		}
		
		protected virtual InstanceGoodsAccountingOperation InstanceGoodsAccountingOperation =>
			GoodsAccountingOperation as InstanceGoodsAccountingOperation;

		public override string InventoryNumber =>
			InventoryNomenclatureInstance?.Nomenclature != null && InventoryNomenclatureInstance.Nomenclature.HasInventoryAccounting
				? InventoryNomenclatureInstance.InventoryNumber
				: base.InventoryNumber;

		public override bool CanEditAmount => false;

		public virtual void CreateEmployeeInstanceOperation(Employee employee, DateTime time)
		{
			GoodsAccountingOperation = new EmployeeInstanceGoodsAccountingOperation
			{
				Employee = employee,
			};
			
			FillOperation(time);
		}
		
		public virtual void CreateCarInstanceOperation(Car car, DateTime time)
		{
			GoodsAccountingOperation = new CarInstanceGoodsAccountingOperation
			{
				Car = car,
			};
			
			FillOperation(time);
		}
		
		public virtual void CreateWarehouseInstanceOperation(Warehouse warehouse, DateTime time)
		{
			GoodsAccountingOperation = new WarehouseInstanceGoodsAccountingOperation
			{
				Warehouse = warehouse,
			};
			
			FillOperation(time);
		}
		
		private void FillOperation(DateTime time)
		{
			if(GoodsAccountingOperation is null)
			{
				throw new InvalidOperationException("Не создана операция списания!");
			}
			
			InstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			InstanceGoodsAccountingOperation.OperationTime = time;
			base.FillOperation();
		}
	}
}

