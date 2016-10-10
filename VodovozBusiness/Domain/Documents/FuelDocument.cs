using System;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	public class FuelDocument: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set {SetField(ref date, value, () => Date);}
		}

		private Employee driver;

		[Display (Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set {SetField(ref driver, value, () => Driver);}
		}

		private FuelOperation operation;

		[Display (Name = "Номер операции")]
		public virtual FuelOperation Operation {
			get { return operation; }
			set {SetField(ref operation, value, () => Operation);}
		}

		private decimal? payedForLiter;


		[Display (Name = "Топливо, оплаченное деньгами")]
		public virtual decimal? PayedForFuel {
			get { return payedForLiter; }
			set {SetField(ref payedForLiter, value, () => PayedForFuel);}
		}

		private decimal literCost;


		[Display (Name = "Стоимость литра топлива")]
		public virtual decimal LiterCost {
			get { return literCost; }
			set { SetField(ref literCost, value, () => LiterCost);}
		}

		private FuelType fuel;

		[Display (Name = "Вид топлива")]
		public virtual FuelType Fuel {
			get { return fuel; }
			set { SetField(ref fuel, value, () => Fuel);}
		}

		public FuelDocument()
		{
		}
	}
}

