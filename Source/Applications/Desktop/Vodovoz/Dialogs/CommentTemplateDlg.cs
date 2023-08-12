﻿using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Domain;

namespace Vodovoz
{
	public partial class CommentTemplateDlg : QS.Dialog.Gtk.EntityDialogBase<CommentTemplate>
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
			textComment.Binding.AddBinding (Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource ();
		}

		public override bool Save ()
		{
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info ("Сохраняем шаблон комментария...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

