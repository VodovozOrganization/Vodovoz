using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Gtin",
		Nominative = "Gtin")]
	[EntityPermission]
	[HistoryTrace]

	public class Gtin : PropertyChangedBase, IDomainObject
	{
		private string _gtinNumber;
		private NomenclatureEntity _nomenclature;
		public virtual int Id { get; set; }

		public virtual string GtinNumber
		{
			get => _gtinNumber;
			set => SetField(ref _gtinNumber, value);
		}

		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		public override string ToString()
		{
			return $"Gtin №{Id}: {GtinNumber}";
		}
	}
}
