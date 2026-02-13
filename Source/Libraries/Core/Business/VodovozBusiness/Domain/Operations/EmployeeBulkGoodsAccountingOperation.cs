using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции передвижения товаров по сотруднику(объемный учет)",
		Nominative = "операция передвижения товаров по сотруднику(объемный учет)")]
	public class EmployeeBulkGoodsAccountingOperation : BulkGoodsAccountingOperation
	{
		private Employee _employee;
		
		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		public override OperationType OperationType => OperationType.EmployeeBulkGoodsAccountingOperation;
	}
}

