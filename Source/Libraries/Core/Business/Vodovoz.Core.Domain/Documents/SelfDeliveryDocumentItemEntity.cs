using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Строка документа самовывоза
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentItemEntity : PropertyChangedBase, IDomainObject
	{
		private IObservableList<SelfDeliveryDocumentItemTrueMarkProductCode> _trueMarkProductCodes = new ObservableList<SelfDeliveryDocumentItemTrueMarkProductCode>();
		private SelfDeliveryDocumentEntity _selfDeliveryDocument;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Документ самовывоза, к которому относится строка
		/// </summary>
		[Display(Name = "Документ самовывоза")]
		public virtual SelfDeliveryDocumentEntity SelfDeliveryDocument
		{
			get => _selfDeliveryDocument;
			set => SetField(ref _selfDeliveryDocument, value);
		}

		/// <summary>
		/// Коды ЧЗ товаров, которые были отсканированы в строке документа самовывоза
		/// </summary>
		[Display(Name = "Коды ЧЗ товаров")]
		public virtual IObservableList<SelfDeliveryDocumentItemTrueMarkProductCode> TrueMarkProductCodes
		{
			get => _trueMarkProductCodes;
			set => SetField(ref _trueMarkProductCodes, value);
		}
	}
}
