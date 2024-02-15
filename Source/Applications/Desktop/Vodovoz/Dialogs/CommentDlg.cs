using System;
using NLog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Domain.Client;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CommentDlg : QS.Dialog.Gtk.EntityDialogBase<CommentsTemplates>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger();



		public CommentDlg()
		{
			this.Build();

			UoWGeneric = CommentsTemplates.Create();
			ConfigureDlg();
		}

		public CommentDlg(CommentsTemplates sub) : this(sub.Id)
		{
		}

		public CommentDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CommentsTemplates>(id);
			ConfigureDlg();
		}


		void ConfigureDlg()
		{

			referenceGroup.SubjectType = typeof(CommentsGroups);
			referenceGroup.Binding.AddBinding(Entity, e => e.CommentsTmpGroups, w => w.Subject).InitializeFromSource();

			entryComment.Binding.AddBinding(Entity, e => e.CommentTemplate, w => w.Text).InitializeFromSource();

		}

		public override bool Save()
		{
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info("Сохраняем ...");

			UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}
	}
}
