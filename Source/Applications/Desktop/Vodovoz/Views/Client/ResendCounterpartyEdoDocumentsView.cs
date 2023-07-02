using QS.Views.GtkUI;
using Vodovoz.ViewModels.Dialogs.Counterparty;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ResendCounterpartyEdoDocumentsView : TabViewBase<ResendCounterpartyEdoDocumentsViewModel>
	{
		public ResendCounterpartyEdoDocumentsView(ResendCounterpartyEdoDocumentsViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
