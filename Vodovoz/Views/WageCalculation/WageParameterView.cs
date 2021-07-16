using Gamma.Utilities;
using Gtk;
using QS.ViewModels;
using QS.Views.GtkUI;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WageParameterView : TabViewBase<EmployeeWageParameterViewModel>
	{
		public WageParameterView(EmployeeWageParameterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			comboWageType.ItemsEnum = typeof(WageParameterItemTypes);
			comboWageType.Binding.AddBinding(ViewModel, vm => vm.WageParameterItemType, w => w.SelectedItem).InitializeFromSource();
			comboWageType.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ViewModel.PropertyChanged += (sender, e) => {
				if(e.PropertyName == nameof(ViewModel.WageParameterItemViewModel)) {
					UpdateWageParameterView();
				}
				if(e.PropertyName == nameof(ViewModel.DriverWithCompanyCarWageParameterItemViewModel)) {
					UpdateWageParameterView();
				}
			};

			UpdateWageParameterView();

			buttonSave.Clicked += (sender, e) => ViewModel.Save();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}

		private Notebook notebook;

		private void UpdateWageParameterView()
		{
			notebook?.Destroy();
			notebook = new Notebook();


			notebook.InsertPage(GetWidget(ViewModel.WageParameterItemViewModel), new Label(ViewModel.WageParameterItemType.GetEnumTitle()), 0);
			var ourCarWidget = GetWidget(ViewModel.DriverWithCompanyCarWageParameterItemViewModel);
			if(ourCarWidget != null) {
				notebook.InsertPage(ourCarWidget, new Label(ViewModel.WageParameterItemType.GetEnumTitle() + "(Для авто компании)"), 1);
			}

			vboxDialog.Add(notebook);
			notebook.ShowAll();
		}

		private Widget GetWidget(WidgetViewModelBase viewModel)
		{
			if(viewModel is FixedWageParameterItemViewModel) {
				return new FixedWageParameterView((FixedWageParameterItemViewModel)viewModel);
			} else if(viewModel is PercentWageParameterItemViewModel) {
				return new PercentWageParameterView((PercentWageParameterItemViewModel)viewModel);
			} else if(viewModel is SalesPlanWageParameterItemViewModel) {
				return new SalesPlanWageParameterView((SalesPlanWageParameterItemViewModel)viewModel);
			} else if(viewModel is RatesLevelWageParameterItemViewModel) {
				return new RatesLevelWageParameterView((RatesLevelWageParameterItemViewModel)viewModel);
			} else if(viewModel is OldRatesWageParameterItemViewModel) {
				return new OldRatesWageParameterView((OldRatesWageParameterItemViewModel)viewModel);
			} else {
				return null;
			}
		}
	}
}
