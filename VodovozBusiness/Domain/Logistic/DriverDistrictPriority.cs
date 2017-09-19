using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
	NominativePlural = "приоритеты районов",
	Nominative = "приоритет района")]
	public class DriverDistrictPriority : PropertyChangedBase, IDomainObject, IDistrictPriority
	{
		#region Свойства

		public virtual int Id { get; set; }

		private Employee driver;

		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get { return driver; }
			set { SetField(ref driver, value, () => Driver); }
		}

		private Logistic.LogisticsArea district;

		[Display(Name = "Район")]
		public virtual Logistic.LogisticsArea District
		{
			get { return district; }
			set { SetField(ref district, value, () => District); }
		}

		private int priority;

		[Display(Name = "Приоритет")]
		public virtual int Priority
		{
			get { return priority; }
			set { SetField(ref priority, value, () => Priority); }
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
	}
}
