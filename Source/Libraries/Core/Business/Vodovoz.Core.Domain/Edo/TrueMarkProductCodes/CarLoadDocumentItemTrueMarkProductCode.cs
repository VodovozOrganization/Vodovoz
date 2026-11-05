using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров строк талонов погрузки",
			Nominative = "код ЧЗ товара строки талона погрузки")]
	public class CarLoadDocumentItemTrueMarkProductCode : TrueMarkProductCode
	{
		private int? _carLoadDocumentItemId;

		[Display(Name = "Id строки талона погрузки")]
		public virtual int? CarLoadDocumentItemId
		{
			get => _carLoadDocumentItemId;
			set => SetField(ref _carLoadDocumentItemId, value);
		}
	}
}
