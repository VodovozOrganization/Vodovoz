using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace VodovozBusiness.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Групповые Gtin",
		Nominative = "Групповой Gtin")]
	[EntityPermission]
	[HistoryTrace]

	public class GroupGtin : GroupGtinEntity
	{
		private Nomenclature _nomenclature;

		public virtual new Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
	}
}
