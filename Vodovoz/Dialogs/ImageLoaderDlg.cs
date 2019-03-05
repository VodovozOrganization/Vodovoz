using System;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSValidation;
using Vodovoz.Domain.StoredResources;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ImageLoaderDlg : EntityDialogBase<StoredImageResource>
	{
		public ImageLoaderDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<StoredImageResource>();
			TabName = "Новое изображение";
			ConfigureDlg();
		}

		public ImageLoaderDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<StoredImageResource>(id);
			ConfigureDlg();
		}

		public ImageLoaderDlg(StoredImageResource sub): this(sub.Id)
		{
		}

		private void ConfigureDlg()
		{
			imageNameYentry.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			photoview.Binding.AddBinding(Entity, e => e.BinaryFile, w => w.ImageFile).InitializeFromSource();
			ylabelId.Text = Entity.Id.ToString();
		}

		public override bool Save()
		{
			var valid = new QSValidator<StoredImageResource>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;
			else {
				UoWGeneric.Save();
				return true;
			}

		}
	}
}
