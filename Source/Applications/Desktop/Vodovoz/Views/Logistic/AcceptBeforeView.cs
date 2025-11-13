using QS.Views.Dialog;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Presentation.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class AcceptBeforeView : DialogViewBase<AcceptBeforeViewModel>
	{
		public AcceptBeforeView(AcceptBeforeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			cmbHours.ItemsList = new[] {
				"00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23",
			};
			cmbHours.Binding.AddBinding(ViewModel, vm => vm.Hours, w => w.SelectedItem, new NullableIntToStringConverter()).InitializeFromSource();
			
			cmbMinutes.ItemsList = new[] {
				"00", "05", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55", "59"
			};
			cmbMinutes.Binding.AddBinding(ViewModel, vm => vm.Minutes, w => w.SelectedItem, new NullableIntToStringConverter()).InitializeFromSource();
			
			cmbHours.SelectedItem = "12";
			cmbMinutes.SelectedItem = "00";

			buttonSave.Clicked += (sender, e) => ViewModel.Save();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}
	}
}
