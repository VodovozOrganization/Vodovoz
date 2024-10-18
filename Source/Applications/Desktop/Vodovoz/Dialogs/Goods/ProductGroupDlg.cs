﻿using System;
using Gamma.Binding.Converters;
using Gamma.Widgets.Additions;
using NLog;
using QS.Dialog.GtkUI;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Representations.ProductGroups;

namespace Vodovoz.Dialogs.Goods
{
	public partial class ProductGroupDlg : QS.Dialog.Gtk.EntityDialogBase<ProductGroup>
	{
		public ProductGroupDlg()
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<ProductGroup>();
			ConfigureDialog();
		}

		public ProductGroupDlg(int id)
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<ProductGroup>(id);
			ConfigureDialog();
		}

		public ProductGroupDlg(ProductGroup sub) : this(sub.Id) { }

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected void ConfigureDialog()
		{
			if(Entity.Id != 0 && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_online_store"))
				vbox2.Sensitive = false;
			
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			yentryOnlineStoreGuid.Binding.AddBinding(
				Entity, e => e.OnlineStoreGuid, w => w.Text, new GuidToStringConverter()).InitializeFromSource();

			ycheckExportToOnlineStore.Binding.AddBinding(Entity, e => e.ExportToOnlineStore, w => w.Active).InitializeFromSource();
			
			ycheckArchived.Binding.AddBinding(Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			ycheckArchived.Toggled += OnArchiveToggled;

			ycheckbuttonIsHighlightInCarLoadDocument.Binding
				.AddBinding(Entity, e => e.IsHighlightInCarLoadDocument, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonIsHighlightInCarLoadDocument.Toggled += OnIsHighlightInCarLoadDocumentToggled;

			ycheckbuttonIsNeedAdditionalControl.Binding
				.AddBinding(Entity, e => e.IsNeedAdditionalControl, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonIsNeedAdditionalControl.Toggled += OnIsNeedAdditionalControlToggled;
			
			entryParent.JournalButtons = Buttons.None;
			entryParent.RepresentationModel = new ProductGroupVM(UoW, new ProductGroupFilterViewModel
			{
				HidenByDefault = false,
				HideArchive = true
			});
			entryParent.Binding.AddBinding(Entity, e => e.Parent, w => w.Subject).InitializeFromSource();

			checklistCharacteristics.EnumType = typeof(NomenclatureProperties);
			checklistCharacteristics.Binding.AddBinding(
				Entity, e => e.Characteristics, w => w.SelectedValuesList, new EnumsListConverter<NomenclatureProperties>()).InitializeFromSource();

			ylblOnlineStore.Text = Entity.OnlineStore?.Name;
			ylblOnlineStore.Visible = !string.IsNullOrWhiteSpace(Entity.OnlineStore?.Name);
			ylblOnlineStoreStr.Visible = !string.IsNullOrWhiteSpace(Entity.OnlineStore?.Name);
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info("Сохранение...");
			UoWGeneric.Save();
			logger.Info("Ок");
			return true;
		}

		#endregion

		private void OnArchiveToggled(object sender, EventArgs e)
		{
			var archiving = ycheckArchived.Active;
			if(!archiving)
			{
				var parent = Entity.Parent;
				if(parent != null && parent.IsArchive)
				{
					ycheckArchived.Active = true;
					MessageDialogHelper.RunWarningDialog(
						$"Родительская группа {parent.Name} архивирована.\n" +
						"Выполните одно из действий:\n" +
						"Либо уберите родительскую группу у данной группы\n" +
						"Либо разархивируйте родителя\n" +
						"Либо перенесите эту группу в действующую неархивную группу товаров");
				}
			}
			else
			{
				Entity.FetchChilds(UoW);
				Entity.SetIsArchiveRecursively(archiving);
			}
		}

		private void OnIsHighlightInCarLoadDocumentToggled(object sender, EventArgs e)
		{
			var infoMessage = $"Атрибут \"Выделять в талонах погрузки\" будет " +
				$"{(Entity.IsHighlightInCarLoadDocument ? "проставлен" : "снят")} " +
				$"также для всех дочерних групп";

			MessageDialogHelper.RunInfoDialog(infoMessage);

			Entity.FetchChilds(UoW);
			Entity.SetIsHighlightInCarLoadDocumenToAllChildGroups(Entity.IsHighlightInCarLoadDocument);
		}

		private void OnIsNeedAdditionalControlToggled(object sender, EventArgs e)
		{
			var infoMessage = $"Атрибут \"Требует доп. контроля водителя\" будет " +
				$"{(Entity.IsNeedAdditionalControl ? "проставлен" : "снят")} " +
				$"также для всех дочерних групп";

			MessageDialogHelper.RunInfoDialog(infoMessage);

			Entity.FetchChilds(UoW);
			Entity.SetIsNeedAdditionalControlToAllChildGroups(Entity.IsNeedAdditionalControl);
		}

		public override void Destroy()
		{
			ycheckArchived.Toggled -= OnArchiveToggled;
			ycheckbuttonIsHighlightInCarLoadDocument.Toggled -= OnIsHighlightInCarLoadDocumentToggled;
			ycheckbuttonIsNeedAdditionalControl.Toggled -= OnIsNeedAdditionalControlToggled;
			base.Destroy();
		}
	}
}
