using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RegradingOfGoodsDocumentItemsView : WidgetOnDialogBase
	{
		RegradingOfGoodsDocumentItem newRow;
		RegradingOfGoodsDocumentItem FineEditItem;

		public RegradingOfGoodsDocumentItemsView()
		{
			this.Build();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<RegradingOfGoodsDocumentItem>()
				.AddColumn("Старая номенклатура").AddTextRenderer(x => x.NomenclatureOld.Name)
				.AddColumn("Кол-во на складе").AddTextRenderer(x => x.NomenclatureOld.Unit.MakeAmountShortStr(x.AmountInStock))
				.AddColumn("Новая номенклатура").AddTextRenderer(x => x.NomenclatureNew.Name)
				.AddColumn("Кол-во пересортицы").AddNumericRenderer(x => x.Amount).Editing()
				.AddSetter(
					(w, x) => w.Adjustment = new Gtk.Adjustment(
						0,
						0,
						GetMaxValueForAdjustmentSetting(x),
						1,
						10,
						10
					)
				)
				.AddSetter((w, x) => w.Digits = (uint)x.NomenclatureNew.Unit.Digits)
				.AddColumn("Сумма ущерба").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Штраф").AddTextRenderer(x => x.Fine != null ? x.Fine.Description : String.Empty)
				.AddColumn("Что произошло").AddTextRenderer(x => x.Comment).Editable()
				.Finish();
			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;
		}

		double GetMaxValueForAdjustmentSetting(RegradingOfGoodsDocumentItem item){
			if(item.NomenclatureOld.Category == NomenclatureCategory.bottle
			   && item.NomenclatureNew.Category == NomenclatureCategory.water)
				return 39;
			return (double)item.AmountInStock;
		}

		void YtreeviewItems_Selection_Changed (object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		private IUnitOfWorkGeneric<RegradingOfGoodsDocument> documentUoW;

		public IUnitOfWorkGeneric<RegradingOfGoodsDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<RegradingOfGoodsDocumentItem> ();

				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
				UpdateButtonState();
				DocumentUoW.Root.PropertyChanged += DocumentUoW_Root_PropertyChanged;
				if (!DocumentUoW.IsNew)
					LoadStock();
			}
		}

		private void UpdateButtonState()
		{
			var selected = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			buttonChangeNew.Sensitive = buttonDelete.Sensitive = selected != null;
			buttonChangeOld.Sensitive = selected != null && DocumentUoW.Root.Warehouse != null;
			buttonAdd.Sensitive = buttonFromTemplate.Sensitive = DocumentUoW.Root.Warehouse != null;

			buttonFine.Sensitive = selected != null;
			if(selected != null)
			{
				if (selected.Fine != null)
					buttonFine.Label = "Изменить штраф";
				else
					buttonFine.Label = "Добавить штраф";
			}
			buttonDeleteFine.Sensitive = selected != null && selected.Fine != null;
		}

		void DocumentUoW_Root_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == DocumentUoW.Root.GetPropertyName(x => x.Warehouse))
				UpdateButtonState();
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var filter = new StockBalanceFilter();
			filter.RestrictWarehouse = DocumentUoW.Root.Warehouse;
			var selectOldNomenclature = new ReferenceRepresentation(new ViewModel.StockBalanceVM(filter),
				"Выберите номенклатуру на замену");
			selectOldNomenclature.Mode = OrmReferenceMode.Select;
			selectOldNomenclature.ObjectSelected += SelectOldNomenclature_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, selectOldNomenclature);
		}

		void SelectOldNomenclature_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var nomenclature = DocumentUoW.GetById<Nomenclature>(e.ObjectId);
			var VMNode = e.VMNode as ViewModel.StockBalanceVMNode;
			newRow = new RegradingOfGoodsDocumentItem()
			{
				NomenclatureOld = nomenclature,
				AmountInStock = VMNode.Amount
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
			DocumentUoW.Root.AddItem(newRow);
		}

		private void LoadStock()
		{
			var nomenclatureIds = DocumentUoW.Root.Items.Select(x => x.NomenclatureOld.Id).ToArray();
			var inStock = Repository.StockRepository.NomenclatureInStock(DocumentUoW, DocumentUoW.Root.Warehouse.Id, 
				nomenclatureIds, DocumentUoW.Root.TimeStamp);

			foreach(var item in DocumentUoW.Root.Items)
			{
				item.AmountInStock = inStock[item.NomenclatureOld.Id];
			}
		}

		protected void OnButtonChangeOldClicked(object sender, EventArgs e)
		{
			var filter = new StockBalanceFilter();
			filter.RestrictWarehouse = DocumentUoW.Root.Warehouse;
			var changeOldNomenclature = new ReferenceRepresentation(new ViewModel.StockBalanceVM(filter),
				"Изменить старую номенклатуру");
			changeOldNomenclature.Mode = OrmReferenceMode.Select;
			changeOldNomenclature.ObjectSelected += ChangeOldNomenclature_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, changeOldNomenclature);
		}

		void ChangeOldNomenclature_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			if (row == null)
				return;

			var nomenclature = DocumentUoW.GetById<Nomenclature>(e.ObjectId);
			var VMNode = e.VMNode as ViewModel.StockBalanceVMNode;
			row.NomenclatureOld = nomenclature;
			row.AmountInStock = VMNode.Amount;
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
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			if (row == null)
				return;
			
			var nomenclature = e.Subject as Nomenclature;
			row.NomenclatureNew = nomenclature;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			if (row.WarehouseIncomeOperation.Id == 0)
				DocumentUoW.Delete(row.WarehouseIncomeOperation);
			if (row.WarehouseWriteOffOperation.Id == 0)
				DocumentUoW.Delete(row.WarehouseWriteOffOperation);
			if(row.Id != 0)
				DocumentUoW.Delete(row);
			DocumentUoW.Root.ObservableItems.Remove(row);
		}

		protected void OnYtreeviewItemsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if (args.Column.Title == "Старая номенклатура")
				buttonChangeOld.Click();
			if (args.Column.Title == "Новая номенклатура")
				buttonChangeNew.Click();
		}

		protected void OnButtonFineClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			FineDlg fineDlg;
			if (selected.Fine != null)
			{
				fineDlg = new FineDlg(selected.Fine);
				fineDlg.EntitySaved += FineDlgExist_EntitySaved;
			}
			else
			{
				fineDlg = new FineDlg("Недостача");
				fineDlg.EntitySaved += FineDlgNew_EntitySaved;
			}
			fineDlg.Entity.TotalMoney = selected.SumOfDamage;
			FineEditItem = selected;
			MyTab.TabParent.AddSlaveTab(MyTab, fineDlg);
		}

		void FineDlgNew_EntitySaved (object sender, EntitySavedEventArgs e)
		{
			FineEditItem.Fine = e.Entity as Fine;
			FineEditItem = null;
		}

		void FineDlgExist_EntitySaved (object sender, EntitySavedEventArgs e)
		{
			DocumentUoW.Session.Refresh(FineEditItem.Fine);
		}

		protected void OnButtonDeleteFineClicked(object sender, EventArgs e)
		{
			var item = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			DocumentUoW.Delete(item.Fine);
			item.Fine = null;
			UpdateButtonState();
		}

		protected void OnButtonFromTemplateClicked(object sender, EventArgs e)
		{
			var selectTemplate = new OrmReference(typeof(RegradingOfGoodsTemplate));
			selectTemplate.Mode = OrmReferenceMode.Select;
			selectTemplate.ObjectSelected += SelectTemplate_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, selectTemplate);
		}

		void SelectTemplate_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if (DocumentUoW.Root.Items.Count > 0)
			{
				if (MessageDialogWorks.RunQuestionDialog("Текущий список будет очищен. Продолжить?"))
					DocumentUoW.Root.ObservableItems.Clear();
				else
					return;
			}

			var template = DocumentUoW.GetById<RegradingOfGoodsTemplate>((e.Subject as RegradingOfGoodsTemplate).Id);
			foreach(var item in template.Items)
			{
				DocumentUoW.Root.AddItem(new RegradingOfGoodsDocumentItem()
					{
						NomenclatureNew = item.NomenclatureNew,
						NomenclatureOld = item.NomenclatureOld
					});
			}
			LoadStock();
		}
	}
}

