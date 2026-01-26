using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Documents;

namespace VodovozBusiness.Domain.Documents
{
	/// <summary>
	/// Строка документа самовывоза
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentItem : SelfDeliveryDocumentItemEntity
	{
		private SelfDeliveryDocument _document;

		/// <summary>
		/// Документ самовывоза, к которому относится строка
		/// </summary>
		[Display(Name = "Документ самовывоза")]
		public virtual new SelfDeliveryDocument Document
		{
			get => _document;
			set => SetField(ref _document, value);
		}
	}
}
