using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters
{
	public partial class OrdersByDistrictReport : ViewBase<OrdersByDistrictReportViewModel>, ISingleUoWDialog
	{
		public OrdersByDistrictReport(OrdersByDistrictReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entryDistrict.SetEntityAutocompleteSelectorFactory(ViewModel.DistrictsSelectorFactory);
			entryDistrict.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.District, w => w.Subject)
				.InitializeFromSource();

			checkAllDistrict.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsAllDistricts, w => w.Active)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
