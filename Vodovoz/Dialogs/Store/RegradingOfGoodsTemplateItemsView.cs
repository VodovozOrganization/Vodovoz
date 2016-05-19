using System;
using Gamma.GtkWidgets;
using Vodovoz.Domain.Store;
using QSOrmProject;
using System.Collections.Generic;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RegradingOfGoodsTemplateItemsView : WidgetOnDialogBase
	{
		RegradingOfGoodsTemplateItem newRow;

		public RegradingOfGoodsTemplateItemsView()
		{
			this.Build();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<RegradingOfGoodsTemplateItem>()
				.AddColumn("Старая номенклатура").AddTextRenderer(x => x.NomenclatureOld.Name)
				.AddColumn("Новая номенклатура").AddTextRenderer(x => x.NomenclatureNew.Name)
				.Finish();
			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;
		}

		void YtreeviewItems_Selection_Changed (object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		private IUnitOfWorkGeneric<RegradingOfGoodsTemplate> templateUoW;

		public IUnitOfWorkGeneric<RegradingOfGoodsTemplate> TemplateUoW {
			get { return templateUoW; }
			set {
				if (templateUoW == value)
					return;
				templateUoW = value;
				if (TemplateUoW.Root.Items == null)
					TemplateUoW.Root.Items = new List<RegradingOfGoodsTemplateItem> ();

				ytreeviewItems.ItemsDataSource = TemplateUoW.Root.ObservableItems;
				UpdateButtonState();
			}
		}

		private void UpdateButtonState()
		{
			var selected = ytreeviewItems.GetSelectedObject<RegradingOfGoodsTemplateItem>();
			buttonChangeNew.Sensitive = buttonDelete.Sensitive = buttonChangeOld.Sensitive = selected != null;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var selectOldNomenclature = new OrmReference(Repository.NomenclatureRepository.NomenclatureOfGoodsOnlyQuery());
			selectOldNomenclature.TabName =	"Выберите номенклатуру на замену";
			selectOldNomenclature.Mode = OrmReferenceMode.Select;
			selectOldNomenclature.ObjectSelected += SelectOldNomenclature_ObjectSelected1;
			MyTab.TabParent.AddSlaveTab(MyTab, selectOldNomenclature);
		}

		void SelectOldNomenclature_ObjectSelected1 (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var nomenclature = e.Subject as Nomenclature;
			newRow = new RegradingOfGoodsTemplateItem()
				{
					NomenclatureOld = nomenclature
				};

			var selectNewNomenclature = new OrmReference(Repository.NomenclatureRepository.NomenclatureOfGoodsOnlyQuery());
			selectNewNomenclature.Mode = OrmReferenceMode.Select;
			selectNewNomenclature.TabName = "Выберите новую номенклатуру";
			selectNewNomenclature.ObjectSelected += SelectNewNomenclature_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, selectNewNomenclature);
		}

		void SelectNewNomenclature_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var nomenclature = e.Subject as Nomenclature;
			newRow.NomenclatureNew = nomenclature;
			TemplateUoW.Root.AddItem(newRow);
		}

		protected void OnButtonChangeOldClicked(object sender, EventArgs e)
		{
			var changeOldNomenclature = new OrmReference(Repository.NomenclatureRepository.NomenclatureOfGoodsOnlyQuery());
			changeOldNomenclature.TabName =	"Изменить старую номенклатуру";
			changeOldNomenclature.Mode = OrmReferenceMode.Select;
			changeOldNomenclature.ObjectSelected += ChangeOldNomenclature_ObjectSelected1;;
			MyTab.TabParent.AddSlaveTab(MyTab, changeOldNomenclature);
		}

		void ChangeOldNomenclature_ObjectSelected1 (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsTemplateItem>();
			if (row == null)
				return;

			var nomenclature = e.Subject as Nomenclature;
			row.NomenclatureOld = nomenclature;
		}


		protected void OnButtonChangeNewClicked(object sender, EventArgs e)
		{
			var changeNewNomenclature = new OrmReference(Repository.NomenclatureRepository.NomenclatureOfGoodsOnlyQuery());
			changeNewNomenclature.Mode = OrmReferenceMode.Select;
			changeNewNomenclature.TabName = "Изменить новую номенклатуру";
			changeNewNomenclature.ObjectSelected += ChangeNewNomenclature_ObjectSelected;;
			MyTab.TabParent.AddSlaveTab(MyTab, changeNewNomenclature);
		}

		void ChangeNewNomenclature_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsTemplateItem>();
			if (row == null)
				return;

			var nomenclature = e.Subject as Nomenclature;
			row.NomenclatureNew = nomenclature;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsTemplateItem>();
			if(row.Id != 0)
				TemplateUoW.Delete(row);
			TemplateUoW.Root.ObservableItems.Remove(row);
		}

		protected void OnYtreeviewItemsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if (args.Column.Title == "Старая номенклатура")
				buttonChangeOld.Click();
			if (args.Column.Title == "Новая номенклатура")
				buttonChangeNew.Click();
		}

	}
}

