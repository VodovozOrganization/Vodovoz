using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "распределение номенклатур для быстрой доставки",
		NominativePlural = "распределение номенклатур для быстрой доставки")]
	[EntityPermission]
	[HistoryTrace]
	public class AdditionalLoadingNomenclatureDistribution : PropertyChangedBase, IDomainObject
	{
		private Nomenclature _nomenclature;
		private decimal _percent;

		public virtual int Id { get; set; }

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Процент")]
		public virtual decimal Percent
		{
			get => _percent;
			set => SetField(ref _percent, value);
		}
	}
}
