using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Domain.Goods
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "цены",
		Nominative = "цена")]
	[HistoryTrace]
	public class NomenclaturePriceBase : NomenclaturePriceGeneralBase
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

