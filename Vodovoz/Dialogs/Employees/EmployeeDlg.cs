using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QSBanks;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class EmployeeDlg : OrmGtkDialogBase<Employee>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private EmployeeCategory[] hiddenCategory;

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

		public EmployeeDlg(IUnitOfWorkGeneric<Employee> uow)
		{
			this.Build();
			UoWGeneric = uow;
			if(!QSMain.User.Permissions["can_change_trainee_to_driver"]) {
				hiddenCategory = new EmployeeCategory[] { EmployeeCategory.driver, EmployeeCategory.forwarder };
			}
			ConfigureDlg();

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
			checkVisitingMaster.Binding.AddBinding(Entity, e => e.VisitingMaster, w => w.Active).InitializeFromSource();
			cmbDriverOf.ItemsEnum = typeof(CarTypeOfUse);
			cmbDriverOf.Binding.AddBinding(Entity, e => e.DriverOf, w => w.SelectedItemOrNull).InitializeFromSource();

			dataentryLastName.Binding.AddBinding(Entity, e => e.LastName, w => w.Text).InitializeFromSource();
			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryPatronymic.Binding.AddBinding(Entity, e => e.Patronymic, w => w.Text).InitializeFromSource();

			ytextviewPassportIssuedOrg.Binding.AddBinding(Entity, e => e.PassportIssuedOrg, w => w.Buffer.Text).InitializeFromSource();
			ydatePassportIssuedDate.Binding.AddBinding(Entity, e => e.PassportIssuedDate, w => w.DateOrNull).InitializeFromSource();
			entryAddressCurrent.Binding.AddBinding(Entity, e => e.AddressCurrent, w => w.Text).InitializeFromSource();
			entryAddressRegistration.Binding.AddBinding(Entity, e => e.AddressRegistration, w => w.Text).InitializeFromSource();
			entryInn.Binding.AddBinding(Entity, e => e.INN, w => w.Text).InitializeFromSource();

			dataentryAndroidLogin.Binding.AddBinding(Entity, e => e.AndroidLogin, w => w.Text).InitializeFromSource();
			dataentryAndroidPassword.Binding.AddBinding(Entity, e => e.AndroidPassword, w => w.Text).InitializeFromSource();
			yentryDeliveryDaySchedule.SubjectType = typeof(DeliveryDaySchedule);
			yentryDeliveryDaySchedule.Binding.AddBinding(Entity, e => e.DefaultDaySheldule, w => w.Subject).InitializeFromSource();

			var filterDefaultForwarder = new EmployeeFilter(UoW);
			filterDefaultForwarder.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.driver);
			yentryDefaultForwarder.RepresentationModel = new EmployeesVM(filterDefaultForwarder);
			yentryDefaultForwarder.Binding.AddBinding(Entity, e => e.DefaultForwarder, w => w.Subject).InitializeFromSource();

			referenceNationality.SubjectType = typeof(Nationality);
			referenceNationality.Binding.AddBinding(Entity, e => e.Nationality, w => w.Subject).InitializeFromSource();
			yentrySubdivision.SubjectType = typeof(Subdivision);
			yentrySubdivision.Binding.AddBinding(Entity, e => e.Subdivision, w => w.Subject).InitializeFromSource();
			referenceUser.SubjectType = typeof(User);
			referenceUser.CanEditReference = false;
			referenceUser.Binding.AddBinding(Entity, e => e.User, w => w.Subject).InitializeFromSource();

			comboCategory.ItemsEnum = typeof(EmployeeCategory);
			if(hiddenCategory != null && hiddenCategory.Any()){
				comboCategory.AddEnumToHideList(hiddenCategory.Cast<object>().ToArray());
			}
			comboCategory.Binding.AddBinding(Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();
			comboCategory.ChangedByUser += (sender, e) => {
				if(Entity.Category != EmployeeCategory.driver)
					cmbDriverOf.SelectedItemOrNull = null;
			};
			comboWageCalcType.ItemsEnum = typeof(WageCalculationType);
			comboWageCalcType.Binding.AddBinding(Entity, e => e.WageCalcType, w => w.SelectedItem).InitializeFromSource();

			photoviewEmployee.Binding.AddBinding(Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource();
			photoviewEmployee.GetSaveFileName = () => Entity.FullName;

			attachmentFiles.AttachToTable = OrmMain.GetDBTableName(typeof(Employee));
			if(Entity.Id != 0) {
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
			yspinWageCalcRate.Binding.AddBinding(Entity, e => e.WageCalcRate, w => w.ValueAsDecimal).InitializeFromSource();

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
			    = lblDriverOf.Visible
				= hboxDriversParameters.Visible
				= ((EmployeeCategory)e.SelectedItem == EmployeeCategory.driver);

			labelWageCalcType.Visible
				= hboxCustomWageCalc.Visible
				= ((EmployeeCategory)e.SelectedItem == EmployeeCategory.driver
				   || (EmployeeCategory)e.SelectedItem == EmployeeCategory.forwarder);
			
			hboxCustomWageCalc.Sensitive = QSMain.User.Permissions["can_edit_wage"];
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

		protected void OnComboWageCalcTypeEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			labelWageCalcRate.Visible = yspinWageCalcRate.Visible
				= ((WageCalculationType)e.SelectedItem != WageCalculationType.normal 
				   && (WageCalculationType)e.SelectedItem != WageCalculationType.percentageForService
				   && (WageCalculationType)e.SelectedItem != WageCalculationType.withoutPayment);

			if((WageCalculationType)e.SelectedItem == WageCalculationType.percentage)
			{
				yspinWageCalcRate.Adjustment.Upper = 100;
				Entity.WageCalcRate = Entity.WageCalcRate > 100 ? 100 : Entity.WageCalcRate;
			}

			if((WageCalculationType)e.SelectedItem == WageCalculationType.fixedDay
			   || (WageCalculationType)e.SelectedItem == WageCalculationType.fixedRoute)
			{
				yspinWageCalcRate.Adjustment.Upper = 100000;
			}

			if((WageCalculationType)e.SelectedItem == WageCalculationType.withoutPayment) {
				Entity.WageCalcRate = 0;
			}
		}
	}
}

