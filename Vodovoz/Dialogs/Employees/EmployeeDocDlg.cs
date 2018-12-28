using System;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QSValidation;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeDocDlg : QS.Dialog.Gtk.EntityDialogBase<EmployeeDocument>
	{
		public EmployeeDocDlg(Employee employee, EmployeeDocument.DocumentType[] hiddenDocument)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<EmployeeDocument>();
			Entity.Employee = employee;
			comboCategory.AddEnumToHideList(hiddenDocument.Cast<object>().ToArray());
			ConfigureDlg();
			this.ShowAll();
		}

		public EmployeeDocDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<EmployeeDocument>(id);
			ConfigureDlg();
		}

		public EmployeeDocDlg(EmployeeDocument sub) : this(sub.Id)
		{

		}

		public override bool Save()
		{
			UoWGeneric.Save();
			return false;
		}

		private void ConfigureDlg()
		{
			comboCategory.ItemsEnum = typeof(EmployeeDocument.DocumentType);
			comboCategory.Binding.AddBinding(Entity, e => e.Document, w => w.SelectedItemOrNull).InitializeFromSource();

			yentryPasSeria.MaxLength = 30;
			yentryPasSeria.Binding.AddBinding(Entity, e => e.PassportSeria, w => w.Text).InitializeFromSource();
			yentryPassportNumber.MaxLength = 30;
			yentryPassportNumber.Binding.AddBinding(Entity, e => e.PassportNumber, w => w.Text).InitializeFromSource();

			ytextviewIssueOrg.Binding.AddBinding(Entity, e => e.PassportIssuedOrg, w => w.Buffer.Text).InitializeFromSource();
			ydatePassportIssuedDate.Binding.AddBinding(Entity, e => e.PassportIssuedDate, w => w.DateOrNull).InitializeFromSource();
		}
	}
}
