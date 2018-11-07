using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Operations;
namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки премии",
		Nominative = "строка премии")]
	public class PremiumItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual Premium Premium { get; set; }

		Employee employee;

		[Display(Name = "Сотрудник")]
		public virtual Employee Employee {
			get { return employee; }
			set {
				SetField(ref employee, value, () => Employee);
			}
		}

		decimal money;

		[Display(Name = "Деньги")]
		public virtual decimal Money {
			get { return money; }
			set {
				SetField(ref money, value, () => Money);
			}
		}

		private WagesMovementOperations wageOperation;

		[Display(Name = "Операция начисления премии")]
		public virtual WagesMovementOperations WageOperation {
			get { return wageOperation; }
			set { SetField(ref wageOperation, value, () => WageOperation); }
		}

		public virtual string Title {
			get {
				return String.Format("{0} - {1}",
					Employee.ShortName,
					CurrencyWorks.GetShortCurrencyString(Money));
			}
		}

	}
}
