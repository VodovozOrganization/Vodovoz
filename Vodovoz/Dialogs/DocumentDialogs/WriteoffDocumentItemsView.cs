using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WriteoffDocumentItemsView : WidgetOnDialogBase
	{
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
					.SetDisplayFunc (DomainHelper.GetObjectTilte).Editing ()
					.FillItems (Repository.CullingCategoryRepository.All (DocumentUoW))
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
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}
				
			var filter = new StockBalanceFilter (UnitOfWorkFactory.CreateWithoutRoot ());
			filter.SetAndRefilterAtOnce(x => x.RestrictWarehouse = DocumentUoW.Root.WriteoffWarehouse);

			ReferenceRepresentation SelectDialog = new ReferenceRepresentation (new ViewModel.StockBalanceVM (filter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.None;
			SelectDialog.ObjectSelected += NomenclatureSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void NomenclatureSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var nomenctature = DocumentUoW.GetById<Nomenclature> (e.ObjectId);
			DocumentUoW.Root.AddItem(nomenctature, 0, (e.VMNode as ViewModel.StockBalanceVMNode).Amount);
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

