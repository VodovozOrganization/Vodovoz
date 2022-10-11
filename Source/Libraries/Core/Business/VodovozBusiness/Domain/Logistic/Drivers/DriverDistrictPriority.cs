using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "приоритет района",
		NominativePlural = "приоритеты районов")]
	public class DriverDistrictPriority : PropertyChangedBase, IDomainObject, IDistrictPriority, ICloneable
	{
		#region Свойства

		public virtual int Id { get; set; }

		private District district;
		[Display(Name = "Район")]
		public virtual District District {
			get => district;
			set => SetField(ref district, value);
		}

		//FIXME Удалить после обновления
		private Employee driver;
		[Display(Name = "Водитель")]
		public virtual Employee Driver {
			get => driver;
			set => SetField(ref driver, value);
		}

		private int priority;
		[Display(Name = "Приоритет")]
		public virtual int Priority {
			get => priority;
			set => SetField(ref priority, value);
		}

		private DriverDistrictPrioritySet driverDistrictPrioritySet;
		[Display(Name = "Версия приоритетов районов водителя")]
		public virtual DriverDistrictPrioritySet DriverDistrictPrioritySet {
			get => driverDistrictPrioritySet;
			set => SetField(ref driverDistrictPrioritySet, value);
		}

		#endregion

		#region Функции

		public virtual AtWorkDriverDistrictPriority CreateAtDay(AtWorkDriver atDayDriver)
		{
			var result = new AtWorkDriverDistrictPriority
			{
				Driver = atDayDriver,
				District = this.District,
				Priority = this.Priority
			};
			return result;
		}

		#endregion

		public virtual object Clone()
		{
			return new DriverDistrictPriority {
				District = District,
				Priority = Priority,
				DriverDistrictPrioritySet = DriverDistrictPrioritySet
			};
		}

		public override string ToString()
		{
			return District?.DistrictName == null ? base.ToString() : $"({Priority}) {District.DistrictName}";
		}
	}
}
