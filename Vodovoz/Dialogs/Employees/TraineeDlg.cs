using System;
using System.Collections.Generic;
using NLog;
using QSBanks;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TraineeDlg : OrmGtkDialogBase<Trainee>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public TraineeDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Trainee>();
			ConfigureDlg();
		}

		public TraineeDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Trainee>(id);
			ConfigureDlg();
		}

		public TraineeDlg(Trainee sub) : this(sub.Id)
		{
		}

		public void ConfigureDlg()
		{
			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;

			ConfigureBindings();
		}

		public void ConfigureBindings()
		{
			logger.Info("Настройка биндинга компонентов диалога стажера");
			//Основные
			dataentryLastName.Binding.AddBinding(Entity, e => e.LastName, w => w.Text).InitializeFromSource();
			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryPatronymic.Binding.AddBinding(Entity, e => e.Patronymic, w => w.Text).InitializeFromSource();
			dataentryPassportSeria.MaxLength = 30;
			dataentryPassportSeria.Binding.AddBinding(Entity, e => e.PassportSeria, w => w.Text).InitializeFromSource();
			dataentryPassportNumber.MaxLength = 30;
			dataentryPassportNumber.Binding.AddBinding(Entity, e => e.PassportNumber, w => w.Text).InitializeFromSource();
			ytextviewPassportIssuedOrg.Binding.AddBinding(Entity, e => e.PassportIssuedOrg, w => w.Buffer.Text).InitializeFromSource();
			ydatePassportIssuedDate.Binding.AddBinding(Entity, e => e.PassportIssuedDate, w => w.DateOrNull).InitializeFromSource();
			entryAddressCurrent.Binding.AddBinding(Entity, e => e.AddressCurrent, w => w.Text).InitializeFromSource();
			entryAddressRegistration.Binding.AddBinding(Entity, e => e.AddressRegistration, w => w.Text).InitializeFromSource();
			entryInn.Binding.AddBinding(Entity, e => e.INN, w => w.Text).InitializeFromSource();
			dataentryDrivingNumber.MaxLength = 20;
			dataentryDrivingNumber.Binding.AddBinding(Entity, e => e.DrivingNumber, w => w.Text).InitializeFromSource();
			referenceNationality.SubjectType = typeof(Nationality);
			referenceNationality.Binding.AddBinding(Entity, e => e.Nationality, w => w.Subject).InitializeFromSource();
			photoviewEmployee.Binding.AddBinding(Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource();
			photoviewEmployee.GetSaveFileName = () => Entity.FullName;
			phonesView.UoW = UoWGeneric;
			if(Entity.Phones == null) {
				Entity.Phones = new List<Phone>();
			}
			phonesView.Phones = Entity.Phones;

			//Реквизиты
			accountsView.ParentReference = new ParentReferenceGeneric<Trainee, Account>(UoWGeneric, o => o.Accounts);
			accountsView.SetTitle("Банковские счета стажера");

			//Файлы
			attachmentFiles.AttachToTable = OrmMain.GetDBTableName(typeof(Trainee));
			if(Entity.Id != 0) {
				attachmentFiles.ItemId = Entity.Id;
				attachmentFiles.UpdateFileList();
			}
			logger.Info("Ok");
		}

		public override bool HasChanges {
			get { return UoWGeneric.HasChanges || attachmentFiles.HasChanges; }
		}

		public override bool Save()
		{
			var valid = new QSValidator<Trainee>(Entity);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel)) {
				return false;
			}
			phonesView.SaveChanges();
			logger.Info("Сохраняем стажера...");
			try {
				UoWGeneric.Save();
				if(Entity.Id != 0) {
					attachmentFiles.ItemId = Entity.Id;
				}
				attachmentFiles.SaveChanges();
			} catch(Exception ex) {
				logger.Error(ex, "Не удалось записать стажера.");
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

		protected void OnRadioTabAccountingToggled(object sender, EventArgs e)
		{
			if(radioTabAccounting.Active)
				notebookMain.CurrentPage = 1;
		}

		protected void OnRadioTabFilesToggled(object sender, EventArgs e)
		{
			if(radioTabFiles.Active)
				notebookMain.CurrentPage = 2;
		}

		protected void OnButtonChangeToEmployeeClicked(object sender, EventArgs e)
		{
			if(UoW.HasChanges || Entity.Id == 0) {
				if(!MessageDialogWorks.RunQuestionDialog("Для продолжения необходимо сохранить изменения, сохранить и продолжить?")) {
					return;
				}
				if(Save()) {
					OnEntitySaved(true);
				} else {
					return;
				}
			}
			var employeeUow = UnitOfWorkFactory.CreateWithNewRoot<Employee>();
			Personnel.ChangeTraineeToEmployee(employeeUow, Entity);
			TabParent.OpenTab(OrmMain.GenerateDialogHashName<Employee>(Entity.Id),
							  () => new EmployeeDlg(employeeUow));
			this.OnCloseTab(false);
		}
	}
}
