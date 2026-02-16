using System.ComponentModel.DataAnnotations;
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
	public class GtinEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _gtinNumber;
		private NomenclatureEntity _nomenclature;
		private uint _priority;

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

		public virtual uint Priority
		{
			get => _priority;
			set => SetField(ref _priority, value);
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
