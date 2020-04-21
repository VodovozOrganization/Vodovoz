using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WageParameterView : TabViewBase<WageParameterViewModel>
	{
		public WageParameterView(WageParameterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			comboWageType.ItemsEnum = typeof(WageParameterTypes);
			comboWageType.Binding.AddBinding(ViewModel, vm => vm.WageParameterType, w => w.SelectedItem).InitializeFromSource();
			comboWageType.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ViewModel.PropertyChanged += (sender, e) => {
				if(e.PropertyName == nameof(ViewModel.TypedWageParameterViewModel)) {
					UpdateWageParameterView();
				}
			};

			UpdateWageParameterView();

			buttonSave.Clicked += (sender, e) => ViewModel.Save();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(false, QS.Navigation.CloseSource.Cancel);
		}

		private Widget wageParameterView;

		private void UpdateWageParameterView()
		{
			wageParameterView?.Destroy();

			if(ViewModel.TypedWageParameterViewModel is FixedWageParameterViewModel) {
				wageParameterView = new FixedWageParameterView((FixedWageParameterViewModel)ViewModel.TypedWageParameterViewModel);
			} else if(ViewModel.TypedWageParameterViewModel is PercentWageParameterViewModel) {
				wageParameterView = new PercentWageParameterView((PercentWageParameterViewModel)ViewModel.TypedWageParameterViewModel);
			} else if(ViewModel.TypedWageParameterViewModel is SalesPlanWageParameterViewModel) {
				wageParameterView = new SalesPlanWageParameterView((SalesPlanWageParameterViewModel)ViewModel.TypedWageParameterViewModel);
			} else if(ViewModel.TypedWageParameterViewModel is RatesLevelWageParameterViewModel) {
				wageParameterView = new RatesLevelWageParameterView((RatesLevelWageParameterViewModel)ViewModel.TypedWageParameterViewModel);
			} else if(ViewModel.TypedWageParameterViewModel is OldRatesWageParameterViewModel) {
				wageParameterView = new OldRatesWageParameterView((OldRatesWageParameterViewModel)ViewModel.TypedWageParameterViewModel);
			} else {
				return;
			}

			vboxDialog.Add(wageParameterView);
			wageParameterView.Show();
		}
	}
}
