using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeContractDlg : QS.Dialog.Gtk.EntityDialogBase<EmployeeContract>
	{
		public EmployeeContractDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<EmployeeContract>();
			ConfigureDlg();
			this.ShowAll();
		}

		public EmployeeContractDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<EmployeeContract>(id);
			ConfigureDlg();
			this.ShowAll();
		}

		void ConfigureDlg()
		{
			yentry2.Binding.AddBinding(Entity, e => e.Document, w => w.Text).InitializeFromSource();
		}

		public override bool Save()
		{
			throw new NotImplementedException();
		}
	}
}
