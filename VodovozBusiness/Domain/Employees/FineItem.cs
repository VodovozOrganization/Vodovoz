using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using QSProjectsLib;

namespace Vodovoz.Domain.Employees
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
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

		public virtual string Title {
			get{
				return String.Format("{0} - {1}", 
					Employee.ShortName, 
					CurrencyWorks.GetShortCurrencyString(Money));
			}
		}

	}

}

