using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
	NominativePlural = "приоритеты районов",
	Nominative = "приоритет района")]
	public class AtWorkDriverDistrictPriority : PropertyChangedBase, IDomainObject, IDistrictPriority
	{
		#region Свойства

		public virtual int Id { get; set; }

		AtWorkDriver driver;
		[Display(Name = "Водитель")]
		public virtual AtWorkDriver Driver {
			get => driver;
			set => SetField(ref driver, value, () => Driver);
		}

		LogisticsArea district;
		[Display(Name = "Район")]
		public virtual LogisticsArea District {
			get => district;
			set => SetField(ref district, value, () => District);
		}

		int priority;
		[Display(Name = "Приоритет")]
		public virtual int Priority {
			get => priority;
			set => SetField(ref priority, value, () => Priority);
		}

		#endregion
	}
}