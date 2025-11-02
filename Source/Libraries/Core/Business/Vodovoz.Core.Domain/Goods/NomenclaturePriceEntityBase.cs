using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "цены",
		Nominative = "цена")]
	[HistoryTrace]
	public class NomenclaturePriceEntityBase : NomenclaturePriceGeneralBase
	{
		private NomenclatureEntity _nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
	}
}

