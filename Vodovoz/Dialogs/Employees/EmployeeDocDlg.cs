using System;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeDocDlg : QS.Dialog.Gtk.EntityDialogBase<EmployeeDocument>
	{
		public EmployeeDocDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<EmployeeDocument>();
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
			return false;
		}

		private void ConfigureDlg()
		{
			foreach(EmployeeDocument.DocumentType doc in Enum.GetValues(typeof(EmployeeDocument.DocumentType)))
				comboCategory.AppendText(doc.GetEnumTitle());
		}
	}
}
