using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QSValidation;
using Vodovoz.Repositories;

namespace Vodovoz.Dialogs
{
	public partial class TypeOfEntityDlg : QS.Dialog.Gtk.EntityDialogBase<TypeOfEntity>
	{
		public TypeOfEntityDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<TypeOfEntity>();
			ConfigureDlg();
		}

		public TypeOfEntityDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<TypeOfEntity>(id);
			ConfigureDlg();
		}

		public TypeOfEntityDlg(TypeOfEntity sub) : this(sub.Id) { }

		private void ConfigureDlg()
		{
			IList<Type> items = DomainHelper.GetHavingAttributeEntityTypes<EntityPermissionAttribute>(Assembly.GetAssembly(typeof(TypeOfEntity))).ToList();
			yentryName.Binding.AddBinding(Entity, e => e.CustomName, w => w.Text).InitializeFromSource();
			ySpecCmbEntityType.SetRenderTextFunc<Type>(TypeOfEntityRepository.GetRealName);
			ySpecCmbEntityType.ItemsList = items;
			ySpecCmbEntityType.SelectedItem = items.FirstOrDefault(i => i.Name == Entity.Type);
			ySpecCmbEntityType.ItemSelected += YSpecCmbEntityType_ItemSelected;
		}

		void YSpecCmbEntityType_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			if(e.SelectedItem is Type) {
				Entity.CustomName = TypeOfEntityRepository.GetRealName(e.SelectedItem as Type);
				Entity.Type = (e.SelectedItem as Type).Name;
			}
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var valid = new QSValidator<TypeOfEntity>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save();
			return true;
		}

		#endregion
	}
}
