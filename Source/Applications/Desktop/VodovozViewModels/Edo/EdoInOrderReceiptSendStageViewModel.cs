using QS.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderReceiptSendStageViewModel : WidgetViewModelBase
	{
		private EdoInOrderReceiptViewModel _selectedReceipt;

		public EdoInOrderReceiptSendStageViewModel(
			IEnumerable<EdoInOrderReceiptNode> receipts
			)
		{
			Receipts = receipts.Select(x => new EdoInOrderReceiptViewModel(x)).ToList();
			SelectedReceipt = Receipts.FirstOrDefault();
		}

		public IList<EdoInOrderReceiptViewModel> Receipts { get; }

		public virtual EdoInOrderReceiptViewModel SelectedReceipt
		{
			get => _selectedReceipt;
			set => SetField(ref _selectedReceipt, value);
		}
	}
}
