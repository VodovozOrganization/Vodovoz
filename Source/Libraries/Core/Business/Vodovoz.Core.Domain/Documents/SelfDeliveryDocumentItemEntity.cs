using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentItemEntity : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }
		private IObservableList<SelfDeliveryDocumentItemTrueMarkProductCode> _trueMarkProductCodes = new ObservableList<SelfDeliveryDocumentItemTrueMarkProductCode>();
		
		[Display(Name = "Коды ЧЗ товаров")]
		public virtual IObservableList<SelfDeliveryDocumentItemTrueMarkProductCode> TrueMarkProductCodes
		{
			get => _trueMarkProductCodes;
			set => SetField(ref _trueMarkProductCodes, value);
		}
	}
}
