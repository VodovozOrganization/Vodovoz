using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.WageCalculation
{
	[Appellative(Gender = GrammaticalGender.Masculine, 
		NominativePlural = "планы продаж номенклатур", 
		Nominative = "план продаж номенклатуры")]
	[HistoryTrace]
	[EntityPermission]
	public class NomenclatureSalesPlanItem : SalesPlanItem
	{
		private Nomenclature _nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
	}
}
