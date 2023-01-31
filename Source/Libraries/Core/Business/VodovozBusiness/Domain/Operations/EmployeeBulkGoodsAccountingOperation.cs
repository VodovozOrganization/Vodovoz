using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class EmployeeBulkGoodsAccountingOperation : BulkGoodsAccountingOperation
	{
		private Employee _employee;
		
		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		public override OperationTypeByStorage OperationTypeByStorage => OperationTypeByStorage.Employee;
	}
}

