using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;

namespace Vodovoz
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции с топливом",
		Nominative = "операция с топливом")]
	[HistoryTrace]
	public class FuelOperation : OperationBase
	{
		private FuelType fuel;

		[Display(Name = "Тип топлива")]
		public virtual FuelType Fuel
		{
			get { return fuel; }
			set { SetField(ref fuel, value, () => Fuel); }
		}

		private Employee driver;

		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get { return driver; }
			set { SetField(ref driver, value, () => Driver); }
		}

		private Car _car;

		[Display(Name = "Транспортное средство")]
		public virtual Car Car
		{
			get { return _car; }
			set { SetField(ref _car, value, () => Car); }
		}

		private decimal litersGived;

		[Display(Name = "Выдано литров топлива")]
		public virtual decimal LitersGived
		{
			get { return litersGived; }
			set { SetField(ref litersGived, value, () => LitersGived); }
		}

		private decimal payedLiters;
		[Display(Name = "Выдано литров топлива деньгами")]
		public virtual decimal PayedLiters
		{
			get => payedLiters;
			set => SetField(ref payedLiters, value, () => PayedLiters);
		}

		private decimal litersOutlayed;

		[Display(Name = "Потрачено литров топлива")]
		public virtual decimal LitersOutlayed
		{
			get { return litersOutlayed; }
			set { SetField(ref litersOutlayed, value, () => LitersOutlayed); }
		}

		private bool isFine;

		[Display(Name = "Операция со штрафом")]
		public virtual bool IsFine
		{
			get { return isFine; }
			set { SetField(ref isFine, value, () => IsFine); }
		}

		public FuelOperation() { }

		public virtual string Title => string.Format("{0} №{1}", this.GetType().GetSubjectName(), Id);
	}
}

