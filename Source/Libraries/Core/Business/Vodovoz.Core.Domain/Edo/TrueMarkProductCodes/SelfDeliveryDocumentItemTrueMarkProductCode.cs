using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров строк документов самовывоза",
			Nominative = "код ЧЗ товара строки документа самовывоза")]
	public class SelfDeliveryDocumentItemTrueMarkProductCode : TrueMarkProductCode
	{
		private int? _selfDeliveryDocumentItemId;

		[Display(Name = "Id строки документа самовывоза")]
		public virtual int? SelfDeliveryDocumentItemId
		{
			get => _selfDeliveryDocumentItemId;
			set => SetField(ref _selfDeliveryDocumentItemId, value);
		}
	}
}
