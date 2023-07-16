using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets;
using static Vodovoz.ViewModels.Widgets.FastDeliveryTransferViewModel;

namespace Vodovoz.ViewWidgets.Logistics
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FastDeliveryTransferView : WidgetViewBase<FastDeliveryTransferViewModel>
	{
		public FastDeliveryTransferView(FastDeliveryTransferViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			if(ViewModel == null)
			{
				return;
			}

			ylabelInfoAddress.Text = ViewModel.AddressInfo;
			ylabelInfoDriverFrom.Text = ViewModel.DriverInfo;

			ytreeviewDriversList.CreateFluentColumnsConfig<RouteListNode>()
				.AddColumn("Водитель").AddTextRenderer(x => x.DriverFullName, useMarkup: true)
				.AddColumn("Машина").AddTextRenderer(x => x.CarRegistrationNumber, useMarkup: true)
				.Finish();

			ytreeviewDriversList.Binding.AddBinding(ViewModel, v => v.RouteListToSelectedNode, t => t.SelectedRow).InitializeFromSource();

			ytreeviewDriversList.ItemsDataSource = ViewModel.RouteListNodes;

			ybuttonTransfer.Clicked += (s, e) => ViewModel.TransferCommand.Execute();
			ybuttonCancel.Clicked += (s, e) => ViewModel.CancelCommand.Execute();
		}
	}
}
