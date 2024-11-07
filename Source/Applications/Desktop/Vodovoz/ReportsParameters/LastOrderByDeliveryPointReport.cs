using QS.Views;
using QSWidgetLib;
using System;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters
{
	public partial class LastOrderByDeliveryPointReport : ViewBase<LastOrderByDeliveryPointReportViewModel>
	{
		public LastOrderByDeliveryPointReport(LastOrderByDeliveryPointReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ydatepicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.DateOrNull)
				.InitializeFromSource();

			buttonSanitary.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Sanitary, w => w.Active)
				.InitializeFromSource();

			BottleDeptEntry.ValidationMode = ValidationType.numeric;
			BottleDeptEntry.Changed += BottleDeptChanged;

			buttonCreateRepot.BindCommand(ViewModel.GenerateReportCommand);
		}

		private void BottleDeptChanged(object sender, EventArgs e)
		{
			if(!string.IsNullOrEmpty(BottleDeptEntry.Text))
			{
				ViewModel.BottleDept = Convert.ToInt32(BottleDeptEntry.Text);
			}
			else
			{
				ViewModel.BottleDept = 0;
			}
		}
	}
}
