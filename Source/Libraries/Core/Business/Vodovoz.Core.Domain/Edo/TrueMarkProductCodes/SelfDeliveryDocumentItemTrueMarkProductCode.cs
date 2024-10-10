using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров строк документов самовывоза",
			Nominative = "код ЧЗ товара строки документа самовывоза")]
	public class SelfDeliveryDocumentItemTrueMarkProductCode : TrueMarkProductCode
	{
		private SelfDeliveryDocumentItemEntity _selfDeliveryDocumentItem;

		[Display(Name = "Строка документа самовывоза")]
		public virtual SelfDeliveryDocumentItemEntity SelfDeliveryDocumentItem
		{
			get => _selfDeliveryDocumentItem;
			set => SetField(ref _selfDeliveryDocumentItem, value);
		}
	}
}
