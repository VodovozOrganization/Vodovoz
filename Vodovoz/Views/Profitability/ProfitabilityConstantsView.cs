using System.ComponentModel;
using Gtk;
using QS.Views;
using Vodovoz.ViewModels.ViewModels.Profitability;
using Vodovoz.ViewWidgets.Profitability;

namespace Vodovoz.Views.Profitability
{
	public partial class ProfitabilityConstantsView : ViewBase<ProfitabilityConstantsViewModel>
	{
		private ProfitabilityConstantsDataView _profitabilityConstantsDataView;
		public ProfitabilityConstantsView(ProfitabilityConstantsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnRecalculateAndSave.Clicked += (sender, args) => ViewModel.RecalculateAndSaveCommand.Execute();
			
			var monthPicker = new MonthPickerView(ViewModel.MonthPickerViewModel);
			monthPicker.Show();
			hboxMonth.Add(monthPicker);
			
			lblCalculationSaved.Binding
				.AddBinding(ViewModel, vm => vm.IsCalculationDateAndAuthorActive, w => w.Visible)
				.InitializeFromSource();
			lblCalculationSaveTimeAndAuthor.Binding
				.AddBinding(ViewModel.Entity, e => e.CalculationDateAndAuthor, w => w.Text)
				.AddBinding(ViewModel, vm => vm.IsCalculationDateAndAuthorActive, w => w.Visible)
				.InitializeFromSource();
			
			CreateAndShowConstantsDataView();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void CreateAndShowConstantsDataView()
		{
			_profitabilityConstantsDataView = new ProfitabilityConstantsDataView(ViewModel.ConstantsDataViewModel);
			_profitabilityConstantsDataView.Show();
			vboxMain.Add(_profitabilityConstantsDataView);
			Box.BoxChild profitabilityConstantsDataBox = (Box.BoxChild)vboxMain[_profitabilityConstantsDataView];
			profitabilityConstantsDataBox.Position = 1;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.ConstantsDataViewModel))
			{
				_profitabilityConstantsDataView.Destroy();
				CreateAndShowConstantsDataView();
			}
		}

		public override void Destroy()
		{
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			base.Destroy();
		}
	}
}
