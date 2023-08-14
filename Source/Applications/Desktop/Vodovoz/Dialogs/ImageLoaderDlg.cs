using System;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Domain.StoredResources;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ImageLoaderDlg : EntityDialogBase<StoredResource>
	{
		public ImageLoaderDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<StoredResource>();
			UoWGeneric.Root.Type = ResoureceType.Image;
			TabName = "Новое изображение";
			ConfigureDlg();
		}

		public ImageLoaderDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<StoredResource>(id);
			UoWGeneric.Root.Type = ResoureceType.Image;
			ConfigureDlg();
		}

		public ImageLoaderDlg(StoredResource sub): this(sub.Id)
		{
		}

		private void ConfigureDlg()
		{
			imageNameYentry.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			photoview.Binding.AddBinding(Entity, e => e.BinaryFile, w => w.ImageFile).InitializeFromSource();
			comboType.ItemsEnum = typeof(ImageType);
			comboType.Binding.AddBinding(Entity, e => e.ImageType, w => w.SelectedItemOrNull).InitializeFromSource();
			ylabelId.Text = Entity.Id.ToString();
		}

		public override bool Save()
		{
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}
			
			UoWGeneric.Save();
			return true;
		}
	}
}
