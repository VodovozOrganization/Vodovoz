using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class CarModelView : TabViewBase<CarModelViewModel>
	{
		public CarModelView(CarModelViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonSave.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);

			entryManufacturer.SetEntityAutocompleteSelectorFactory(
				ViewModel.CarManufacturerJournalFactory.CreateCarManufacturerAutocompleteSelectorFactory()
			);
			entryManufacturer.Binding
				.AddBinding(ViewModel.Entity, e => e.CarManufacturer, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ycheckIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			maxWeightSpin.Binding
				.AddBinding(ViewModel.Entity, e => e.MaxWeight, w => w.ValueAsInt)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			maxVolumeSpin.Binding
				.AddBinding(ViewModel.Entity, e => e.MaxVolume, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			comboTypeOfUse.ItemsEnum = typeof(CarTypeOfUse);
			comboTypeOfUse.Binding
				.AddBinding(ViewModel.Entity, e => e.CarTypeOfUse, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
