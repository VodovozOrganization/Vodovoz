using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace VodovozBusiness.Domain.Goods
{
	/// <summary>
	/// Универсальный код товара Gtin
	/// </summary>

	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Gtin",
		Nominative = "Gtin")]
	[EntityPermission]
	[HistoryTrace]
	public class Gtin : GtinEntity
	{
		private Nomenclature _nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
	}
}
