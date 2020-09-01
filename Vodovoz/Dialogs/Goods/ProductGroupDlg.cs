using System;
using Gamma.Widgets.Additions;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Dialogs.Goods
{
	public partial class ProductGroupDlg : QS.Dialog.Gtk.EntityDialogBase<ProductGroup>
	{
		public ProductGroupDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<ProductGroup>();
			ConfigureDialog();
		}

		public ProductGroupDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<ProductGroup>(id);
			ConfigureDialog();
		}

		public ProductGroupDlg(ProductGroup sub) : this(sub.Id) { }


		protected void ConfigureDialog()
		{
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			yentryOnlineStoreGuid.Binding.AddBinding(
				Entity, e => e.OnlineStoreGuid, w => w.Text, new GuidToStringConverter()).InitializeFromSource();

			ycheckExportToOnlineStore.Binding.AddBinding(Entity, e => e.ExportToOnlineStore, w => w.Active).InitializeFromSource();
			
			ycheckbuttonOnlineStore.Active = Entity.IsOnlineStore;
			ycheckbuttonOnlineStore.Binding.AddBinding(Entity, e => e.IsOnlineStore, w => w.Active);

			ycheckArchived.Active = Entity.IsArchive;
			ycheckArchived.Binding.AddBinding(Entity, e => e.IsArchive, w => w.Active);
			
			yentryParent.SubjectType = typeof(ProductGroup);
			yentryParent.Binding.AddBinding(Entity, e => e.Parent, w => w.Subject).InitializeFromSource();

			checklistCharacteristics.EnumType = typeof(NomenclatureProperties);
			checklistCharacteristics.Binding.AddBinding(
				Entity, e => e.Characteristics, w => w.SelectedValuesList, new EnumsListConverter<NomenclatureProperties>()).InitializeFromSource();

			ylblOnlineStore.Text = Entity.OnlineStore?.Name;
			ylblOnlineStore.Visible = !String.IsNullOrWhiteSpace(Entity.OnlineStore?.Name);
			ylblOnlineStoreStr.Visible = !String.IsNullOrWhiteSpace(Entity.OnlineStore?.Name);
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var valid = new QS.Validation.QSValidator<ProductGroup>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save();
			return true;
		}

		#endregion
	}
}
