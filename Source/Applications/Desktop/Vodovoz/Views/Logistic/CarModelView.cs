using Gamma.ColumnConfig;
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

			fuelConsumptionSpin.Binding.AddBinding(ViewModel, vm => vm.FuelConsumption, w => w.Value).InitializeFromSource();

			datepickerVersionDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedFuelDate, w => w.DateOrNull)
				.InitializeFromSource(); 

			ytreeCarFuelVersions.ColumnsConfig = FluentColumnsConfig<CarFuelVersion>.Create()
				.AddColumn("Код").MinWidth(50).HeaderAlignment(0.5f).AddTextRenderer(x => x.Id == 0 ? "Новая" : x.Id.ToString()).XAlign(0.5f)
				.AddColumn("Расход").AddTextRenderer(x => x.FuelConsumption.ToString()).XAlign(0.5f)
				.AddColumn("Начало действия").AddTextRenderer(x => x.StartDate.ToString("g")).XAlign(0.5f)
				.AddColumn("Окончание действия").AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToString("g") : "").XAlign(0.5f)
				.AddColumn("")
				.Finish();
			ytreeCarFuelVersions.ItemsDataSource = ViewModel.Entity.ObservableCarFuelVersions;
			ytreeCarFuelVersions.Binding.AddBinding(ViewModel, vm => vm.SelectedCarFuelVersion, w => w.SelectedRow).InitializeFromSource();

			yspnBtnTechInspectInterval.Binding
				.AddBinding(ViewModel.Entity, e => e.TeсhInspectInterval, w => w.ValueAsInt)	
				.InitializeFromSource();

			buttonNewVersion.Binding.AddBinding(ViewModel, vm => vm.CanAddNewFuelVersion, w => w.Sensitive).InitializeFromSource();
			buttonNewVersion.Clicked += (sender, args) => ViewModel.AddNewCarFuelVersion();

			buttonChangeVersionDate.Binding.AddBinding(ViewModel, vm => vm.CanChangeFuelVersionDate, w => w.Sensitive).InitializeFromSource();
			buttonChangeVersionDate.Clicked += (sender, args) => ViewModel.ChangeFuelVersionStartDate();
		}
	}
}
