using NLog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Domain;
using QS.Project.Services;

namespace Vodovoz
{
	public partial class FineTemplateDlg : QS.Dialog.Gtk.EntityDialogBase<FineTemplate>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public FineTemplateDlg ()
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<FineTemplate> ();
			ConfigureDlg ();
		}

		public FineTemplateDlg (int id)
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<FineTemplate> (id);
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
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info ("Сохраняем шаблон комментария для штрафа...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

