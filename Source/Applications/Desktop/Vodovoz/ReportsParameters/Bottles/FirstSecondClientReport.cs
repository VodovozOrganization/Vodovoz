using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FirstSecondClientReport : ViewBase<FirstSecondClientReportViewModel>, ISingleUoWDialog
	{
		public FirstSecondClientReport(FirstSecondClientReportViewModel viewModel) : base(viewModel)
		{
			Build();

			daterangepicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yCpecCmbDiscountReason.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DiscountReason, w => w.SelectedItem)
				.AddBinding(vm => vm.DiscountReasons, w => w.ItemsList)
				.InitializeFromSource();

			evmeAuthor.SetEntityAutocompleteSelectorFactory(ViewModel.AuthorSelectorFactory);
			evmeAuthor.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Author, w => w.Subject)
				.InitializeFromSource();

			chkHasPromoSet.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HasPromoset, w => w.Active)
				.InitializeFromSource();

			ycheckbutton1.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowOnlyClientsWithOneOrder, w => w.Active)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
