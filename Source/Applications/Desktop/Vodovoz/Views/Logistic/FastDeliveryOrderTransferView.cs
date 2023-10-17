using QS.Views.Dialog;
using System;
using Vodovoz.ViewModels.ViewModels.Logistic;
using static Vodovoz.ViewModels.ViewModels.Logistic.FastDeliveryOrderTransferViewModel;

namespace Vodovoz.Views.Logistic
{
	[WindowSize(400, 600)]
	public partial class FastDeliveryOrderTransferView : DialogViewBase<FastDeliveryOrderTransferViewModel>
	{
		public FastDeliveryOrderTransferFilterView Filter { get; }

		public FastDeliveryOrderTransferView(
			FastDeliveryOrderTransferViewModel viewModel)
			: base(viewModel)
		{
			if(viewModel is null)
			{
				throw new ArgumentNullException(nameof(viewModel));
			}

			Filter = new FastDeliveryOrderTransferFilterView(ViewModel.FilterViewModel);
			Build();
			Configure();
		}

		private void Configure()
		{
			vboxFilterContainer.Add(Filter);
			Filter.ShowAll();

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

		public override void Destroy()
		{
			ytreeviewDriversList?.Destroy();
			base.Destroy();
		}
	}
}
