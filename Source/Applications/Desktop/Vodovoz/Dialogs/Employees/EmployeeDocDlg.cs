using System;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeDocDlg : SingleUowTabBase, IAskSaveOnCloseViewModel
	{
		public EmployeeDocument Entity;
		IUnitOfWork _unitOfWork;
		private readonly ICommonServices _commonServices;
		private readonly bool _canEditEmployee;
		private bool _canEdit;

		public EmployeeDocDlg(IUnitOfWork uow, EmployeeDocumentType[] hiddenDocument, ICommonServices commonServices, bool canEditEmployee)
		{
			Build();
			_unitOfWork = uow;
			_commonServices = commonServices;
			_canEditEmployee = canEditEmployee;
			if(hiddenDocument != null)
			{
				comboCategory.AddEnumToHideList(hiddenDocument.Cast<object>().ToArray());
			}

			Entity = new EmployeeDocument();
			TabName = "Новый документ";
			ConfigureDlg();
			ShowAll();
		}

		public event EventHandler Save;

		public EmployeeDocDlg(int id, IUnitOfWork uow, ICommonServices commonServices, bool canEditEmployee)
		{
			Build();
			_unitOfWork = uow;
			_commonServices = commonServices;
			_canEditEmployee = canEditEmployee;
			Entity = (EmployeeDocument)_unitOfWork.GetById(typeof(EmployeeDocument), id);
			TabName = Entity.Document.GetEnumTitle();
			ConfigureDlg();
		}

		public EmployeeDocDlg(EmployeeDocument sub, IUnitOfWork uow, ICommonServices commonServices, bool canEditEmployee)
			: this(sub.Id, uow, commonServices, canEditEmployee) { }

		public bool AskSaveOnClose => _canEdit;
		
		private bool SaveDoc()
		{
			var validator = ServicesConfig.ValidationService;
			var isValid = validator.Validate(Entity);
			if(isValid)
			{
				_unitOfWork.Save(Entity);
				OnCloseTab(false);
			}
			return isValid;
		}

		private void ConfigureDlg()
		{
			var employeeDocumentsPermission = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(EmployeeDocument));

			if(Entity.Id != 0 && !employeeDocumentsPermission.CanRead)
			{
				_commonServices.InteractiveService
					.ShowMessage(QS.Dialog.ImportanceLevel.Error, "Недостаточно прав для просмотра документов", "Недостаточно прав");
				FailInitialize = true;
				TabParent?.ForceCloseTab(this, QS.Navigation.CloseSource.Self);
			}

			_canEdit =
				(employeeDocumentsPermission.CanUpdate || (Entity.Id == 0 && employeeDocumentsPermission.CanCreate)) && _canEditEmployee;
			
			buttonSave.Sensitive = _canEdit;

			comboCategory.ItemsEnum = typeof(EmployeeDocumentType);
			comboCategory.Sensitive = _canEdit;
			comboCategory.Binding
				.AddBinding(Entity, e => e.Document, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yentryPasSeria.MaxLength = 30;
			yentryPasSeria.Binding
				.AddBinding(Entity, e => e.PassportSeria, w => w.Text)
				.InitializeFromSource();
			yentryPassportNumber.MaxLength = 30;
			yentryPassportNumber.Binding
				.AddBinding(Entity, e => e.PassportNumber, w => w.Text)
				.InitializeFromSource();
			yentryDocName.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			ytextviewIssueOrg.Binding
				.AddBinding(Entity, e => e.PassportIssuedOrg, w => w.Buffer.Text)
				.InitializeFromSource();
			ydatePassportIssuedDate.Binding
				.AddBinding(Entity, e => e.PassportIssuedDate, w => w.DateOrNull)
				.InitializeFromSource();
			ycheckMainDoc.Sensitive = _canEdit;
			ycheckMainDoc.Binding
				.AddBinding(Entity, e => e.MainDocument, w => w.Active)
				.InitializeFromSource();
			OnDocumentTypeSelected(this, null);
		}

		protected void OnDocumentTypeSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			var docType = (EmployeeDocumentType)comboCategory.SelectedItem;

			if(Entity.MainDocument == true && docType == EmployeeDocumentType.Passport)
			{
				ycheckMainDoc.Visible = true;
				return;
			}

			if(docType == EmployeeDocumentType.Passport)
			{
				ycheckMainDoc.Visible = true;
			}
			else
			{
				ycheckMainDoc.Active = false;
				ycheckMainDoc.Visible = false;
			}
		}

		protected void OnSaveButtonClicked(object sender, EventArgs e)
		{
			bool isValid = SaveDoc();
			if(isValid)
			{
				Save?.Invoke(sender, e);
			}
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}
	}
}
