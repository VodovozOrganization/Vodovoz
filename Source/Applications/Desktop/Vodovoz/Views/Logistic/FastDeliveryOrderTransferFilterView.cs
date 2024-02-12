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
			switch(ViewModel.Mode)
			{
				case FastDeliveryOrderTransferMode.All:
					yradiobuttonModeAll.Active = true;
					break;
				case FastDeliveryOrderTransferMode.FastDelivery:
					yradiobuttonModeFastDelivery.Active = true;
					break;
				case FastDeliveryOrderTransferMode.Shifted:
					yradiobuttonModeShifted.Active = true;
					break;
			}

			yradiobuttonModeAll.Toggled += OnModeChanged;
			yradiobuttonModeFastDelivery.Toggled += OnModeChanged;
			yradiobuttonModeShifted.Toggled += OnModeChanged;
		}

		private void OnModeChanged(object sender, EventArgs e)
		{
			if(sender == yradiobuttonModeAll && yradiobuttonModeAll.Active)
			{
				ViewModel.Mode = FastDeliveryOrderTransferMode.All;
				return;
			}

			if(sender == yradiobuttonModeFastDelivery && yradiobuttonModeFastDelivery.Active)
			{
				ViewModel.Mode = FastDeliveryOrderTransferMode.FastDelivery;
				return;
			}

			if(sender == yradiobuttonModeShifted && yradiobuttonModeShifted.Active)
			{
				ViewModel.Mode = FastDeliveryOrderTransferMode.Shifted;
				return;
			}
		}
	}
}
