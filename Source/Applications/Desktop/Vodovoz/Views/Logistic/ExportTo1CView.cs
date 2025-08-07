using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModelBased;
using Vodovoz.ViewModels.ViewModels.Service;

namespace Vodovoz.Views.Logistic
{
	public partial class ExportTo1CView : TabViewBase<ExportTo1CViewModel>
	{
		public ExportTo1CView(ExportTo1CViewModel viewModel)
			: base(viewModel)
		{
			Build();

			ConfigureView();
		}

		private void ConfigureView()
		{
			comboOrganization.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CashlessOrganizations, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedCashlessOrganization, w => w.SelectedItem)
				.AddBinding(vm => vm.CanExport, w => w.Sensitive)
				.InitializeFromSource();

			comboRetailOrganization.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RetailOrganizations, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedRetailOrganization, w => w.SelectedItem)
				.AddBinding(vm => vm.CanExport, w => w.Sensitive)
				.InitializeFromSource();

			dateperiodpicker1.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.AddBinding(vm => vm.CanExport, w => w.Sensitive)
				.InitializeFromSource();

			buttonExportBookkeeping.BindCommand(ViewModel.ExportBookkeepingCommand); // Export(Export1cMode.BuhgalteriaOOO);
			buttonExportBookkeeping.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanExport, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonComplexAutomation1CExport.BindCommand(ViewModel
				.ExportComplexAutomationCommand); //Export(Export1cMode.ComplexAutomation);
			ybuttonComplexAutomation1CExport.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanExport, w => w.Sensitive)
				.InitializeFromSource();

			buttonExportIPTinkoff.BindCommand(ViewModel.ExportIPTinkoffCommand); //Export(Export1cMode.IPForTinkoff);
			buttonExportIPTinkoff.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanExport, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonExportBookkeepingNew.BindCommand(ViewModel.ExportBookkeepingNewCommand); /*.Clicked += (sender, args) => {
				if(comboOrganization.SelectedItem is Organization)
				{
					Export(Export1cMode.BuhgalteriaOOONew);
				}
				else
				{
					MessageDialogHelper.RunWarningDialog("Для этой выгрузки необходимо выбрать организацию");
				}
			};*/

			ybuttonExportBookkeepingNew.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanExport, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
