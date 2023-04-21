using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания (объемный учет)",
		Nominative = "строка списания (объемный учет)")]
	[HistoryTrace]
	public class BulkWriteOffDocumentItem : WriteOffDocumentItem
	{
		public override AccountingType AccountingType => AccountingType.Bulk;
		
		public virtual void CreateEmployeeBulkOperation(Employee employee, DateTime time)
		{
			GoodsAccountingOperation = new EmployeeBulkGoodsAccountingOperation
			{
				Employee = employee,
			};
			
			FillOperation(time);
		}
		
		public virtual void CreateCarBulkOperation(Car car, DateTime time)
		{
			GoodsAccountingOperation = new CarBulkGoodsAccountingOperation
			{
				Car = car,
			};
			
			FillOperation(time);
		}
		
		public virtual void CreateWarehouseBulkOperation(Warehouse warehouse, DateTime time)
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Warehouse = warehouse
			};
			
			FillOperation(time);
		}
		
		private void FillOperation(DateTime time)
		{
			if(GoodsAccountingOperation is null)
			{
				throw new InvalidOperationException("Не создана операция списания!");
			}

			GoodsAccountingOperation.OperationTime = time;
			FillOperation();
		}
	}
}

