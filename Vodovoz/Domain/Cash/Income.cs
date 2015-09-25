using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "приходные одера",
		Nominative = "приходный ордер")]
	public class Income : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		private IncomeType typeOperation;

		[Display (Name = "Тип операции")]
		public IncomeType TypeOperation {
			get { return typeOperation; }
			set { SetField (ref typeOperation, value, () => TypeOperation); }
		}

		Employee casher;

		[Display (Name = "Кассир")]
		public virtual Employee Casher {
			get { return casher; }
			set { SetField (ref casher, value, () => Casher); }
		}

		Employee employee;

		[Display (Name = "Сотрудник")]
		public virtual Employee Employee {
			get { return employee; }
			set { SetField (ref employee, value, () => Employee); }
		}

		IncomeCategory incomeCategory;

		[Display (Name = "Статья дохода")]
		public virtual IncomeCategory IncomeCategory {
			get { return incomeCategory; }
			set { SetField (ref incomeCategory, value, () => IncomeCategory); }
		}

		string description;

		[Display (Name = "Основание")]
		public virtual string Description {
			get { return description; }
			set { SetField (ref description, value, () => Description); }
		}


		decimal money;

		[Display (Name = "Сумма")]
		public virtual decimal Money {
			get { return money; }
			set {
				SetField (ref money, value, () => Money); 
			}
		}

		public virtual string Title { 
			get { return String.Format ("Приходный ордер №{0} от {1:d}", Id, Date); }
		}
			
		#endregion

		public Income ()
		{
		}
	}

	public enum IncomeType
	{
		[Display (Name = "Прочий приход")]
		Common,
		[Display (Name = "Приход от водителя")]
		DriverReport,
	}

	public class IncomeTypeStringType : NHibernate.Type.EnumStringType
	{
		public IncomeTypeStringType () : base (typeof(IncomeType))
		{
		}
	}

}

