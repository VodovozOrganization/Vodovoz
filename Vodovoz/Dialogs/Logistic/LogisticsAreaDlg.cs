using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class LogisticsAreaDlg : OrmGtkDialogBase<LogisticsArea>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public LogisticsAreaDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<LogisticsArea>();
			ConfigureDlg();
		}

		public LogisticsAreaDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<LogisticsArea>(id);
			ConfigureDlg();
		}

		public LogisticsAreaDlg(LogisticsArea sub) : this (sub.Id)
		{
		}

		void ConfigureDlg()
		{
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ycheckIsCity.Binding.AddBinding(Entity, e => e.IsCity, w => w.Active).InitializeFromSource();
		}

		public override bool Save()
		{
			var valid = new QSValidation.QSValidator<LogisticsArea>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем {0}...", Entity.Name);
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}
	}
}
