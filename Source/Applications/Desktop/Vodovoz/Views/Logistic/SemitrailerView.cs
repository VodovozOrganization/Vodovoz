using Core.Infrastructure;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class SemitrailerView : TabViewBase<SemitrailerViewModel>
	{
		public SemitrailerView(SemitrailerViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CloseCommand);

			buttonSave.Binding
				.AddBinding(ViewModel, vm => vm.AskSaveOnClose, w => w.Sensitive)
				.InitializeFromSource();

			vehicleNumberEntry.Binding
				.AddBinding(ViewModel.Entity, e => e.RegistrationNumber, w => w.Number)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.Sensitive)
				.InitializeFromSource();

			entryCarModel.ViewModel = ViewModel.CarModelViewModel;

			entryCarModel.Binding
				.AddBinding(ViewModel, e => e.CanChangeCarModel, w => w.ViewModel.IsEditable)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.Sensitive)
				.InitializeFromSource();

			yentryVIN.Binding
				.AddBinding(ViewModel.Entity, e => e.VIN, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.IsEditable)
				.InitializeFromSource();

			yentryManufactureYear.Binding
				.AddBinding(ViewModel.Entity, e => e.ManufactureYear, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.IsEditable)
				.InitializeFromSource();
			
			yentryChassisNumber.Binding
				.AddBinding(ViewModel.Entity, e => e.ChassisNumber, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.IsEditable)
				.InitializeFromSource();
			
			yentryColor.Binding
				.AddBinding(ViewModel.Entity, e => e.Color, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.IsEditable)
				.InitializeFromSource();

			yentryDocSeries.Binding
				.AddBinding(ViewModel.Entity, e => e.DocSeries, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.IsEditable)
				.InitializeFromSource();

			yentryDocNumber.Binding
				.AddBinding(ViewModel.Entity, e => e.DocNumber, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.IsEditable)
				.InitializeFromSource();

			yentryDocIssuedOrg.Binding
				.AddBinding(ViewModel.Entity, e => e.DocIssuedOrg, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.IsEditable)
				.InitializeFromSource();

			ydatepickerDocIssuedDate.Binding
				.AddBinding(ViewModel.Entity, e => e.DocIssuedDate, w => w.DateOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.Sensitive)
				.InitializeFromSource();

			yentryPTSNum.Binding
				.AddBinding(ViewModel.Entity, e => e.DocPTSNumber, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.IsEditable)
				.InitializeFromSource();

			yentryPTSSeries.Binding
				.AddBinding(ViewModel.Entity, e => e.DocPTSSeries, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.IsEditable)
				.InitializeFromSource();

			speciallistcomboboxIncomeChannel.SetRenderTextFunc<IncomeChannel>(x => x.GetEnumDisplayName());
			speciallistcomboboxIncomeChannel.ItemsList = Enum.GetValues(typeof(IncomeChannel));
			speciallistcomboboxIncomeChannel.Binding
				.AddBinding(ViewModel.Entity, e => e.IncomeChannel, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEditCarCard, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
