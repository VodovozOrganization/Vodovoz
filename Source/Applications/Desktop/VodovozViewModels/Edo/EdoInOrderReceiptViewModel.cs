using QS.ViewModels;
using Vodovoz.Core.Data.Repositories;
using Gamma.Utilities;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderReceiptViewModel : WidgetViewModelBase
	{
		public EdoInOrderReceiptViewModel(EdoInOrderReceiptNode receiptNode)
		{
			ReceiptNode = receiptNode ?? throw new System.ArgumentNullException(nameof(receiptNode));
			
			DocNumber = receiptNode.DocumentNumber;
			DocGuid = receiptNode.DocumentGuid.ToString();
			DocType = receiptNode.DocumentType.GetEnumTitle();
			DocTime = receiptNode.CreationTime.ToString("dd.MM.yyyy HH:mm");
			DocStatus = receiptNode.DocumentStatus.GetEnumTitle();
			DocIndex = (receiptNode.Index + 1).ToString();
			Contact = receiptNode.Contact;
			FiscalNumber = receiptNode.FiscalNumber;
			FiscalMark = receiptNode.FiscalMark;
			FiscalKktNumber = receiptNode.FiscalKktNumber;
			FiscalTime = receiptNode.FiscalTime?.ToString("dd.MM.yyyy HH:mm");
			ResaleInn = receiptNode.ClientInn;
			Cashier = receiptNode.Cashier;
			Sum = receiptNode.Sum.ToString("F2");
			CashError = receiptNode.FailureMessage;
			HasCashError = !string.IsNullOrWhiteSpace(receiptNode.FailureMessage);
		}

		public EdoInOrderReceiptNode ReceiptNode { get; }

		public string DocNumber { get; }
		public string DocGuid { get; }
		public string DocType { get; }
		public string DocTime { get; }
		public string DocStatus { get; }
		public string DocIndex { get; }
		public string Contact { get; }
		public string FiscalNumber { get; }
		public string FiscalMark { get; }
		public string FiscalKktNumber { get; }
		public string FiscalTime { get; }
		public string ResaleInn { get; }
		public string Cashier { get; }
		public string Sum { get; }
		public string CashError { get; }
		public bool HasCashError { get; }
	}
}
