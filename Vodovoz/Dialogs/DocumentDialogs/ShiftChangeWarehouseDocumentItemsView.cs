using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Dialogs.DocumentDialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ShiftChangeWarehouseDocumentItemsView : WidgetOnDialogBase
	{

		public ShiftChangeWarehouseDocumentItemsView()
		{
			this.Build();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<ShiftChangeWarehouseDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Кол-во в учёте").AddTextRenderer(x => x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB) : x.AmountInDB.ToString())
				.AddColumn("Кол-во по факту").AddNumericRenderer(x => x.AmountInFact).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddSetter((w, x) => w.Digits = (x.Nomenclature.Unit != null ? (uint)x.Nomenclature.Unit.Digits : 1))
				.AddColumn("Разница").AddTextRenderer(x => x.Difference != 0 && x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.Difference) : String.Empty)
				.AddSetter((w, x) => w.Foreground = x.Difference < 0 ? "red" : "blue")
				.AddColumn("Сумма ущерба").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Что произошло").AddTextRenderer(x => x.Comment).Editable()
				.Finish();
		}

		private IUnitOfWorkGeneric<ShiftChangeWarehouseDocument> documentUoW;

		public IUnitOfWorkGeneric<ShiftChangeWarehouseDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if(documentUoW == value)
					return;
				documentUoW = value;
				if(DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<ShiftChangeWarehouseDocumentItem>();

				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
				UpdateButtonState();
				if(DocumentUoW.Root.Warehouse != null && DocumentUoW.Root.Items.Count == 0)
					buttonFillItems.Click();
				DocumentUoW.Root.PropertyChanged += DocumentUoW_Root_PropertyChanged;
			}
		}

		void DocumentUoW_Root_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == DocumentUoW.Root.GetPropertyName(x => x.Warehouse)) {
				if(DocumentUoW.Root.Warehouse != null)
					buttonFillItems.Click();
				UpdateButtonState();
			}

		}

		protected void OnButtonFillItemsClicked(object sender, EventArgs e)
		{
			if(DocumentUoW.Root.Items.Count == 0)
				DocumentUoW.Root.FillItemsFromStock(DocumentUoW);
			else
				DocumentUoW.Root.UpdateItemsFromStock(DocumentUoW);
			UpdateButtonState();
		}

		private void UpdateButtonState()
		{
			buttonFillItems.Sensitive = DocumentUoW.Root.Warehouse != null;
			if(DocumentUoW.Root.Items.Count == 0)
				buttonFillItems.Label = "Заполнить по складу";
			else
				buttonFillItems.Label = "Обновить остатки";
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var nomenclatureSelectDlg = new OrmReference(Repository.NomenclatureRepository.NomenclatureOfGoodsOnlyQuery());
			nomenclatureSelectDlg.Mode = OrmReferenceMode.Select;
			nomenclatureSelectDlg.ObjectSelected += NomenclatureSelectDlg_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, nomenclatureSelectDlg);
		}

		void NomenclatureSelectDlg_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var nomenclature = e.Subject as Nomenclature;
			if(DocumentUoW.Root.Items.Any(x => x.Nomenclature.Id == nomenclature.Id))
				return;

			DocumentUoW.Root.AddItem(nomenclature, 0, 0);
		}
	}
}