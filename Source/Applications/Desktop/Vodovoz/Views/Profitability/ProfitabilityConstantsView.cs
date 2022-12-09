using System.ComponentModel;
using Gtk;
using QS.Views;
using Vodovoz.ViewModels.Profitability;
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

			btnRecalculateAndSave.Binding
				.AddBinding(ViewModel, vm => vm.IsIdleState, w => w.Sensitive)
				.InitializeFromSource();
			
			var monthPicker = new MonthPickerView(ViewModel.MonthPickerViewModel);
			monthPicker.Show();
			hboxMonth.Add(monthPicker);

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
