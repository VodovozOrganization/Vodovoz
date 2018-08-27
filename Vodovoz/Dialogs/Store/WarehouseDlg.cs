using System;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	
	public partial class WarehouseDlg : OrmGtkDialogBase<Warehouse>
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public WarehouseDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Warehouse> ();
			ConfigureDialog ();
		}

		public WarehouseDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Warehouse> (id);
			ConfigureDialog ();
		}

		public WarehouseDlg (Warehouse sub): this(sub.Id) {}


		protected void ConfigureDialog(){
			yentryName.Binding
				.AddBinding (UoWGeneric.Root, (warehouse) => warehouse.Name, (widget) => widget.Text)
				.InitializeFromSource ();
			ycheckOnlineStore.Binding.AddBinding(Entity, e => e.PublishOnlineStore, w => w.Active).InitializeFromSource();
			ycheckbuttonCanReceiveBottles.Binding
				.AddBinding (UoWGeneric.Root, (warehouse) => warehouse.CanReceiveBottles, (widget) => widget.Active)
				.InitializeFromSource ();
			ycheckbuttonCanReceiveEquipment.Binding
				.AddBinding (UoWGeneric.Root, (warehouse) => warehouse.CanReceiveEquipment, (widget) => widget.Active)
				.InitializeFromSource ();
			ycheckbuttonArchive.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.IsArchive, (widget) => widget.Active)
				.InitializeFromSource();
			ycheckbuttonArchive.Sensitive = QSMain.User.Permissions["can_archive_warehouse"];

			comboTypeOfUse.ItemsEnum = typeof(WarehouseUsing);
			comboTypeOfUse.Binding.AddBinding(Entity, e => e.TypeOfUse, w => w.SelectedItem).InitializeFromSource();
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save ()
		{
			var valid = new QSValidation.QSValidator<Warehouse> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем склад...");
			UoWGeneric.Save ();
			return true;
		}

		#endregion
	}
}

