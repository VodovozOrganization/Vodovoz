using Gamma.Widgets.Additions;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Transport.Reports.IncorrectFuel;
namespace Vodovoz.Views.Reports
{
	public partial class IncorrectFuelReportView : TabViewBase<IncorrectFuelReportViewModel>
	{
		private const int _hpanedDefaultPosition = 440;
		private const int _hpanedMinimalPosition = 16;

		public IncorrectFuelReportView(IncorrectFuelReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			hpanedMain.Position = _hpanedDefaultPosition;

			daterangepickerPeriod.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entityentryCar.ViewModel = ViewModel.CarEntityEntryViewModel;
			entityentryFuelCard.ViewModel = ViewModel.FuelCardEntityEntryViewModel;

			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.Binding
				.AddBinding(ViewModel, vm => vm.CarTypesOfUse, w => w.SelectedValuesList, new EnumsListConverter<CarTypeOfUse>())
				.InitializeFromSource();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.Binding
				.AddBinding(ViewModel, vm => vm.CarOwnTypes, w => w.SelectedValuesList, new EnumsListConverter<CarOwnType>())
				.InitializeFromSource();

			ycheckbuttonExcludeOfficeWorkers.Binding
				.AddBinding(ViewModel, vm => vm.IsExcludeOfficeWorkers, w => w.Active)
				.InitializeFromSource();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			yvboxFilterContainer.Visible = !yvboxFilterContainer.Visible;

			hpanedMain.Position = yvboxFilterContainer.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = yvboxFilterContainer.Visible ? ArrowType.Left : ArrowType.Right;
		}
	}
}
