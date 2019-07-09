using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;
using QS.Project.Journal.EntitySelector;
using QS.Project.Journal;
using Vodovoz.Dialogs.Cash;
using QS.Project.Domain;
using QS.DomainModel.Config;
using Vodovoz.Infrastructure.Services;
using Gamma.ColumnConfig;
using Vodovoz.Domain.Fuel;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Dialogs.Fuel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FuelWriteoffDocumentView : TabViewBase<FuelWriteoffDocumentViewModel>
	{
		public FuelWriteoffDocumentView(FuelWriteoffDocumentViewModel fuelWriteoffDocumentViewModel) : base(fuelWriteoffDocumentViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			fuelbalanceview.ViewModel = ViewModel.FuelBalanceViewModel;

			ydatepickerDate.Binding.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date).InitializeFromSource();
			ydatepickerDate.Binding.AddBinding(ViewModel, e => e.CanEditDate, w => w.Sensitive).InitializeFromSource();
			ylabelCashierValue.Binding.AddBinding(ViewModel.Entity, e => e.Cashier, w => w.LabelProp, new EmployeeToLastNameWithInitialsConverter()).InitializeFromSource();

			IEntityConfigurationProvider entityConfigurationProvider = new DefaultEntityConfigurationProvider();
			var expenseCategorySelectorFactory = new SimpleEntitySelectorFactory<ExpenseCategory, ExpenseCategoryViewModel>(() => {
				var expenseCategoryJournalViewModel = new SimpleEntityJournalViewModel<ExpenseCategory, ExpenseCategoryViewModel>(x => x.Name,
						() => new ExpenseCategoryViewModel(EntityConstructorParam.ForCreate(), ServicesConfig.CommonServices),
						(node) => new ExpenseCategoryViewModel(EntityConstructorParam.ForOpen(node.Id), ServicesConfig.CommonServices),
						entityConfigurationProvider, 
						ServicesConfig.CommonServices
					);
				expenseCategoryJournalViewModel.SelectionMode = JournalSelectionMode.Single;
				return expenseCategoryJournalViewModel;
			});
			entryExpenseCategory.SetEntityAutocompleteSelectorFactory(expenseCategorySelectorFactory);
			entryExpenseCategory.Binding.AddBinding(ViewModel.Entity, e => e.ExpenseCategory, w => w.Subject).InitializeFromSource();
			entryExpenseCategory.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entryEmployee.SetEntityAutocompleteSelectorFactory(new EntityRepresentationAdapterFactory(typeof(Employee), () => new EmployeesVM()));
			entryEmployee.Binding.AddBinding(ViewModel.Entity, e => e.Employee, w => w.Subject).InitializeFromSource();
			entryEmployee.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ycomboboxCashSubdivision.SetRenderTextFunc<Subdivision>(s => s.Name);
			ycomboboxCashSubdivision.Binding.AddBinding(ViewModel, vm => vm.AvailableSubdivisions, w => w.ItemsList).InitializeFromSource();
			ycomboboxCashSubdivision.Binding.AddBinding(ViewModel.Entity, e => e.CashSubdivision, w => w.SelectedItem).InitializeFromSource();
			ycomboboxCashSubdivision.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ytextviewReason.Binding.AddBinding(ViewModel.Entity, vm => vm.Reason, w => w.Buffer.Text).InitializeFromSource();
			ytextviewReason.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ytreeviewItems.ColumnsConfig = FluentColumnsConfig<FuelWriteoffDocumentItem>.Create()
				.AddColumn("Топливо").AddTextRenderer(x => x.FuelType.Name)
				.AddColumn("Количество").AddNumericRenderer(x => x.Liters)
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit)
					.AddSetter((c, n) => c.Adjustment = new Gtk.Adjustment(0, 0, (double)ViewModel.GetAvailableLiters(n.FuelType), 1, 10, 0))
				.Finish();
			ytreeviewItems.Selection.Mode = Gtk.SelectionMode.Single;
			ytreeviewItems.ItemsDataSource = ViewModel.Entity.ObservableFuelWriteoffDocumentItems;
			ytreeviewItems.Selection.Changed += (sender, e) => {
				ViewModel.AddWriteoffItemCommand.RaiseCanExecuteChanged();
				ViewModel.DeleteWriteoffItemCommand.RaiseCanExecuteChanged(); 
			};

			ybuttonAddItem.Clicked += (sender, e) => ViewModel.AddWriteoffItemCommand.Execute();
			ybuttonAddItem.Sensitive = ViewModel.AddWriteoffItemCommand.CanExecute();
			ViewModel.AddWriteoffItemCommand.CanExecuteChanged += (sender, e) => { ybuttonAddItem.Sensitive = ViewModel.AddWriteoffItemCommand.CanExecute(); };

			ybuttonDeleteItem.Clicked += (sender, e) => ViewModel.DeleteWriteoffItemCommand.Execute(GetSelectedItem());
			ybuttonDeleteItem.Sensitive = ViewModel.DeleteWriteoffItemCommand.CanExecute(GetSelectedItem());
			ViewModel.DeleteWriteoffItemCommand.CanExecuteChanged += (sender, e) => { ybuttonDeleteItem.Sensitive = ViewModel.DeleteWriteoffItemCommand.CanExecute(GetSelectedItem()); };

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false); };

			buttonPrint.Clicked += (sender, e) => { ViewModel.PrintCommand.Execute(); };
			buttonPrint.Sensitive = ViewModel.PrintCommand.CanExecute();
			ViewModel.PrintCommand.CanExecuteChanged += (sender, e) => { buttonPrint.Sensitive = ViewModel.PrintCommand.CanExecute(); };
		}

		private FuelWriteoffDocumentItem GetSelectedItem()
		{
			return ytreeviewItems.GetSelectedObject() as FuelWriteoffDocumentItem;
		}
	}
}
