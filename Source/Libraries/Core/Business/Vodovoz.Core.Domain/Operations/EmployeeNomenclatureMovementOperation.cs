using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class EmployeeNomenclatureMovementOperation : OperationBase
	{
		private EmployeeEntity _employee;
		private NomenclatureEntity _nomenclature;
		private decimal _amount;

		/// <summary>
		/// Сотрудник
		/// </summary>
		[Display(Name = "Сотрудник")]
		public virtual EmployeeEntity Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}
	}
}
