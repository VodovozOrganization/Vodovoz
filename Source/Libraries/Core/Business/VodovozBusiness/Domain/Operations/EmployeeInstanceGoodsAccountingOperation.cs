using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции передвижения товаров по сотруднику(экземплярный учет)",
		Nominative = "операция передвижения товаров по сотруднику(экземплярный учет)")]
	public class EmployeeInstanceGoodsAccountingOperation : InstanceGoodsAccountingOperation
	{
		private Employee _employee;
		
		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		public override OperationType OperationType => OperationType.EmployeeInstanceGoodsAccountingOperation;
	}
}

