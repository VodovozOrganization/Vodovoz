using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeContractDlg : QS.Dialog.Gtk.EntityDialogBase<EmployeeDocument>
	{
		public EmployeeContractDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<EmployeeDocument>();
			ConfigureDlg();
			this.ShowAll();
		}

		void ConfigureDlg()
		{

		}

		public override bool Save()
		{
			throw new NotImplementedException();
		}
	}
}
