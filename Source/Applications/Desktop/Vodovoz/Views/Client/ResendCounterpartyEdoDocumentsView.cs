using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using static Vodovoz.ViewModels.Dialogs.Counterparties.ResendCounterpartyEdoDocumentsViewModel;

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
			ybuttonCancel.Clicked += (sender, args) => ViewModel.CancelCommand.Execute();
			ybuttonSelectAll.Clicked += (sender, args) => ViewModel.SelectAllCommand.Execute();
			ybuttonUnselectAll.Clicked += (sender, args) => ViewModel.UnselectAllCommand.Execute();
			ybuttonInvertSelected.Clicked += (sender, args) => ViewModel.InvertSelectionCommand.Execute();

			ytreeviewEdoDocuments.ColumnsConfig = FluentColumnsConfig<EdoContainerSelectableNode>.Create()
				.AddColumn(" Выбор ")
					.AddToggleRenderer(x => x.IsSelected)
					.Editing(true)
				.AddColumn(" Номер \n заказа ")
					.AddTextRenderer(x => x.EdoContainer.Order.Id.ToString())
				.AddColumn(" Дата \n создания ")
					.AddTextRenderer(x => x.EdoContainer.Created.ToString("dd.MM.yyyy\nHH:mm"))
				.AddColumn(" Код документооборота ")
					.AddTextRenderer(x => x.EdoContainer.DocFlowId.HasValue ? x.EdoContainer.DocFlowId.ToString() : string.Empty)
				.AddColumn(" Отправленные \n документы ")
					.AddTextRenderer(x => x.EdoContainer.SentDocuments)
				.AddColumn(" Статус \n документооборота ")
					.AddEnumRenderer(x => x.EdoDocFlowStatus)
				.AddColumn(" Доставлено \n клиенту? ")
					.AddToggleRenderer(x => x.EdoContainer.Received)
					.Editing(false)
				.AddColumn(" Описание ошибки ")
					.AddTextRenderer(x => x.EdoContainer.ErrorDescription)
					.WrapWidth(500)
				.AddColumn("")
				.Finish();

			ytreeviewEdoDocuments.ItemsDataSource = ViewModel.EdoContainerNodes;

			ViewModel.EdoContainerNodesListChanged += OnEdoContainerListChanged;
		}

		private void OnEdoContainerListChanged(object sender, EventArgs e)
		{
			ytreeviewEdoDocuments.ItemsDataSource = ViewModel.EdoContainerNodes;
		}

		public override void Destroy()
		{
			if(ViewModel != null)
			{
				ViewModel.EdoContainerNodesListChanged -= OnEdoContainerListChanged;
			}
			ytreeviewEdoDocuments?.Destroy();
			base.Destroy();
		}
	}
}
