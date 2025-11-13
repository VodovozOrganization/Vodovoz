using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using QS.Navigation;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class CarView : TabViewBase<CarViewModel>
	{
		public CarView(CarViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			notebook1.Page = 0;
			notebook1.ShowTabs = false;
			
			buttonSave.Sensitive = ViewModel.AskSaveOnClose;

			vehicleNumberEntry.Binding.AddBinding(ViewModel.Entity, e => e.RegistrationNumber, w => w.Number).InitializeFromSource();

			entryCarModel.ViewModel = ViewModel.CarModelViewModel;

			entryCarModel.Binding
				.AddBinding(ViewModel, e => e.CanChangeCarModel, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			orderNumberSpin.Binding.AddBinding(ViewModel.Entity, e => e.OrderNumber, w => w.ValueAsInt).InitializeFromSource();

			yentryVIN.Binding.AddBinding(ViewModel.Entity, e => e.VIN, w => w.Text).InitializeFromSource();
			yentryManufactureYear.Binding.AddBinding(ViewModel.Entity, e => e.ManufactureYear, w => w.Text).InitializeFromSource();
			yentryMotorNumber.Binding.AddBinding(ViewModel.Entity, e => e.MotorNumber, w => w.Text).InitializeFromSource();
			yentryChassisNumber.Binding.AddBinding(ViewModel.Entity, e => e.ChassisNumber, w => w.Text).InitializeFromSource();
			yentryCarcaseNumber.Binding.AddBinding(ViewModel.Entity, e => e.Carcase, w => w.Text).InitializeFromSource();
			yentryColor.Binding.AddBinding(ViewModel.Entity, e => e.Color, w => w.Text).InitializeFromSource();
			yentryDocSeries.Binding.AddBinding(ViewModel.Entity, e => e.DocSeries, w => w.Text).InitializeFromSource();
			yentryDocNumber.Binding.AddBinding(ViewModel.Entity, e => e.DocNumber, w => w.Text).InitializeFromSource();
			yentryDocIssuedOrg.Binding.AddBinding(ViewModel.Entity, e => e.DocIssuedOrg, w => w.Text).InitializeFromSource();
			ydatepickerDocIssuedDate.Binding.AddBinding(ViewModel.Entity, e => e.DocIssuedDate, w => w.DateOrNull).InitializeFromSource();

			yenumcomboboxArchivingReason.ItemsEnum = typeof(ArchivingReason);
			yenumcomboboxArchivingReason.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.ArchivingReason, w => w.SelectedItemOrNull)
				.AddBinding(e => e.IsArchive, w => w.Visible)
				.InitializeFromSource();

			ylabelArchivingReason.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Visible)
				.InitializeFromSource();

			yentryPTSNum.Binding.AddBinding(ViewModel.Entity, e => e.DocPTSNumber, w => w.Text).InitializeFromSource();
			yentryPTSSeries.Binding.AddBinding(ViewModel.Entity, e => e.DocPTSSeries, w => w.Text).InitializeFromSource();

			entryDriver.ViewModel = ViewModel.DriverViewModel;

			textDriverInfo.Binding.AddBinding(ViewModel, vm => vm.DriverInfoText, w => w.Text).InitializeFromSource();

			entryFuelType.ViewModel = ViewModel.FuelTypeViewModel;

			radiobuttonMain.Active = true;

			minBottlesSpin.Binding.AddBinding(ViewModel.Entity, e => e.MinBottles, w => w.ValueAsInt).InitializeFromSource();
			maxBottlesSpin.Binding.AddBinding(ViewModel.Entity, e => e.MaxBottles, w => w.ValueAsInt).InitializeFromSource();
			minBottlesFromAddressSpin.Binding.AddBinding(ViewModel.Entity, e => e.MinBottlesFromAddress, w => w.ValueAsInt).InitializeFromSource();
			maxBottlesFromAddressSpin.Binding.AddBinding(ViewModel.Entity, e => e.MaxBottlesFromAddress, w => w.ValueAsInt).InitializeFromSource();

			photoviewCar.Binding
				.AddBinding(ViewModel, vm => vm.Photo, w => w.ImageFile)
				.AddBinding(ViewModel, vm => vm.PhotoFilename, w => w.FileName)
				.InitializeFromSource();

			attachedfileinformationsview1.InitializeViewModel(ViewModel.AttachedFileInformationsViewModel);

			checkIsArchive.Binding
				.AddBinding(ViewModel, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonUsedInDelivery.Binding
				.AddBinding(ViewModel.Entity, e => e.IsUsedInDelivery, w => w.Active)
				.InitializeFromSource();

			ylabelArchivingDate.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.ArchivingDate != null, w => w.Visible)
				.InitializeFromSource();

			datepickerArchivingDate.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(e => e.ArchivingDate, w => w.DateOrNull)
				.AddFuncBinding(e => e.ArchivingDate != null, w => w.Visible)
				.InitializeFromSource();

			textDriverInfo.Selectable = true;

			minBottlesFromAddressSpin.Binding.AddBinding(ViewModel, vm => vm.CanChangeBottlesFromAddress, w => w.Sensitive).InitializeFromSource();
			maxBottlesFromAddressSpin.Binding.AddBinding(ViewModel, vm => vm.CanChangeBottlesFromAddress, w => w.Sensitive).InitializeFromSource();

			yTreeGeographicGroups.Selection.Mode = Gtk.SelectionMode.Single;
			yTreeGeographicGroups.ColumnsConfig = FluentColumnsConfig<GeoGroup>.Create()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.Finish();
			yTreeGeographicGroups.ItemsDataSource = ViewModel.Entity.ObservableGeographicGroups;

			carVersionsView.ViewModel = ViewModel.CarVersionsViewModel;
			carversioneditingview.ViewModel = ViewModel.CarVersionEditingViewModel;
			odometerReadingView.ViewModel = ViewModel.OdometerReadingsViewModel;
			fuelcardversionview.ViewModel = ViewModel.FuelCardVersionViewModel;
			carinsuranceversionviewOsago.ViewModel = ViewModel.OsagoInsuranceVersionViewModel;
			carinsuranceversionviewKasko.ViewModel = ViewModel.KaskoInsuranceVersionViewModel;
			carinsuranceversioneditingview.ViewModel = ViewModel.CarInsuranceVersionEditingViewModel;

			radiobuttonMain.Toggled += OnRadiobuttonMainToggled;
			radioBtnGeographicGroups.Toggled += OnRadioBtnGeographicGroupsToggled;
			radiobuttonFiles.Toggled += OnRadiobuttonFilesToggled;
			btnRemoveGeographicGroup.Clicked += OnBtnRemoveGeographicGroupClicked;

			btnAddGeographicGroup.Clicked += (s, e) => ViewModel.AddGeoGroupCommand.Execute();
			ViewModel.AddGeoGroupCommand.CanExecuteChanged += (s, e) => btnAddGeographicGroup.Sensitive = ViewModel.AddGeoGroupCommand.CanExecute();
			ViewModel.AddGeoGroupCommand.RaiseCanExecuteChanged();

			yentryCarTechnicalCheckup.Binding
				.AddBinding(ViewModel, vm => vm.LastCarTechnicalCheckupDate, w => w.Text)
				.InitializeFromSource();

			yentryPreviousTechInspectDate.Binding
				.AddBinding(ViewModel, vm => vm.PreviousTechInspectDate, w => w.Text)
				.InitializeFromSource();

			yentryPreviousTechInspectOdometer.Binding
				.AddBinding(ViewModel, vm => vm.PreviousTechInspectOdometer, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			yentryUpcomingTechInspectKm.Binding
				.AddBinding(ViewModel, vm => vm.UpcomingTechInspectKm, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			yentryUpcomingTechInspectKm.Changed += OnUpcomingTechInspectKmChanged;

			yentryUpcomingTechInspectLeft.Binding
				.AddBinding(ViewModel, vm => vm.UpcomingTechInspectLeft, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			speciallistcomboboxIncomeChannel.SetRenderTextFunc<IncomeChannel>(x => x.GetEnumDisplayName());
			speciallistcomboboxIncomeChannel.ItemsList = Enum.GetValues(typeof(IncomeChannel));
			speciallistcomboboxIncomeChannel.Binding
				.AddBinding(ViewModel.Entity, e => e.IncomeChannel, w => w.SelectedItem)
				.InitializeFromSource();
			
			if(!ViewModel.CanEdit)
			{
				vboxMain.Sensitive = false;
				vboxGeographicGroups.Sensitive = false;
				attachedfileinformationsview1.Sensitive = false;
			}

			ybuttonOpenCarAcceptanceCertificate.BindCommand(ViewModel.CreateCarAcceptanceCertificateCommand);
			ybuttonCreateRentalContract.BindCommand(ViewModel.CreateRentalContractCommand);

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
		}

		private void OnUpcomingTechInspectKmChanged(object sender, EventArgs e)
		{
			var entry = sender as yEntry;
			var chars = entry.Text.ToCharArray();

			var text = ViewModel.StringHandler.ConvertCharsArrayToNumericString(chars);

			if(string.IsNullOrWhiteSpace(text))
			{
				entry.Text = ViewModel.UpcomingTechInspectKmCalculated.ToString();
				return;
			}

			if(!int.TryParse(text, out int newValue))
			{
				entry.Text = ViewModel.UpcomingTechInspectKmCalculated.ToString();
				return;
			}

			if(ViewModel.UpcomingTechInspectKmCalculated < newValue)
			{
				ViewModel.ShowErrorMessage("Нельзя установить значение более расчетного");
				entry.Text = ViewModel.UpcomingTechInspectKmCalculated.ToString();
				return;
			}

			entry.Text = text;
		}

		protected void OnRadiobuttonMainToggled(object sender, EventArgs e)
		{
			if(radiobuttonMain.Active)
			{
				notebook1.CurrentPage = 0;
			}
		}

		protected void OnRadioBtnGeographicGroupsToggled(object sender, EventArgs e)
		{
			if(radioBtnGeographicGroups.Active)
			{
				notebook1.CurrentPage = 1;
			}
		}

		protected void OnRadiobuttonFilesToggled(object sender, EventArgs e)
		{
			if(radiobuttonFiles.Active)
			{
				notebook1.CurrentPage = 2;
			}
		}

		protected void OnBtnRemoveGeographicGroupClicked(object sender, EventArgs e)
		{
			if(yTreeGeographicGroups.GetSelectedObject() is GeoGroup selectedObj)
			{
				ViewModel.Entity.ObservableGeographicGroups.Remove(selectedObj);
			}
		}

		public override void Destroy()
		{
			yentryUpcomingTechInspectKm.Changed -= OnUpcomingTechInspectKmChanged;

			base.Destroy();
		}
	}
}
