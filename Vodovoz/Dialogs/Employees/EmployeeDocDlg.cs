using System;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSValidation;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeDocDlg : SingleUowTabBase
	{
		public EmployeeDocument Entity;
		IUnitOfWork unitOfWork ;
		public EmployeeDocDlg(IUnitOfWork uow, EmployeeDocumentType[] hiddenDocument)
		{
			this.Build();
			unitOfWork = uow;
			if(hiddenDocument!=null)
				comboCategory.AddEnumToHideList(hiddenDocument.Cast<object>().ToArray());
			Entity = new EmployeeDocument();
			TabName = "Новый документ";
			ConfigureDlg();
			this.ShowAll();
		}

		public event EventHandler Save;

		public EmployeeDocDlg(int id, IUnitOfWork uow)
		{
			this.Build();
			unitOfWork = uow;
			Entity=(EmployeeDocument)unitOfWork.GetById(typeof(EmployeeDocument), id);
			TabName = Entity.Document.GetEnumTitle();
			ConfigureDlg();
		}

		public EmployeeDocDlg(EmployeeDocument sub,IUnitOfWork uow) : this(sub.Id, uow)
		{

		}

		private bool SaveDoc()
		{
			var valid = new QSValidator<EmployeeDocument>(Entity);
			valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel);
			if(valid.IsValid) 
			{
				unitOfWork.Save(Entity);
				OnCloseTab(false);
			}
			return valid.IsValid;
		}

		private void ConfigureDlg()
		{
			comboCategory.ItemsEnum = typeof(EmployeeDocumentType);
			comboCategory.Binding.AddBinding(Entity, e => e.Document, w => w.SelectedItemOrNull).InitializeFromSource();

			yentryPasSeria.MaxLength = 30;
			yentryPasSeria.Binding.AddBinding(Entity, e => e.PassportSeria, w => w.Text).InitializeFromSource();
			yentryPassportNumber.MaxLength = 30;
			yentryPassportNumber.Binding.AddBinding(Entity, e => e.PassportNumber, w => w.Text).InitializeFromSource();
			yentryDocName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			ytextviewIssueOrg.Binding.AddBinding(Entity, e => e.PassportIssuedOrg, w => w.Buffer.Text).InitializeFromSource();
			ydatePassportIssuedDate.Binding.AddBinding(Entity, e => e.PassportIssuedDate, w => w.DateOrNull).InitializeFromSource();
			ycheckMainDoc.Binding.AddBinding(Entity,e=>e.MainDocument, w => w.Active).InitializeFromSource();
			OnDocumentTypeSelected(this, null);
		}

		protected void OnDocumentTypeSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			var docType = (EmployeeDocumentType)comboCategory.SelectedItem;

			if(Entity.MainDocument==true && docType==EmployeeDocumentType.Passport) 
			{
				ycheckMainDoc.Visible = true;
				label6.Visible = true;
				return;
			}

			if(docType== EmployeeDocumentType.Passport) 
			{
				ycheckMainDoc.Visible = true;
				label6.Visible = true;
			} 
			else 
			{
				ycheckMainDoc.Active = false;
				ycheckMainDoc.Visible = false;
				label6.Visible = false;
			}
		}

		protected void OnSaveButtonClicked(object sender, EventArgs e)
		{
			bool isValid=SaveDoc();
			if(isValid)
				Save?.Invoke(sender, e);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}
	}
}
