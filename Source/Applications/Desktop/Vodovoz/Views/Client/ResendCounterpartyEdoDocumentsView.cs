using Gamma.ColumnConfig;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Models.CashReceipts;
using Vodovoz.ViewModels.Dialogs.Counterparty;
using static Vodovoz.ViewModels.Dialogs.Counterparty.ResendCounterpartyEdoDocumentsViewModel;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ResendCounterpartyEdoDocumentsView : TabViewBase<ResendCounterpartyEdoDocumentsViewModel>
	{
		public ResendCounterpartyEdoDocumentsView(ResendCounterpartyEdoDocumentsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		protected void ConfigureDlg()
		{
			if(ViewModel == null)
			{
				return;
			}

			ybuttonSendSelected.Clicked += (sender, args) => ViewModel.ResendSelectedEdoDocumentsCommand.Execute();
			ybuttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			ybuttonSelectAll.Clicked += (sender, args) => ViewModel.SelectAllCommand.Execute();
			ybuttonUnselectAll.Clicked += (sender, args) => ViewModel.UnselectAllCommand.Execute();
			ybuttonInvertSelected.Clicked += (sender, args) => ViewModel.InvertSelectionCommand.Execute();

			ytreeviewEdoDocuments.ColumnsConfig = FluentColumnsConfig<EdoContainerSelectableNode>.Create()
				.AddColumn(" Выбор ")
					.AddToggleRenderer(x => x.IsSelected)
					.Editing(true)
				.AddColumn(" Дата \n создания ")
					.AddTextRenderer(x => x.EdoContainer.Created.ToString("dd.MM.yyyy\nHH:mm"))
				.AddColumn(" Номер \n заказа ")
					.AddTextRenderer(x => x.EdoContainer.Order.Id.ToString())
				.AddColumn(" Код документооборота ")
					.AddTextRenderer(x => x.EdoContainer.DocFlowId.HasValue ? x.EdoContainer.DocFlowId.ToString() : string.Empty)
				.AddColumn(" Отправленные \n документы ")
					.AddTextRenderer(x => x.EdoContainer.SentDocuments)
				.AddColumn(" Статус \n документооборота ")
					.AddEnumRenderer(x => x.EdoContainer.EdoDocFlowStatus)
				.AddColumn(" Доставлено \n клиенту? ")
					.AddToggleRenderer(x => x.EdoContainer.Received)
					.Editing(false)
				.AddColumn(" Описание ошибки ")
					.AddTextRenderer(x => x.EdoContainer.ErrorDescription)
					.WrapWidth(500)
				.AddColumn("")
				.Finish();

			ytreeviewEdoDocuments.ItemsDataSource = ViewModel.EdoContainerNodes;
		}
	}
}
