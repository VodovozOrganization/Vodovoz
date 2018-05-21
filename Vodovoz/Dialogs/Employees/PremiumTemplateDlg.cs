using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PremiumTemplateDlg : OrmGtkDialogBase<PremiumTemplate>
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		public PremiumTemplateDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<PremiumTemplate>();
			ConfigureDlg();
		}

		public PremiumTemplateDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<PremiumTemplate>(id);
			ConfigureDlg();
		}

		public PremiumTemplateDlg(PremiumTemplate sub) : this (sub.Id)
		{
		}

		void ConfigureDlg()
		{
			yentryReason.Binding.AddBinding(Entity, e => e.Reason, w => w.Text).InitializeFromSource();
			yspinbuttonPremiumMoney.Binding.AddBinding(Entity, e => e.PremiumMoney, w => w.ValueAsDecimal).InitializeFromSource();
		}

		public override bool Save()
		{
			var valid = new QSValidator<PremiumTemplate>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем шаблон комментария для штрафа...");
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}
	}
}
