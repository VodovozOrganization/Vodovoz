using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

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

		Sector _sector;
		[Display(Name = "Район")]
		public virtual Sector Sector {
			get => _sector;
			set => SetField(ref _sector, value, () => Sector);
		}

		public virtual SectorVersion SectorVersion => Sector.GetActiveSectorVersion();
		
		int priority;
		[Display(Name = "Приоритет")]
		public virtual int Priority {
			get => priority;
			set => SetField(ref priority, value, () => Priority);
		}

		#endregion
	}
}