using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения зарплат",
		Nominative = "передвижение зарплат")]
	public class WagesMovementOperations : OperationBase
	{
		private Employee employee;

		[Display(Name = "Сотрудник")]
		public virtual Employee Employee
		{
			get { return employee; }
			set { SetField(ref employee, value, () => Employee); }
		}

		private WagesType operationType;

		[Display(Name = "Тип операции")]
		public virtual WagesType OperationType
		{
			get { return operationType; }
			set { SetField(ref operationType, value, () => OperationType); }
		}

		private decimal money;

		[Display(Name = "Сумма")]
		public virtual decimal Money
		{
			get { return money; }
			set { SetField(ref money, value, () => Money); }
		}

		public WagesMovementOperations()
		{
		}
	}

	public enum WagesType {
		[Display(Name = "Выдача зарплаты")]
		GivedWage,
		[Display(Name = "Выдача аванса")]
		GivedAdvance,
		[Display(Name = "Удержание штрафа")]
		HoldedFine,
		[Display(Name = "Начисление зарплаты")]
		AccrualWage,
		[Display(Name = "Начисление премии")]
		PremiumWage
	}

}
