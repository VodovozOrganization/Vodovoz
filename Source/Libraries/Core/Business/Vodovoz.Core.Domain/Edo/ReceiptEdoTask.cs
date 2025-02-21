using QS.Extensions.Observable.Collections.List;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class ReceiptEdoTask : OrderEdoTask
	{
		private EdoReceiptStatus _receiptStatus;
		private int? _сashboxId;
		private IObservableList<EdoFiscalDocument> _fiscalDocuments = new ObservableList<EdoFiscalDocument>();

		[Display(Name = "Статус")]
		public virtual EdoReceiptStatus ReceiptStatus
		{
			get => _receiptStatus;
			set => SetField(ref _receiptStatus, value);
		}

		[Display(Name = "Код кассы")]
		public virtual int? CashboxId
		{
			get => _сashboxId;
			set => SetField(ref _сashboxId, value);
		}

		[Display(Name = "Фискальые документы")]
		public virtual IObservableList<EdoFiscalDocument> FiscalDocuments
		{
			get => _fiscalDocuments;
			set => SetField(ref _fiscalDocuments, value);
		}

		public override EdoTaskType TaskType => EdoTaskType.Receipt;
	}
}
