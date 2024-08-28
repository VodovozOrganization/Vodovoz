using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Documents;

namespace VodovozBusiness.Domain.TrueMark.TrueMarkProductCodes
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров строк талонов погрузки",
			Nominative = "код ЧЗ товара строки талона погрузки")]
	public class CarLoadDocumentItemTrueMarkProductCode : TrueMarkProductCode
	{
		private CarLoadDocumentItem _carLoadDocumentItem;

		[Display(Name = "Строка талона погрузки")]
		public virtual CarLoadDocumentItem CarLoadDocumentItem
		{
			get => _carLoadDocumentItem;
			set => SetField(ref _carLoadDocumentItem, value);
		}
	}
}
