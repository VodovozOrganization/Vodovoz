using NLog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	public partial class FineTemplateDlg : OrmGtkDialogBase<FineTemplate>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public FineTemplateDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<FineTemplate> ();
			ConfigureDlg ();
		}

		public FineTemplateDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<FineTemplate> (id);
			ConfigureDlg ();
		}

		public FineTemplateDlg (FineTemplate sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			yentryReason.Binding.AddBinding(Entity, e => e.Reason, w => w.Text).InitializeFromSource();
			yspinbuttonFineMoney.Binding.AddBinding(Entity, e => e.FineMoney, w => w.ValueAsDecimal).InitializeFromSource();
		}

		public override bool Save ()
		{
			var valid = new QSValidator<FineTemplate> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем шаблон комментария для штрафа...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

