using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Logistic;
using QS.Dialog.GtkUI;
using Vodovoz.TempAdapters;

namespace Vodovoz
{
	public partial class CarsDlg : QS.Dialog.Gtk.EntityDialogBase<Car>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private ICarRepository carRepository;
		private readonly IEmployeeJournalFactory _employeeJournalFactory = new EmployeeJournalFactory();

		public override bool HasChanges => UoWGeneric.HasChanges || attachmentFiles.HasChanges;

		public CarsDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Car>();
			TabName = "Новый автомобиль";
			ConfigureDlg();
		}

		public CarsDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Car>(id);
			ConfigureDlg();
		}

		public CarsDlg(Car sub) : this(sub.Id) { }

		private void ConfigureDlg()
		{
			notebook1.Page = 0;
			notebook1.ShowTabs = false;

			dataentryModel.Binding.AddBinding(Entity, e => e.Model, w => w.Text).InitializeFromSource();
			dataentryRegNumber.Binding.AddBinding(Entity, e => e.RegistrationNumber, w => w.Text).InitializeFromSource();

			comboTypeOfUse.ItemsEnum = typeof(CarTypeOfUse);
			comboTypeOfUse.Binding.AddBinding(Entity, e => e.TypeOfUse, w => w.SelectedItemOrNull).InitializeFromSource();
			
			comboDriverCarKind.ItemsList = UoW.GetAll<DriverCarKind>();
			comboDriverCarKind.Binding.AddBinding(Entity, e => e.DriverCarKind, w => w.SelectedItem).InitializeFromSource();

			orderNumberSpin.Binding.AddBinding(Entity, e => e.OrderNumber, w => w.ValueAsInt).InitializeFromSource();

			yentryVIN.Binding.AddBinding(Entity, e => e.VIN, w => w.Text).InitializeFromSource();
			yentryManufactureYear.Binding.AddBinding(Entity, e => e.ManufactureYear, w => w.Text).InitializeFromSource();
			yentryMotorNumber.Binding.AddBinding(Entity, e => e.MotorNumber, w => w.Text).InitializeFromSource();
			yentryChassisNumber.Binding.AddBinding(Entity, e => e.ChassisNumber, w => w.Text).InitializeFromSource();
			yentryCarcaseNumber.Binding.AddBinding(Entity, e => e.Carcase, w => w.Text).InitializeFromSource();
			yentryColor.Binding.AddBinding(Entity, e => e.Color, w => w.Text).InitializeFromSource();
			yentryDocSeries.Binding.AddBinding(Entity, e => e.DocSeries, w => w.Text).InitializeFromSource();
			yentryDocNumber.Binding.AddBinding(Entity, e => e.DocNumber, w => w.Text).InitializeFromSource();
			yentryDocIssuedOrg.Binding.AddBinding(Entity, e => e.DocIssuedOrg, w => w.Text).InitializeFromSource();
			ydatepickerDocIssuedDate.Binding.AddBinding(Entity, e => e.DocIssuedDate, w => w.DateOrNull).InitializeFromSource();

			yentryFuelCardNumber.Binding.AddBinding(Entity, e => e.FuelCardNumber, w => w.Text).InitializeFromSource();
			yentryFuelCardNumber.Binding.AddFuncBinding(Entity, e => e.CanEditFuelCardNumber, w => w.Sensitive).InitializeFromSource();

			yentryPTSNum.Binding.AddBinding(Entity, e => e.DocPTSNumber, w => w.Text).InitializeFromSource();
			yentryPTSSeries.Binding.AddBinding(Entity, e => e.DocPTSSeries, w => w.Text).InitializeFromSource();
			
			entryDriver.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());
			entryDriver.Changed += OnEntryDriverChanged;
			entryDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			dataentryFuelType.SubjectType = typeof(FuelType);
			dataentryFuelType.Binding.AddBinding(Entity, e => e.FuelType, w => w.Subject).InitializeFromSource();
			radiobuttonMain.Active = true;

			dataspinbutton1.Binding.AddBinding(Entity, e => e.FuelConsumption, w => w.Value).InitializeFromSource();
			maxWeightSpin.Binding.AddBinding(Entity, e => e.MaxWeight, w => w.ValueAsInt).InitializeFromSource();
			maxVolumeSpin.Binding.AddBinding(Entity, e => e.MaxVolume, w => w.Value).InitializeFromSource();
			minBottlesSpin.Binding.AddBinding(Entity, e => e.MinBottles, w => w.ValueAsInt).InitializeFromSource();
			maxBottlesSpin.Binding.AddBinding(Entity, e => e.MaxBottles, w => w.ValueAsInt).InitializeFromSource();
			minBottlesFromAddressSpin.Binding.AddBinding(Entity, e => e.MinBottlesFromAddress, w => w.ValueAsInt).InitializeFromSource();
			maxBottlesFromAddressSpin.Binding.AddBinding(Entity, e => e.MaxBottlesFromAddress, w => w.ValueAsInt).InitializeFromSource();

			photoviewCar.Binding.AddBinding(Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource();
			photoviewCar.GetSaveFileName = () => String.Format("{0}({1})", Entity.Model, Entity.RegistrationNumber);

			carRepository = new CarRepository();

			checkIsRaskat.Active = Entity.IsRaskat;

			Entity.PropertyChanged += (s, e) => {
				if (e.PropertyName == nameof(Entity.IsRaskat) && checkIsRaskat.Active != Entity.IsRaskat) {
					checkIsRaskat.Active = Entity.IsRaskat;
				}
			};

			checkIsRaskat.Toggled += (s, e) => {
				if(Entity.Id == 0 || !carRepository.IsInAnyRouteList(UoW, Entity)) {
					Entity.IsRaskat = checkIsRaskat.Active;
				}
				else if(checkIsRaskat.Active != Entity.IsRaskat) {
					checkIsRaskat.Active = Entity.IsRaskat;
					MessageDialogHelper.RunWarningDialog("На данном автомобиле есть МЛ, смена типа невозможна");
				}
			};
			labelRaskatType.Binding.AddBinding(Entity, e => e.IsRaskat, w => w.Visible).InitializeFromSource();
			
			enumRaskatType.ItemsEnum = typeof(RaskatType);
			enumRaskatType.ShowSpecialStateNot = true;
			enumRaskatType.Binding.AddBinding(Entity, e => e.IsRaskat, w => w.Visible).InitializeFromSource();
			enumRaskatType.Binding.AddBinding(Entity, e => e.RaskatType, w => w.SelectedItemOrNull).InitializeFromSource();
			enumRaskatType.Binding.AddFuncBinding(Entity, e => e.Id == 0, w => w.Sensitive).InitializeFromSource();

			checkIsArchive.Binding.AddBinding(Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();

			attachmentFiles.AttachToTable = OrmConfig.GetDBTableName(typeof(Car));
			if(!UoWGeneric.IsNew) {
				attachmentFiles.ItemId = UoWGeneric.Root.Id;
				attachmentFiles.UpdateFileList();
			}
			OnEntryDriverChanged(null, null);
			textDriverInfo.Selectable = true;

			int currentUserId = ServicesConfig.CommonServices.UserService.CurrentUserId;
			bool canChangeVolumeWeightConsumption = 
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission("can_change_cars_volume_weight_consumption", currentUserId)
				|| Entity.Id == 0
				|| !(Entity.IsCompanyCar || Entity.IsRaskat);

			bool canChangeBottlesFromAddress = ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission("can_change_cars_bottles_from_address", currentUserId);

			dataspinbutton1.Sensitive = canChangeVolumeWeightConsumption;
			maxVolumeSpin.Sensitive = canChangeVolumeWeightConsumption;
			maxWeightSpin.Sensitive = canChangeVolumeWeightConsumption;

			checkIsRaskat.Sensitive = CarTypeIsEditable() || ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_change_car_is_raskat");
			comboTypeOfUse.Sensitive = CarTypeIsEditable();

			minBottlesFromAddressSpin.Sensitive = canChangeBottlesFromAddress;
			maxBottlesFromAddressSpin.Sensitive = canChangeBottlesFromAddress;

			yTreeGeographicGroups.Selection.Mode = Gtk.SelectionMode.Single;
			yTreeGeographicGroups.ColumnsConfig = FluentColumnsConfig<GeographicGroup>.Create()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.Finish();
			yTreeGeographicGroups.ItemsDataSource = Entity.ObservableGeographicGroups;

			UpdateSensitivity();
		}

		bool CarTypeIsEditable() => Entity.Id == 0;

		public override bool Save()
		{
			var valid = new QSValidator<Car>(UoWGeneric.Root);
            if (valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel)) {
				return false;
			}

			logger.Info("Сохраняем автомобиль...");
			try {
				UoWGeneric.Save();
				if(UoWGeneric.IsNew) {
					attachmentFiles.ItemId = UoWGeneric.Root.Id;
				}
				attachmentFiles.SaveChanges();
			} catch(Exception ex) {
				logger.Error(ex, "Не удалось записать Автомобиль.");
				QSProjectsLib.QSMain.ErrorMessage((Gtk.Window)this.Toplevel, ex);
				return false;
			}
			logger.Info("Ok");
			return true;
		}

		protected void OnRadiobuttonMainToggled(object sender, EventArgs e)
		{
			if(radiobuttonMain.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnRadioBtnGeographicGroupsToggled(object sender, EventArgs e)
		{
			if(radioBtnGeographicGroups.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnRadiobuttonFilesToggled(object sender, EventArgs e)
		{
			if(radiobuttonFiles.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnEntryDriverChanged(object sender, EventArgs e)
		{
			if(UoWGeneric.Root.Driver != null) {
				var docs = Entity.Driver.GetMainDocuments();
				if(docs.Any())
					textDriverInfo.Text = string.Format(
						"\tПаспорт: {0} № {1}\n\tАдрес регистрации: {2}",
						UoWGeneric.Root.Driver.Documents[0].PassportSeria,
						UoWGeneric.Root.Driver.Documents[0].PassportNumber,
						UoWGeneric.Root.Driver.AddressRegistration
					);
				else
					textDriverInfo.Text = "Главный документ отсутствует";
			}
		}

		protected void OnBtnAddGeographicGroupClicked(object sender, EventArgs e)
		{
			var selectGeographicGroups = new OrmReference(typeof(GeographicGroup), UoW);
			selectGeographicGroups.Mode = OrmReferenceMode.MultiSelect;
			selectGeographicGroups.ObjectSelected += SelectGeographicGroups_ObjectSelected;
			TabParent.AddSlaveTab(this, selectGeographicGroups);
		}

		void SelectGeographicGroups_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if (yTreeGeographicGroups.ItemsDataSource is GenericObservableList<GeographicGroup> ggList) {
				foreach (var item in e.Subjects) {
					if (item is GeographicGroup group && !ggList.Any(x => x.Id == group.Id)) {
						ggList.Add(group);
					}
				}
			}
		}

		protected void OnBtnRemoveGeographicGroupClicked(object sender, EventArgs e)
		{
			var ggList = yTreeGeographicGroups.ItemsDataSource as GenericObservableList<GeographicGroup>;
			if(yTreeGeographicGroups.GetSelectedObject() is GeographicGroup selectedObj && ggList != null)
				ggList.Remove(selectedObj);
		}

		protected void OnComboTypeOfUseChangedByUser(object sender, EventArgs e)
		{
			UpdateSensitivity();

			if(Entity.IsCompanyCar) {
				Entity.Driver = null;
				Entity.DriverCarKind = null;
			}
			if(CarTypeIsEditable())
				Entity.IsRaskat = false;
		}

		private void UpdateSensitivity()
		{
			comboDriverCarKind.Sensitive = Entity.TypeOfUse.HasValue && !Entity.IsCompanyCar;
		}
	}
}
