using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.ViewModels.ReportsParameters;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class OrderCreationDateReportView : ViewBase<OrderCreationDateReportViewModel>, ISingleUoWDialog
	{
		public OrderCreationDateReportView(OrderCreationDateReportViewModel viewModel) : base(viewModel)
		{
			Build();		
			
			evmeEmployee.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeAutocompleteSelectorFactory);

			evmeEmployee.CanOpenWithoutTabParent = true;
			evmeEmployee.Binding
				.AddBinding(ViewModel, vm => vm.CanEditEmployee, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.Employee, w => w.Subject)
				.InitializeFromSource();

			datePeriodPicker.Binding
				.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			buttonCreateReport.Binding.AddBinding(ViewModel, vm => vm.CanCreateReport, w => w.Sensitive).InitializeFromSource();
			buttonCreateReport.Clicked += (s, e) => ViewModel.LoadReportCommand.Execute();			
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
