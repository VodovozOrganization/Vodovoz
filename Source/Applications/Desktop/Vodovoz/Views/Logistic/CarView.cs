using Gamma.ColumnConfig;
using QS.Navigation;
using QS.Views.GtkUI;
using QSOrmProject;
using System;
using System.Linq;
using NHibernate.Criterion;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.ViewModels.Logistic;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Models;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using QS.Project.Journal;

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

			vehicleNumberEntry.Binding.AddBinding(ViewModel.Entity, e => e.RegistrationNumber, w => w.Number).InitializeFromSource();

			entryCarModel.CanEditReference = ViewModel.CanEditCarModel;
			entryCarModel.SetEntityAutocompleteSelectorFactory(ViewModel.CarModelJournalFactory
				.CreateCarModelAutocompleteSelectorFactory(ViewModel.LifetimeScope));
			entryCarModel.Binding
				.AddBinding(ViewModel.Entity, e => e.CarModel, w => w.Subject)
				.AddBinding(ViewModel, e => e.CanChangeCarModel, w => w.Sensitive)
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

			yentryFuelCardNumber.Binding.AddBinding(ViewModel.Entity, e => e.FuelCardNumber, w => w.Text).InitializeFromSource();
			yentryFuelCardNumber.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditFuelCardNumber, w => w.Sensitive).InitializeFromSource();

			yentryPTSNum.Binding.AddBinding(ViewModel.Entity, e => e.DocPTSNumber, w => w.Text).InitializeFromSource();
			yentryPTSSeries.Binding.AddBinding(ViewModel.Entity, e => e.DocPTSSeries, w => w.Text).InitializeFromSource();

			entryDriver.SetEntityAutocompleteSelectorFactory(
				ViewModel.EmployeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());

			textDriverInfo.Binding.AddBinding(ViewModel, vm => vm.DriverInfoText, w => w.Text).InitializeFromSource();

			entryDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			dataentryFuelType.SubjectType = typeof(FuelType);
			dataentryFuelType.Binding.AddBinding(ViewModel.Entity, e => e.FuelType, w => w.Subject).InitializeFromSource();
			radiobuttonMain.Active = true;

			minBottlesSpin.Binding.AddBinding(ViewModel.Entity, e => e.MinBottles, w => w.ValueAsInt).InitializeFromSource();
			maxBottlesSpin.Binding.AddBinding(ViewModel.Entity, e => e.MaxBottles, w => w.ValueAsInt).InitializeFromSource();
			minBottlesFromAddressSpin.Binding.AddBinding(ViewModel.Entity, e => e.MinBottlesFromAddress, w => w.ValueAsInt).InitializeFromSource();
			maxBottlesFromAddressSpin.Binding.AddBinding(ViewModel.Entity, e => e.MaxBottlesFromAddress, w => w.ValueAsInt).InitializeFromSource();

			photoviewCar.Binding.AddBinding(ViewModel.Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource();

			attachmentsView.ViewModel = ViewModel.AttachmentsViewModel;

			checkIsArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			
			textDriverInfo.Selectable = true;

			minBottlesFromAddressSpin.Binding.AddBinding(ViewModel, vm => vm.CanChangeBottlesFromAddress, w => w.Sensitive).InitializeFromSource();
			maxBottlesFromAddressSpin.Binding.AddBinding(ViewModel, vm => vm.CanChangeBottlesFromAddress, w => w.Sensitive).InitializeFromSource();

			yTreeGeographicGroups.Selection.Mode = Gtk.SelectionMode.Single;
			yTreeGeographicGroups.ColumnsConfig = FluentColumnsConfig<GeoGroup>.Create()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.Finish();
			yTreeGeographicGroups.ItemsDataSource = ViewModel.Entity.ObservableGeographicGroups;

			carVersionsView.ViewModel = ViewModel.CarVersionsViewModel;
			odometerReadingView.ViewModel = ViewModel.OdometerReadingsViewModel;

			radiobuttonMain.Toggled += OnRadiobuttonMainToggled;
			radioBtnGeographicGroups.Toggled += OnRadioBtnGeographicGroupsToggled;
			radiobuttonFiles.Toggled += OnRadiobuttonFilesToggled;
			btnRemoveGeographicGroup.Clicked += OnBtnRemoveGeographicGroupClicked;

			btnAddGeographicGroup.Clicked += (s, e) => ViewModel.AddGeoGroupCommand.Execute();
			ViewModel.AddGeoGroupCommand.CanExecuteChanged += (s, e) => btnAddGeographicGroup.Sensitive = ViewModel.AddGeoGroupCommand.CanExecute();
			ViewModel.AddGeoGroupCommand.RaiseCanExecuteChanged();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
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
	}
}
