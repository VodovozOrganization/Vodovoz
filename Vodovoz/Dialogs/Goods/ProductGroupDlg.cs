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
			yentryName.Binding
					  .AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			yentryOnlineStoreGuid.Binding
			                     .AddBinding(Entity, e => e.OnlineStoreGuid, w => w.Text, new GuidToStringConverter()).InitializeFromSource();

			ycheckOnlineStore.Binding.AddBinding(Entity, e => e.ExportToOnlineStore, w => w.Active).InitializeFromSource();

			yentryParent.SubjectType = typeof(ProductGroup);
			yentryParent.Binding.AddBinding(Entity, e => e.Parent, w => w.Subject).InitializeFromSource();

			checklistCharacteristics.EnumType = typeof(NomenclatureProperties);
			checklistCharacteristics.Binding.AddBinding(Entity, e => e.Characteristics, w => w.SelectedValuesList, new EnumsListConverter<NomenclatureProperties>()).InitializeFromSource();
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var valid = new QSValidation.QSValidator<ProductGroup>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save();
			return true;
		}

		#endregion
	}
}
