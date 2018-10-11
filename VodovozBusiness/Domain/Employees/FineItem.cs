using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Employees
{
	[OrmSubject (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки штрафа",
		Nominative = "строка штрафа")]
	public class FineItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual Fine Fine { get; set; }

		Employee employee;

		[Display (Name = "Сотрудник")]
		public virtual Employee Employee {
			get { return employee; }
			set {
				SetField (ref employee, value, () => Employee);
			}
		}

		decimal money;

		[Display (Name = "Деньги")]
		public virtual decimal Money {
			get { return money; }
			set {
				SetField (ref money, value, () => Money);
			}
		}

		decimal litersOverspending;

		[Display(Name = "Перерасходовано литров")]
		public virtual decimal LitersOverspending {
			get { return litersOverspending; }
			set { SetField(ref litersOverspending, value, () => LitersOverspending); }
		}

		private FuelOperation fuelOutlayedOperation;

		[Display(Name = "Операции расхода топлива")]
		public virtual FuelOperation FuelOutlayedOperation {
			get { return fuelOutlayedOperation; }
			set { SetField(ref fuelOutlayedOperation, value, () => FuelOutlayedOperation); }
		}


		private WagesMovementOperations wageOperation;

		[Display(Name = "Операция по удержанию штрафа")]
		public virtual WagesMovementOperations WageOperation
		{
			get { return wageOperation; }
			set { SetField(ref wageOperation, value, () => WageOperation); }
		}

		public virtual string Title {
			get{
				return String.Format("{0} - {1}", 
					Employee.ShortName, 
					CurrencyWorks.GetShortCurrencyString(Money));
			}
		}

	}

}

