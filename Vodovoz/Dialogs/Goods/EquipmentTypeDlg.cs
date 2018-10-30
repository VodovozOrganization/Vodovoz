using System;
using Vodovoz.Domain;
using QSOrmProject;
using QS.DomainModel.UoW;

namespace Vodovoz
{
	public partial class EquipmentTypeDlg : OrmGtkDialogBase<EquipmentType>{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public EquipmentTypeDlg ()
		{			
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<EquipmentType> ();
			ConfigureDialog ();
		}

		public EquipmentTypeDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<EquipmentType> (id);
			ConfigureDialog ();
		}

		public EquipmentTypeDlg (EquipmentType sub): this(sub.Id) {}


		protected void ConfigureDialog(){
			yentryName.Binding
				.AddBinding (UoWGeneric.Root, equipmentType => equipmentType.Name, widget => widget.Text)
				.InitializeFromSource ();
			
			enumWarrantyType.ItemsEnum = typeof(WarrantyCardType);					
				
			enumWarrantyType.Binding
				.AddBinding (UoWGeneric.Root, equipmentType => equipmentType.WarrantyCardType, widget => widget.SelectedItem)
				.InitializeFromSource ();
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save ()
		{
			var valid = new QSValidation.QSValidator<EquipmentType> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем тип оборудования...");
			UoWGeneric.Save ();
			return true;
		}

		#endregion
	}
}


