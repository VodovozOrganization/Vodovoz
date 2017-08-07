using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
	NominativePlural = "приоритеты районов",
	Nominative = "приоритет района")]
	public class AtWorkDriverDistrictPriority : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private AtWorkDriver driver;

		[Display(Name = "Водитель")]
		public virtual AtWorkDriver Driver {
			get { return driver; }
			set { SetField(ref driver, value, () => Driver); }
		}

		private Logistic.LogisticsArea district;

		[Display(Name = "Район")]
		public virtual Logistic.LogisticsArea District{
			get { return district; }
			set { SetField(ref district, value, () => District); }
		}

		private int priority;

		[Display(Name = "Приоритет")]
		public virtual int Priority {
			get { return priority; }
			set { SetField(ref priority, value, () => Priority); }
		}

		#endregion

	}
}
