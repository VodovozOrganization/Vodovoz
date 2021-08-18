using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "приоритет района",
		NominativePlural = "приоритеты районов")]
	public class DriverDistrictPriority : PropertyChangedBase, IDomainObject, IDistrictPriority, ICloneable
	{
		#region Свойства

		public virtual int Id { get; set; }

		private Sector _sector;
		[Display(Name = "Район")]
		public virtual Sector Sector {
			get => _sector;
			set => SetField(ref _sector, value);
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
				Sector = this.Sector,
				Priority = this.Priority
			};
			return result;
		}

		#endregion

		public virtual object Clone()
		{
			return new DriverDistrictPriority {
				Sector = Sector,
				Priority = Priority,
				DriverDistrictPrioritySet = DriverDistrictPrioritySet
			};
		}

		public override string ToString()
		{
			return Sector?.GetActiveSectorVersion().SectorName == null ? base.ToString() : $"({Priority}) {Sector.GetActiveSectorVersion().SectorName}";
		}
	}
}
