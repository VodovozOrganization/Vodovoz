using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.ViewModels.Logistic;
using FastDeliveryOrderTransferMode = Vodovoz.ViewModels.ViewModels.Logistic.FastDeliveryOrderTransferFilterViewModel.FastDeliveryOrderTransferMode;

namespace Vodovoz.Views.Logistic
{
	[ToolboxItem(true)]
	public partial class FastDeliveryOrderTransferFilterView : FilterViewBase<FastDeliveryOrderTransferFilterViewModel>
	{
		public FastDeliveryOrderTransferFilterView(
			FastDeliveryOrderTransferFilterViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			ycheckRouteListsAll.Toggled += OnModeChanged;
			ycheckRouteListsFastDelivery.Toggled += OnModeChanged;
			ycheckRouteListsShifted.Toggled += OnModeChanged;
		}

		private void OnModeChanged(object sender, EventArgs e)
		{
			if(sender == ycheckRouteListsAll && ycheckRouteListsAll.Active)
			{
				ViewModel.Mode = FastDeliveryOrderTransferMode.All;
				return;
			}

			if(sender == ycheckRouteListsFastDelivery && ycheckRouteListsFastDelivery.Active)
			{
				ViewModel.Mode = FastDeliveryOrderTransferMode.FastDelivery;
				return;
			}

			if(sender == ycheckRouteListsShifted && ycheckRouteListsShifted.Active)
			{
				ViewModel.Mode = FastDeliveryOrderTransferMode.Shifted;
				return;
			}
		}
	}
}
