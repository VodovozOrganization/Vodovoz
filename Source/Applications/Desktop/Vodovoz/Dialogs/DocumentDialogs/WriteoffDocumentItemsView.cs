﻿using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Tdi;
using QSProjectsLib;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WriteoffDocumentItemsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private readonly ICullingCategoryRepository _cullingCategoryRepository = new CullingCategoryRepository();
		GenericObservableList<WriteoffDocumentItem> items;
		WriteoffDocumentItem FineEditItem;

		static Logger logger = LogManager.GetCurrentClassLogger ();

		public WriteoffDocumentItemsView ()
		{
			this.Build ();
			treeItemsList.Selection.Changed += OnSelectionChanged;
		}

		private IUnitOfWorkGeneric<WriteoffDocument> documentUoW;

		public IUnitOfWorkGeneric<WriteoffDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<WriteoffDocumentItem> ();
				items = DocumentUoW.Root.ObservableItems;
				treeItemsList.ColumnsConfig = ColumnsConfigFactory.Create<WriteoffDocumentItem>()
					.AddColumn ("Наименование").AddTextRenderer (i => i.Name)
					.AddColumn ("С/Н оборудования").AddTextRenderer (i => i.EquipmentString)
					.AddColumn ("Количество")
					.AddNumericRenderer (i => i.Amount).Editing ().WidthChars (10)
					.AddSetter ((c, i) => c.Digits = (uint)i.Nomenclature.Unit.Digits)
					.AddSetter((c, i) => c.Editable = i.CanEditAmount)
					.AddSetter ((c, i) => c.Adjustment = new Adjustment(0, 0, (double)i.AmountOnStock, 1, 100, 0))
					.AddTextRenderer (i => i.Nomenclature.Unit.Name, false)
					.AddColumn ("Причина выбраковки").AddComboRenderer (i => i.CullingCategory)
					.SetDisplayFunc (DomainHelper.GetTitle).Editing ()
					.FillItems (_cullingCategoryRepository.All(DocumentUoW))
					.AddColumn("Сумма ущерба").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
					.AddColumn("Штраф").AddTextRenderer(x => x.Fine != null ? x.Fine.Description : String.Empty)
					.AddColumn("Выявлено в процессе").AddTextRenderer(i => i.Comment).Editable()
					.Finish ();

				treeItemsList.ItemsDataSource = items;
			}
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeItemsList.GetSelectedObjects () [0] as WriteoffDocumentItem);
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			var selected = treeItemsList.GetSelectedObject<WriteoffDocumentItem>();
			buttonDelete.Sensitive = treeItemsList.Selection.CountSelectedRows () > 0;
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

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = DialogHelper.FindParentTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			NomenclatureStockFilterViewModel filter = new NomenclatureStockFilterViewModel(new WarehouseJournalFactory());
			filter.RestrictWarehouse = DocumentUoW.Root.WriteoffWarehouse;

			NomenclatureStockBalanceJournalViewModel vm = new NomenclatureStockBalanceJournalViewModel(
				filter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			);

			vm.SelectionMode = JournalSelectionMode.Single;
			vm.OnEntitySelectedResult += (s, ea) => {
				var selectedNode = ea.SelectedNodes.Cast<NomenclatureStockJournalNode>().FirstOrDefault();
				if(selectedNode == null) {
					return;
				}
				var nomenclature = DocumentUoW.GetById<Nomenclature>(selectedNode.Id);
				if(DocumentUoW.Root.Items.Any(x => x.Nomenclature.Id == nomenclature.Id)) {
					return;
				}
				DocumentUoW.Root.AddItem(nomenclature, 0, selectedNode.StockAmount);
			};

			mytab.TabParent.AddSlaveTab (mytab, vm);
		}

		protected void OnButtonFineClicked(object sender, EventArgs e)
		{
			var selected = treeItemsList.GetSelectedObject<WriteoffDocumentItem>();
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
			var item = treeItemsList.GetSelectedObject<WriteoffDocumentItem>();
			DocumentUoW.Delete(item.Fine);
			item.Fine = null;
			OnSelectionChanged(null, EventArgs.Empty);
		}
	}
}

