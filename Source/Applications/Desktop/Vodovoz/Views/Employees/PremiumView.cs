using Gamma.GtkWidgets;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.Views.Employees
{
	public partial class PremiumView : TabViewBase<PremiumViewModel>
	{
		public PremiumView(PremiumViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ylabelDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.Date.ToString("D"), w => w.LabelProp).InitializeFromSource();
			yspinMoney.Binding.AddBinding(ViewModel.Entity, e => e.TotalMoney, w => w.ValueAsDecimal).InitializeFromSource();
			yentryPremiumReasonString.Binding.AddBinding(ViewModel.Entity, e => e.PremiumReasonString, w => w.Text).InitializeFromSource();
			ylabelTotal.Binding.AddBinding(ViewModel, vm => vm.EmployeesSum, w => w.LabelProp).InitializeFromSource();
			employeeViewModelEntry.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeAutocompleteSelectorFactory);
			employeeViewModelEntry.CanEditReference = true;
			employeeViewModelEntry.Sensitive = false;
			employeeViewModelEntry.Binding.AddBinding(ViewModel.Entity, e => e.Author, w => w.Subject).InitializeFromSource();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<PremiumItem>()
				.AddColumn("Сотрудник")
					.AddTextRenderer(x => x.Employee.FullName)
				.AddColumn("Премия")
					.AddNumericRenderer(x => x.Money).Editing().Digits(2)
					.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddColumn("Причина премии")
					.AddTextRenderer(x => x.Premium.PremiumReasonString)
				.Finish();

			ytreeviewItems.ItemsDataSource = ViewModel.Entity.ObservableItems;

			buttonDivideAtAll.Clicked += (sender, e) => ViewModel.DivideAtAllCommand.Execute();
			buttonAdd.Clicked += (sender, e) => ViewModel.AddEmployeeCommand.Execute();
			buttonRemove.Clicked += (sender, e) => ViewModel.DeleteEmployeeCommand
				.Execute(ytreeviewItems.GetSelectedObject<PremiumItem>());
			buttonGetReasonFromTemplate.Clicked += (sender, e) => ViewModel.GetReasonFromTemplate.Execute();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
