using System;
using QSOrmProject;
using Vodovoz.Domain;
using NLog;
using QSValidation;

namespace Vodovoz
{
	public partial class FineCommentTemplateDlg : OrmGtkDialogBase<FineCommentTemplate>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public FineCommentTemplateDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<FineCommentTemplate> ();
			ConfigureDlg ();
		}

		public FineCommentTemplateDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<FineCommentTemplate> (id);
			ConfigureDlg ();
		}

		public FineCommentTemplateDlg (FineCommentTemplate sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			textComment.DataSource = UoWGeneric.Root;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<FineCommentTemplate> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем шаблон комментария для штрафа...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

