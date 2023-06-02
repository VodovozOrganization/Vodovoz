using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Validation;
using Vodovoz.Domain.Employees;
using Vodovoz.Factories;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Employees;
using QS.Attachments.ViewModels.Widgets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using QS.Project.Domain;
using VodovozInfrastructure.Endpoints;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TraineeDlg : QS.Dialog.Gtk.EntityDialogBase<Trainee>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IAttachmentsViewModelFactory _attachmentsViewModelFactory = new AttachmentsViewModelFactory();

		private AttachmentsViewModel _attachmentsViewModel;
		private bool canEdit;

		public TraineeDlg()
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Trainee>();
			ConfigureDlg();
		}

		public TraineeDlg(int id)
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Trainee>(id);
			ConfigureDlg();
		}

		public TraineeDlg(Trainee sub) : this(sub.Id)
		{
		}

		private void ConfigureDlg()
		{
			OnRussianCitizenToggled(null, EventArgs.Empty);
			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;
			canEdit = permissionResult.CanUpdate || (permissionResult.CanCreate && Entity.Id == 0);

			CreateAttachmentsViewModel();
			ConfigureBindings();
		}

		private void ConfigureBindings()
		{
			logger.Info("Настройка биндинга компонентов диалога стажера");
			//Основные
			dataentryLastName.Binding.AddBinding(Entity, e => e.LastName, w => w.Text).InitializeFromSource();
			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryPatronymic.Binding.AddBinding(Entity, e => e.Patronymic, w => w.Text).InitializeFromSource();
			entryAddressCurrent.Binding.AddBinding(Entity, e => e.AddressCurrent, w => w.Text).InitializeFromSource();
			entryAddressRegistration.Binding.AddBinding(Entity, e => e.AddressRegistration, w => w.Text).InitializeFromSource();
			entryInn.Binding.AddBinding(Entity, e => e.INN, w => w.Text).InitializeFromSource();
			dataentryDrivingNumber.MaxLength = 20;
			dataentryDrivingNumber.Binding.AddBinding(Entity, e => e.DrivingLicense, w => w.Text).InitializeFromSource();
			referenceNationality.SubjectType = typeof(Nationality);
			referenceNationality.Binding.AddBinding(Entity, e => e.Nationality, w => w.Subject).InitializeFromSource();
			referenceCitizenship.SubjectType = typeof(Citizenship);
			referenceCitizenship.Binding.AddBinding(Entity, e => e.Citizenship, w => w.Subject).InitializeFromSource();
			photoviewEmployee.Binding.AddBinding(Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource();
			photoviewEmployee.GetSaveFileName = () => Entity.FullName;
			phonesView.UoW = UoWGeneric;
			checkbuttonRussianCitizen.Binding.AddBinding(Entity, e => e.IsRussianCitizen, w => w.Active).InitializeFromSource();
			if(Entity.Phones == null) {
				Entity.Phones = new List<Vodovoz.Domain.Contacts.Phone>();
			}
			phonesView.Phones = Entity.Phones;

			ytreeviewEmployeeDocument.ColumnsConfig = FluentColumnsConfig<EmployeeDocument>.Create()
				.AddColumn("Документ").AddTextRenderer(x => x.Document.GetEnumTitle())
				.AddColumn("Доп. название").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeviewEmployeeDocument.SetItemsSource(Entity.ObservableDocuments);

			//Реквизиты
			accountsView.SetAccountOwner(UoW, Entity);
			accountsView.SetTitle("Банковские счета стажера");

			//Файлы
			attachmentsView.ViewModel = _attachmentsViewModel;

			logger.Info("Ok");
		}

		private void CreateAttachmentsViewModel()
		{
			_attachmentsViewModel = _attachmentsViewModelFactory.CreateNewAttachmentsViewModel(Entity.ObservableAttachments);
		}

		public override bool HasChanges => UoWGeneric.HasChanges;

		public override bool Save()
		{
			var valid = new QSValidator<Trainee>(Entity);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel)) {
				return false;
			}
			phonesView.RemoveEmpty();
			logger.Info("Сохраняем стажера...");
			try
			{
				UoWGeneric.Save();
			}
			catch(Exception ex)
			{
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

		protected void OnRadioTabDocumentsToggled(object sender, EventArgs e)
		{
			if(radioTabDocuments.Active)
				notebookMain.CurrentPage = 3;
		}

		protected void OnButtonChangeToEmployeeClicked(object sender, EventArgs e)
		{
			if(UoW.HasChanges || Entity.Id == 0) {
				if(!MessageDialogHelper.RunQuestionDialog("Для продолжения необходимо сохранить изменения, сохранить и продолжить?")) {
					return;
				}
				if(Save()) {
					OnEntitySaved(true);
				} else {
					return;
				}
			}

			var employeeViewModel = MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<EmployeeViewModel, IEntityUoWBuilder>(
				this, EntityUoWBuilder.ForCreate()).ViewModel;

			Personnel.ChangeTraineeToEmployee(employeeViewModel.Entity, Entity);

			OnCloseTab(false);
		}

		protected void OnRussianCitizenToggled(object sender, EventArgs e)
		{
			if(Entity.IsRussianCitizen == false) {
				labelCitizenship.Visible = true;
				referenceCitizenship.Visible = true;
			} else {
				labelCitizenship.Visible = false;
				referenceCitizenship.Visible = false;
				Entity.Citizenship = null;
			}
		}

		#region Document

		protected void OnButtonAddDocumentClicked(object sender, EventArgs e)
		{
			EmployeeDocDlg dlg = new EmployeeDocDlg(UoW, null, ServicesConfig.CommonServices, canEdit);
			dlg.Save += (object sender1, EventArgs e1) => Entity.ObservableDocuments.Add(dlg.Entity);
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnButtonRemoveDocumentClicked(object sender, EventArgs e)
		{
			var toRemoveDistricts = ytreeviewEmployeeDocument.GetSelectedObjects<EmployeeDocument>().ToList();
			toRemoveDistricts.ForEach(x => Entity.ObservableDocuments.Remove(x));
		}

		protected void OnButtonEditDocumentClicked(object sender, EventArgs e)
		{
			if(ytreeviewEmployeeDocument.GetSelectedObject<EmployeeDocument>() != null)
			{
				EmployeeDocDlg dlg = new EmployeeDocDlg(
					((EmployeeDocument)ytreeviewEmployeeDocument.GetSelectedObjects()[0]).Id, UoW, ServicesConfig.CommonServices, canEdit);
				TabParent.AddSlaveTab(this, dlg);
			}

		}

		protected void OnEmployeeDocumentRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonDocumentEdit.Click();
		}
		#endregion

		public override void Destroy()
		{
			attachmentsView.Destroy();
			base.Destroy();
		}
	}
}
