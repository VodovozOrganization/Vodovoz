using System;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "операции с топливом",
		Nominative = "операция с топливом")]
	public partial class FuelOperation : OperationBase
	{
		private FuelType fuel;

		[Display (Name = "Тип топлива")]
		public virtual FuelType Fuel {
			get { return fuel; }
			set {SetField(ref fuel, value, () => Fuel);}
		}

		private Employee driver;

		[Display (Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField(ref driver, value, () => Driver); }
		}

		private decimal litersGived;

		[Display (Name = "Выдано литров топлива")]
		public virtual DateTime LitersGived {
			get { return litersGived; }
			set {SetField(ref litersGived, value, () => LitersGived);}
		}

		private decimal litersOutlayed;

		[Display (Name = "Потрачено литров топлива")]
		public virtual DateTime LitersOutlayed {
			get { return litersOutlayed; }
			set {SetField(ref litersOutlayed, value, () => LitersOutlayed);}
		}

		public FuelOperation()
		{
		}
	}
}

