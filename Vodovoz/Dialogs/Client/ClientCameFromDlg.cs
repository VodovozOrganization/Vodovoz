using Gtk;
using QS.DomainModel.UoW;
using QSValidation;
using Vodovoz.Domain.Client;

namespace Vodovoz.Dialogs.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ClientCameFromDlg : QSOrmProject.OrmGtkDialogBase<ClientCameFrom>
	{
		public ClientCameFromDlg()
		{
			this.Build();
			UoWGeneric = ClientCameFrom.Create();
			ConfigureDlg();
		}

		public ClientCameFromDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<ClientCameFrom>(id);
			ConfigureDlg();
		}

		public ClientCameFromDlg(ClientCameFrom entity) : this(entity.Id)
		{
		}

		public override bool Save()
		{
			var valid = new QSValidator<ClientCameFrom>(Entity);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel)) {
				return false;
			}
			UoWGeneric.Save();
			return true;
		}

		private void ConfigureDlg()
		{
			entryName.Binding.AddBinding(Entity, x => x.Name, x => x.Text).InitializeFromSource();
		}
	}
}
