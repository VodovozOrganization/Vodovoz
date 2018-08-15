using System;
using QSOrmProject;
using Vodovoz.Additions.Store;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	public partial class UserSettingsDlg : OrmGtkDialogBase<UserSettings>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public UserSettingsDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<UserSettings> (id);
			ConfigureDlg ();
		}

		public UserSettingsDlg (UserSettings sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetWarehouseQuery();
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.DefaultWarehouse, w => w.Subject).InitializeFromSource();
		}

		public override bool Save ()
		{
			var valid = new QSValidation.QSValidator<UserSettings> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем настройки пользователя...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

