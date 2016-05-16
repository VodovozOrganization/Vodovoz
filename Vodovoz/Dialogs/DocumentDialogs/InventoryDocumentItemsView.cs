using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class InventoryDocumentItemsView : WidgetOnDialogBase
	{
		InventoryDocumentItem FineEditItem;

		public InventoryDocumentItemsView()
		{
			this.Build();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<InventoryDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Кол-во в учёте").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB))
				.AddColumn("Кол-во по факту").AddNumericRenderer(x => x.AmountInFact).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddSetter((w, x) => w.Digits = (uint)x.Nomenclature.Unit.Digits)
				.AddColumn("Разница").AddTextRenderer(x => x.Difference != 0 ? x.Nomenclature.Unit.MakeAmountShortStr(x.Difference) : String.Empty)
				.AddSetter((w, x) => w.Foreground = x.Difference < 0 ? "red" : "blue")
				.AddColumn("Сумма ущерба").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Штраф").AddTextRenderer(x => x.Fine != null ? x.Fine.Description : String.Empty)
				.AddColumn("Что произошло").AddTextRenderer(x => x.Comment).Editable()
				.Finish();

			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;
		}

		void YtreeviewItems_Selection_Changed (object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<InventoryDocumentItem>();
			buttonFine.Sensitive = selected != null;
			if(selected != null)
			{
				if (selected.Fine != null)
					buttonFine.Label = "Изменить штраф";
				else
					buttonFine.Label = "Добавить штраф";
			}
		}

		private IUnitOfWorkGeneric<InventoryDocument> documentUoW;

		public IUnitOfWorkGeneric<InventoryDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<InventoryDocumentItem> ();
				
				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
				UpdateButtonState();
				DocumentUoW.Root.PropertyChanged += DocumentUoW_Root_PropertyChanged;
			}
		}

		void DocumentUoW_Root_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == DocumentUoW.Root.GetPropertyName(x => x.Warehouse))
				UpdateButtonState();
		}

		protected void OnButtonFillItemsClicked(object sender, EventArgs e)
		{
			DocumentUoW.Root.FillItemsFromStock(DocumentUoW);
			(MyOrmDialog as InventoryDocumentDlg).SetSensitiveWarehouse(false);
		}

		private void UpdateButtonState()
		{
			buttonFillItems.Sensitive = DocumentUoW.IsNew && DocumentUoW.Root.Warehouse != null;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var nomenclatureSelectDlg = new OrmReference(Repository.NomenclatureRepository.NomenclatureOfGoodsOnlyQuery());
			nomenclatureSelectDlg.Mode = OrmReferenceMode.Select;
			nomenclatureSelectDlg.ObjectSelected += NomenclatureSelectDlg_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, nomenclatureSelectDlg);
		}

		void NomenclatureSelectDlg_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var nomenclature = e.Subject as Nomenclature;
			if (DocumentUoW.Root.Items.Any(x => x.Nomenclature.Id == nomenclature.Id))
				return;

			DocumentUoW.Root.AddItem(nomenclature, 0, 0);
		}

		protected void OnButtonFineClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<InventoryDocumentItem>();
			FineDlg fineDlg;
			if (selected.Fine != null)
			{
				fineDlg = new FineDlg(selected.Fine);
				fineDlg.EntitySaved += FineDlgExist_EntitySaved;
			}
			else
			{
				fineDlg = new FineDlg();
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

	}
}

