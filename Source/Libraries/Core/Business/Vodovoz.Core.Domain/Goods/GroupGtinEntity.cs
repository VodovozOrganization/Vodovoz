using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Групповые Gtin",
		Nominative = "Групповой Gtin")]
	[EntityPermission]
	[HistoryTrace]
	public class GroupGtinEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _gtinNumber;
		private NomenclatureEntity _nomenclature;
		private int _codesCount;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

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

		public virtual int CodesCount
		{
			get => _codesCount;
			set => SetField(ref _codesCount, value);
		}

		public override string ToString()
		{
			return $"Gtin №{Id}: {GtinNumber}";
		}
	}
}
