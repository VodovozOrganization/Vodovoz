using QS.Dialog.Gtk;
using QS.Project.Services;
using Vodovoz.Core.Domain.StoredResources;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ImageLoaderDlg : EntityDialogBase<StoredResource>
	{
		public ImageLoaderDlg()
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<StoredResource>();
			UoWGeneric.Root.Type = ResourceType.Image;
			TabName = "Новое изображение";
			ConfigureDlg();
		}

		public ImageLoaderDlg(int id)
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<StoredResource>(id);
			UoWGeneric.Root.Type = ResourceType.Image;
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
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}
			
			UoWGeneric.Save();
			return true;
		}
	}
}
