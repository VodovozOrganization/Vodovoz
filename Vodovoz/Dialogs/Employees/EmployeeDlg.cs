using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NLog;
using QSBanks;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	public partial class EmployeeDlg : OrmGtkDialogBase<Employee>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public EmployeeDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Employee>();
			TabName = "Новый сотрудник";
			ConfigureDlg();
		}

		public EmployeeDlg(int id)
		{
			this.Build();
			logger.Info("Загрузка информации о сотруднике...");
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Employee>(id);
			ConfigureDlg();
		}

		public EmployeeDlg(Employee sub) : this(sub.Id)
		{
		}

		private void ConfigureDlg()
		{
			dataentryPassportSeria.MaxLength = 30;
			dataentryPassportSeria.Binding.AddBinding(Entity, e => e.PassportSeria, w => w.Text).InitializeFromSource();
			dataentryPassportNumber.MaxLength = 30;
			dataentryPassportNumber.Binding.AddBinding(Entity, e => e.PassportNumber, w => w.Text).InitializeFromSource();
			dataentryDrivingNumber.MaxLength = 20;
			dataentryDrivingNumber.Binding.AddBinding(Entity, e => e.DrivingNumber, w => w.Text).InitializeFromSource();
			UoWGeneric.Root.PropertyChanged += OnPropertyChanged;
			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;

			checkIsFired.Binding.AddBinding(Entity, e => e.IsFired, w => w.Active).InitializeFromSource();
			checkLargusDriver.Binding.AddBinding(Entity, e => e.LargusDriver, w => w.Active).InitializeFromSource();

			dataentryLastName.Binding.AddBinding(Entity, e => e.LastName, w => w.Text).InitializeFromSource();
			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryPatronymic.Binding.AddBinding(Entity, e => e.Patronymic, w => w.Text).InitializeFromSource();

			dataentryPassportSeria.Binding.AddBinding(Entity, e => e.PassportSeria, w => w.Text).InitializeFromSource();
			dataentryPassportNumber.Binding.AddBinding(Entity, e => e.PassportNumber, w => w.Text).InitializeFromSource();
			entryAddressCurrent.Binding.AddBinding(Entity, e => e.AddressCurrent, w => w.Text).InitializeFromSource();
			entryAddressRegistration.Binding.AddBinding(Entity, e => e.AddressRegistration, w => w.Text).InitializeFromSource();
			entryInn.Binding.AddBinding(Entity, e => e.INN, w => w.Text).InitializeFromSource();

			dataentryAndroidLogin.Binding.AddBinding(Entity, e => e.AndroidLogin, w => w.Text).InitializeFromSource();
			dataentryAndroidPassword.Binding.AddBinding(Entity, e => e.AndroidPassword, w => w.Text).InitializeFromSource();
			yentryDeliveryDaySchedule.SubjectType = typeof(DeliveryDaySchedule);
			yentryDeliveryDaySchedule.Binding.AddBinding(Entity, e => e.DefaultDaySheldule, w => w.Subject).InitializeFromSource();

			yentryDefaultForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery();
			yentryDefaultForwarder.Binding.AddBinding(Entity, e => e.DefaultForwarder, w => w.Subject).InitializeFromSource();

			referenceNationality.SubjectType = typeof(Nationality);
			referenceNationality.Binding.AddBinding(Entity, e => e.Nationality, w => w.Subject).InitializeFromSource();
			yentrySubdivision.SubjectType = typeof(Subdivision);
			yentrySubdivision.Binding.AddBinding(Entity, e => e.Subdivision, w => w.Subject).InitializeFromSource();
			referenceUser.SubjectType = typeof(User);
			referenceUser.CanEditReference = false;
			referenceUser.Binding.AddBinding(Entity, e => e.User, w => w.Subject).InitializeFromSource();

			comboCategory.ItemsEnum = typeof(EmployeeCategory);
			comboCategory.Binding.AddBinding(Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();

			photoviewEmployee.Binding.AddBinding(Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource();
			photoviewEmployee.GetSaveFileName = () => Entity.FullName;

			attachmentFiles.AttachToTable = OrmMain.GetDBTableName(typeof(Employee));
			if(!UoWGeneric.IsNew) {
				attachmentFiles.ItemId = UoWGeneric.Root.Id;
				attachmentFiles.UpdateFileList();
			}
			phonesView.UoW = UoWGeneric;
			if(UoWGeneric.Root.Phones == null)
				UoWGeneric.Root.Phones = new List<Phone>();
			phonesView.Phones = UoWGeneric.Root.Phones;
			accountsView.ParentReference = new ParentReferenceGeneric<Employee, Account>(UoWGeneric, o => o.Accounts);
			accountsView.SetTitle("Банковские счета сотрудника");
			ydateFirstWorkDay.Binding.AddBinding(Entity, e => e.FirstWorkDay, w => w.DateOrNull).InitializeFromSource();
			yspinTripsPriority.Binding.AddBinding(Entity, e => e.TripPriority, w => w.ValueAsShort).InitializeFromSource();
			yspinDriverSpeed.Binding.AddBinding(Entity, e => e.DriverSpeed, w => w.Value, new MultiplierToPercentConverter()).InitializeFromSource();

			ytreeviewDistricts.ColumnsConfig = FluentColumnsConfig<DriverDistrictPriority>.Create()
				.AddColumn("Район").AddTextRenderer(x => x.District.Name)
				.AddColumn("Приоритет").AddNumericRenderer(x => x.Priority + 1)
				.Finish();
			ytreeviewDistricts.Reorderable = true;

			ytreeviewDistricts.SetItemsSource(Entity.ObservableDistricts);

			logger.Info("Ok");
		}

		public override bool HasChanges {
			get { return UoWGeneric.HasChanges || attachmentFiles.HasChanges; }
		}

		void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			logger.Debug("Property {0} changed", e.PropertyName);
		}

		public override bool Save()
		{
			//Проверяем, чтобы в БД не попала пустая строка
			if(string.IsNullOrWhiteSpace(Entity.AndroidLogin))
				Entity.AndroidLogin = null;

			var valid = new QSValidator<Employee>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			if(Entity.User != null) {
				var associatedEmployees = Repository.EmployeeRepository.GetEmployeesForUser(UoW, Entity.User.Id);
				if(associatedEmployees.Any(e => e.Id != Entity.Id)) {
					string mes = String.Format("Пользователь {0} уже связан с сотрудником {1}, при привязке этого сотрудника к пользователю, старая связь будет удалена. Продолжить?",
									 Entity.User.Name,
									 String.Join(", ", associatedEmployees.Select(e => e.ShortName))
								 );
					if(MessageDialogWorks.RunQuestionDialog(mes)) {
						foreach(var ae in associatedEmployees.Where(e => e.Id != Entity.Id)) {
							ae.User = null;
							UoWGeneric.Save(ae);
						}
					} else
						return false;
				}
			}

			phonesView.SaveChanges();
			logger.Info("Сохраняем сотрудника...");
			try {
				UoWGeneric.Save();
				if(UoWGeneric.IsNew) {
					attachmentFiles.ItemId = UoWGeneric.Root.Id;
				}
				attachmentFiles.SaveChanges();
			} catch(Exception ex) {
				logger.Error(ex, "Не удалось записать сотрудника.");
				QSProjectsLib.QSMain.ErrorMessage((Gtk.Window)this.Toplevel, ex);
				return false;
			}
			logger.Info("Ok");
			return true;

		}

		protected void OnRadioTabInfoToggled(object sender, EventArgs e)
		{
			if(radioTabInfo.Active)
				notebookMain.CurrentPage = 0;
		}

		protected void OnRadioTabFilesToggled(object sender, EventArgs e)
		{
			if(radioTabFiles.Active)
				notebookMain.CurrentPage = 3;
		}

		protected void OnRadioTabAccountingToggled(object sender, EventArgs e)
		{
			if(radioTabAccounting.Active)
				notebookMain.CurrentPage = 2;
		}

		protected void OnComboCategoryEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			radioTabLogistic.Visible 
			    = checkLargusDriver.Visible
				= labelLargusDriver.Visible
				= ((EmployeeCategory)e.SelectedItem == EmployeeCategory.driver);
		}

		protected void OnRadioTabLogisticToggled(object sender, EventArgs e)
		{
			if(radioTabLogistic.Active)
				notebookMain.CurrentPage = 1;
		}

		protected void OnButtonAddDistrictClicked(object sender, EventArgs e)
		{
			var SelectDistrict = new OrmReference(
				UoW,
				Repository.Logistics.LogisticAreaRepository.ActiveAreaQuery()
			);
			SelectDistrict.Mode = OrmReferenceMode.MultiSelect;
			SelectDistrict.ObjectSelected += SelectDistrict_ObjectSelected; ;
			TabParent.AddSlaveTab(this, SelectDistrict);
		}

		protected void OnButtonRemoveDistrictClicked(object sender, EventArgs e)
		{
			var toRemoveDistricts = ytreeviewDistricts.GetSelectedObjects<DriverDistrictPriority>().ToList();
			toRemoveDistricts.ForEach(x => Entity.ObservableDistricts.Remove(x));
		}

		void SelectDistrict_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var addDistricts = e.GetEntities<LogisticsArea>();
			addDistricts.Where(x => Entity.Districts.All(d => d.District.Id != x.Id))
						.Select(x => new DriverDistrictPriority {
							Driver = Entity,
							District = x
						}).ToList().ForEach(x => Entity.ObservableDistricts.Add(x));
		}

	}
}

