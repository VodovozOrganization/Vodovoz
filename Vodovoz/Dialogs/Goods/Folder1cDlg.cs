using System;
using QSOrmProject;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Dialogs.Goods
{
	public partial class Folder1cDlg : OrmGtkDialogBase<Folder1c>
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public Folder1cDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Folder1c>();
			ConfigureDialog();
		}

		public Folder1cDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Folder1c>(id);
			ConfigureDialog();
		}

		public Folder1cDlg(Folder1c sub) : this(sub.Id) { }


		protected void ConfigureDialog()
		{
			yentryName.Binding
			          .AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			yentryCode1c.Binding
			            .AddBinding(Entity, e => e.Code1c, w => w.Text).InitializeFromSource();

			yentryParent.SubjectType = typeof(Folder1c);
			yentryParent.Binding.AddBinding(Entity, e => e.Parent, w => w.Subject).InitializeFromSource();
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var valid = new QSValidation.QSValidator<Folder1c>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save();
			return true;
		}

		#endregion

	}
}
