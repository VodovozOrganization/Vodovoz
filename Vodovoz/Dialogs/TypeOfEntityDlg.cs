using System;
using System.Collections.Generic;
using System.Linq;
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

		void ConfigureDlg()
		{
			IList<Type> items = TypeOfEntityRepository.GetEntityTypesMarkedByEntityPermissionAttribute(Entity.Id == 0);
			//Добавить сущность, если атрибут убран, но право осталось
			if(!Entity.IsActive)
				items.Add(TypeOfEntityRepository.GetEntityType(Entity.Type));
			yentryName.Binding.AddBinding(Entity, e => e.CustomName, w => w.Text).InitializeFromSource();
			ySpecCmbEntityType.SetRenderTextFunc<Type>(TypeOfEntityRepository.GetRealName);
			ySpecCmbEntityType.ItemsList = items.OrderBy(TypeOfEntityRepository.GetRealName);
			ySpecCmbEntityType.SelectedItem = items.FirstOrDefault(i => i?.Name == Entity.Type);
			ySpecCmbEntityType.ItemSelected += YSpecCmbEntityType_ItemSelected;
			SetControlsAcessibility();
		}

		void YSpecCmbEntityType_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			if(e.SelectedItem is Type) {
				Entity.CustomName = TypeOfEntityRepository.GetRealName(e.SelectedItem as Type);
				Entity.Type = (e.SelectedItem as Type).Name;
			}
		}

		void SetControlsAcessibility()
		{
			ySpecCmbEntityType.Sensitive = Entity.Id == 0;
			yentryName.Sensitive = buttonSave.Sensitive = Entity.IsActive;
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