using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using FluentNHibernate.Data;
using FluentNHibernate.Utils;
using Gamma.ColumnConfig;
using QS.Attachments.ViewModels.Widgets;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using QS.Views.GtkUI;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModelBased;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
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

			dataentryModel.Binding.AddBinding(ViewModel.Entity, e => e.Model, w => w.Text).InitializeFromSource();
			dataentryRegNumber.Binding.AddBinding(ViewModel.Entity, e => e.RegistrationNumber, w => w.Text).InitializeFromSource();

			comboTypeOfUse.ItemsEnum = typeof(CarTypeOfUse);
			comboTypeOfUse.Binding.AddBinding(ViewModel.Entity, e => e.TypeOfUse, w => w.SelectedItemOrNull).InitializeFromSource();

			comboDriverCarKind.Binding.AddBinding(ViewModel.Entity, e => e.DriverCarKind, w => w.SelectedItem).InitializeFromSource();

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
			yentryFuelCardNumber.Binding.AddFuncBinding(ViewModel.Entity, e => e.CanEditFuelCardNumber, w => w.Sensitive).InitializeFromSource();

			yentryPTSNum.Binding.AddBinding(ViewModel.Entity, e => e.DocPTSNumber, w => w.Text).InitializeFromSource();
			yentryPTSSeries.Binding.AddBinding(ViewModel.Entity, e => e.DocPTSSeries, w => w.Text).InitializeFromSource();

			entryDriver.SetEntityAutocompleteSelectorFactory(
				ViewModel.EmployeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());

			//textDriverInfo.Binding.AddBinding(ViewModel, vm => vm.DriverInfoText, w => w.Text).InitializeFromSource();
			entryDriver.Changed += ViewModel.OnEntryDriverChanged;

			entryDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			dataentryFuelType.SubjectType = typeof(FuelType);
			dataentryFuelType.Binding.AddBinding(ViewModel.Entity, e => e.FuelType, w => w.Subject).InitializeFromSource();
			radiobuttonMain.Active = true;

			dataspinbutton1.Binding.AddBinding(ViewModel.Entity, e => e.FuelConsumption, w => w.Value).InitializeFromSource();
			maxWeightSpin.Binding.AddBinding(ViewModel.Entity, e => e.MaxWeight, w => w.ValueAsInt).InitializeFromSource();
			maxVolumeSpin.Binding.AddBinding(ViewModel.Entity, e => e.MaxVolume, w => w.Value).InitializeFromSource();
			minBottlesSpin.Binding.AddBinding(ViewModel.Entity, e => e.MinBottles, w => w.ValueAsInt).InitializeFromSource();
			maxBottlesSpin.Binding.AddBinding(ViewModel.Entity, e => e.MaxBottles, w => w.ValueAsInt).InitializeFromSource();
			minBottlesFromAddressSpin.Binding.AddBinding(ViewModel.Entity, e => e.MinBottlesFromAddress, w => w.ValueAsInt).InitializeFromSource();
			maxBottlesFromAddressSpin.Binding.AddBinding(ViewModel.Entity, e => e.MaxBottlesFromAddress, w => w.ValueAsInt).InitializeFromSource();

			photoviewCar.Binding.AddBinding(ViewModel.Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource();

			attachmentsView.ViewModel = ViewModel.AttachmentsViewModel;

			checkIsRaskat.Binding.AddBinding(ViewModel.Entity, e => e.IsRaskat, w => w.Active).InitializeFromSource();
				
			checkIsRaskat.Toggled += ViewModel.OnIsRaskatToggled;

			labelRaskatType.Binding.AddBinding(ViewModel.Entity, e => e.IsRaskat, w => w.Visible).InitializeFromSource();

			enumRaskatType.ItemsEnum = typeof(RaskatType);
			enumRaskatType.ShowSpecialStateNot = true;
			enumRaskatType.Binding.AddBinding(ViewModel.Entity, e => e.IsRaskat, w => w.Visible).InitializeFromSource();
			enumRaskatType.Binding.AddBinding(ViewModel.Entity, e => e.RaskatType, w => w.SelectedItemOrNull).InitializeFromSource();
			enumRaskatType.Binding.AddFuncBinding(ViewModel.Entity, e => e.Id == 0, w => w.Sensitive).InitializeFromSource();

			checkIsArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();

			ViewModel.OnEntryDriverChanged(null, null);
			textDriverInfo.Selectable = true;

			dataspinbutton1.Binding.AddBinding(ViewModel, vm => vm.CanChangeVolumeWeightConsumption, w => w.Sensitive).InitializeFromSource();
			maxVolumeSpin.Binding.AddBinding(ViewModel, vm => vm.CanChangeVolumeWeightConsumption, w => w.Sensitive).InitializeFromSource();
			maxWeightSpin.Binding.AddBinding(ViewModel, vm => vm.CanChangeVolumeWeightConsumption, w => w.Sensitive).InitializeFromSource();

			checkIsRaskat.Binding.AddBinding(ViewModel, vm => vm.CanChangeIsRaskat, w => w.Sensitive).InitializeFromSource();
			comboTypeOfUse.Binding.AddBinding(ViewModel, vm => vm.CanChangeCarType, w => w.Sensitive).InitializeFromSource();

			minBottlesFromAddressSpin.Binding.AddBinding(ViewModel, vm => vm.CanChangeBottlesFromAddress, w => w.Sensitive).InitializeFromSource();
			maxBottlesFromAddressSpin.Binding.AddBinding(ViewModel, vm => vm.CanChangeBottlesFromAddress, w => w.Sensitive).InitializeFromSource();

			yTreeGeographicGroups.Selection.Mode = Gtk.SelectionMode.Single;
			yTreeGeographicGroups.ColumnsConfig = FluentColumnsConfig<GeographicGroup>.Create()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.Finish();
			yTreeGeographicGroups.ItemsDataSource = ViewModel.Entity.ObservableGeographicGroups;

			radiobuttonMain.Toggled += new EventHandler(OnRadiobuttonMainToggled);
			radioBtnGeographicGroups.Toggled += new EventHandler(OnRadioBtnGeographicGroupsToggled);
			radiobuttonFiles.Toggled += new EventHandler(OnRadiobuttonFilesToggled);
			comboTypeOfUse.ChangedByUser += ViewModel.OnTypeOfUseChangedByUser;
			btnAddGeographicGroup.Clicked += new EventHandler(OnBtnAddGeographicGroupClicked);
			btnRemoveGeographicGroup.Clicked += new EventHandler(OnBtnRemoveGeographicGroupClicked);
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

		protected void OnBtnAddGeographicGroupClicked(object sender, EventArgs e)
		{
			var selectGeographicGroups = new OrmReference(typeof(GeographicGroup), ViewModel.UoW)
			{
				Mode = OrmReferenceMode.MultiSelect
			};

			selectGeographicGroups.ObjectSelected += SelectGeographicGroups_ObjectSelected;
			
			Tab.TabParent.AddSlaveTab(Tab, selectGeographicGroups);
		}

		void SelectGeographicGroups_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if(yTreeGeographicGroups.ItemsDataSource is GenericObservableList<GeographicGroup> ggList)
			{
				foreach(var item in e.Subjects)
				{
					if(item is GeographicGroup group && !ggList.Any(x => x.Id == group.Id))
					{
						ggList.Add(group);
					}
				}
			}
		}

		protected void OnBtnRemoveGeographicGroupClicked(object sender, EventArgs e)
		{
			if(yTreeGeographicGroups.GetSelectedObject() is GeographicGroup selectedObj
				&& yTreeGeographicGroups.ItemsDataSource is GenericObservableList<GeographicGroup> ggList)
			{
				ggList.Remove(selectedObj);
			}
		}
	}
}
