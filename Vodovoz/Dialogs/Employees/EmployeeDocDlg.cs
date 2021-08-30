using System;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Validation;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeDocDlg : SingleUowTabBase
	{
		public EmployeeDocument Entity;
		IUnitOfWork _unitOfWork;
		private readonly ICommonServices _commonServices;
		private IPermissionResult _employeeDocumentsPermissionsSet;

		public EmployeeDocDlg(IUnitOfWork uow, EmployeeDocumentType[] hiddenDocument, ICommonServices commonServices)
		{
			Build();
			_unitOfWork = uow;
			_commonServices = commonServices;
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

		public EmployeeDocDlg(int id, IUnitOfWork uow, ICommonServices commonServices)
		{
			Build();
			_unitOfWork = uow;
			_commonServices = commonServices;
			Entity = (EmployeeDocument)_unitOfWork.GetById(typeof(EmployeeDocument), id);
			TabName = Entity.Document.GetEnumTitle();
			ConfigureDlg();
		}

		public EmployeeDocDlg(EmployeeDocument sub, IUnitOfWork uow, ICommonServices commonServices) : this(sub.Id, uow, commonServices) { }

		private bool SaveDoc()
		{
			var valid = new QSValidator<EmployeeDocument>(Entity);
			valid.RunDlgIfNotValid((Gtk.Window)Toplevel);
			if(valid.IsValid)
			{
				_unitOfWork.Save(Entity);
				OnCloseTab(false);
			}
			return valid.IsValid;
		}

		private void ConfigureDlg()
		{
			_employeeDocumentsPermissionsSet = _commonServices.PermissionService
				.ValidateUserPermission(typeof(EmployeeDocument), _commonServices.UserService.CurrentUserId);

			if(Entity.Id != 0 && !_employeeDocumentsPermissionsSet.CanRead)
			{
				_commonServices.InteractiveService
					.ShowMessage(QS.Dialog.ImportanceLevel.Error, "Недостаточно прав для просмотра документов", "Недостаточно прав");
				FailInitialize = true;
				TabParent?.ForceCloseTab(this, QS.Navigation.CloseSource.Self);
			}

			var canUpdate = _employeeDocumentsPermissionsSet.CanUpdate
						 || (Entity.Id == 0 && _employeeDocumentsPermissionsSet.CanCreate);

			foreach(var widget in table1.Children)
			{
				if(widget != GtkScrolledWindow)
				{
					widget.Sensitive = canUpdate;
				}
			}
			buttonSave.Sensitive = canUpdate;
			ytextviewIssueOrg.Sensitive = canUpdate;

			comboCategory.ItemsEnum = typeof(EmployeeDocumentType);
			comboCategory.Binding.AddBinding(Entity, e => e.Document, w => w.SelectedItemOrNull).InitializeFromSource();

			yentryPasSeria.MaxLength = 30;
			yentryPasSeria.Binding.AddBinding(Entity, e => e.PassportSeria, w => w.Text).InitializeFromSource();
			yentryPassportNumber.MaxLength = 30;
			yentryPassportNumber.Binding.AddBinding(Entity, e => e.PassportNumber, w => w.Text).InitializeFromSource();
			yentryDocName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			ytextviewIssueOrg.Binding.AddBinding(Entity, e => e.PassportIssuedOrg, w => w.Buffer.Text).InitializeFromSource();
			ydatePassportIssuedDate.Binding.AddBinding(Entity, e => e.PassportIssuedDate, w => w.DateOrNull).InitializeFromSource();
			ycheckMainDoc.Binding.AddBinding(Entity, e => e.MainDocument, w => w.Active).InitializeFromSource();
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
