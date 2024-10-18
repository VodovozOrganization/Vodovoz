using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров строк талонов погрузки",
			Nominative = "код ЧЗ товара строки талона погрузки")]
	public class CarLoadDocumentItemTrueMarkProductCode : TrueMarkProductCode
	{
		private CarLoadDocumentItemEntity _carLoadDocumentItem;

		[Display(Name = "Строка талона погрузки")]
		public virtual CarLoadDocumentItemEntity CarLoadDocumentItem
		{
			get => _carLoadDocumentItem;
			set => SetField(ref _carLoadDocumentItem, value);
		}
	}
}
