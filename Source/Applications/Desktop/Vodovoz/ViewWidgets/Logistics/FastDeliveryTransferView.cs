using QS.Views.Dialog;
using Vodovoz.ViewModels.Widgets;
using static Vodovoz.ViewModels.Widgets.FastDeliveryTransferViewModel;

namespace Vodovoz.ViewWidgets.Logistics
{
	[WindowSize(400, 600)]
	public partial class FastDeliveryTransferView : DialogViewBase<FastDeliveryTransferViewModel>
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
				.AddColumn("№").AddTextRenderer(x => x.RowNumber.ToString())
				.AddColumn("Водитель").AddTextRenderer(x => x.DriverFullName)
				.AddColumn("Машина").AddTextRenderer(x => x.CarRegistrationNumber)
				.AddColumn("МЛ").AddTextRenderer(x => x.RouteListId.ToString())
				.Finish();

			ytreeviewDriversList.Binding.AddBinding(ViewModel, v => v.RouteListToSelectedNode, t => t.SelectedRow).InitializeFromSource();

			ytreeviewDriversList.ItemsDataSource = ViewModel.RouteListNodes;

			ybuttonTransfer.Clicked += (s, e) => ViewModel.TransferCommand.Execute();
			ybuttonCancel.Clicked += (s, e) => ViewModel.CancelCommand.Execute();
		}
	}
}
