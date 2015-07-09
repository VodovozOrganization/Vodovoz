using System;
using Vodovoz.Domain;
using QSOrmProject;
using NLog;
using QSValidation;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CommentTemplateDlg : OrmGtkDialogBase<CommentTemplate>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public CommentTemplateDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CommentTemplate> ();
			ConfigureDlg ();
		}

		public CommentTemplateDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CommentTemplate> (id);
			ConfigureDlg ();
		}

		public CommentTemplateDlg (CommentTemplate sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			textComment.DataSource = UoWGeneric.Root;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<CommentTemplate> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем шаблон комментария...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

