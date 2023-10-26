using Gamma.Utilities;
using QS.Views;
using System.ComponentModel;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.ReportsParameters;

namespace Vodovoz.ReportsParameters
{
	[ToolboxItem(true)]
	public partial class SetBillsReportView : ViewBase<SetBillsReportViewModel>
    {
		public SetBillsReportView(SetBillsReportViewModel viewModel)
            : base(viewModel)
		{
			Build();

            daterangepickerOrderCreation.Binding
                .AddSource(ViewModel)
                .AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
                .AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
                .InitializeFromSource();

            entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			ybuttonCreateReport.Clicked += (sender, e) => ViewModel.LoadReport();
			ybuttonCreateReport.TooltipText = $"Формирует отчет по заказам в статусе '{OrderStatus.WaitForPayment.GetEnumTitle()}'";
		}
	}
}
