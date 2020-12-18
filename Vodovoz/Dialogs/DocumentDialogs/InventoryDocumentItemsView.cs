using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QS.Tdi;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Store;
using Gtk;
using Gdk;
using Vodovoz.Parameters;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class InventoryDocumentItemsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		InventoryDocumentItem FineEditItem;

		INomenclatureRepository nomenclatureRepository { get; } = new NomenclatureRepository(new NomenclatureParametersProvider());

		public InventoryDocumentItemsView()
		{
			this.Build();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<InventoryDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => GetNomenclatureName(x.Nomenclature), useMarkup: true)
			    .AddColumn("Кол-во в учёте").AddTextRenderer(x => x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB) : x.AmountInDB.ToString())
				.AddColumn("Кол-во по факту").AddNumericRenderer(x => x.AmountInFact).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddSetter((w, x) => w.Digits = ( x.Nomenclature.Unit!= null ? (uint)x.Nomenclature.Unit.Digits : 1)) 
				.AddColumn("Разница").AddTextRenderer(x => x.Difference != 0 && x.Nomenclature.Unit!= null ? x.Nomenclature.Unit.MakeAmountShortStr(x.Difference) : String.Empty)
				.AddSetter((w, x) => w.Foreground = x.Difference < 0 ? "red" : "blue")
				.AddColumn("Сумма ущерба").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Штраф").AddTextRenderer(x => x.Fine != null ? x.Fine.Description : String.Empty)
				.AddColumn("Что произошло").AddTextRenderer(x => x.Comment).Editable()
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) => {
					Color color = new Color(255, 255, 255);
					if(nomenclaturesWithDiscrepancies.Any(x => x.Id == node.Nomenclature.Id)) {
						color = new Color(255, 125, 125);
					}
					cell.CellBackgroundGdk = color;
				})
				.Finish();

			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;


		}

		private string GetNomenclatureName(Nomenclature nomenclature)
		{
			if(nomenclaturesWithDiscrepancies.Any(x => x.Id == nomenclature.Id)) {
				return $"<b>{nomenclature.Name}</b>";
			}
			return nomenclature.Name;
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
			buttonDeleteFine.Sensitive = selected != null && selected.Fine != null;
		}

		IEnumerable<Nomenclature> nomenclaturesWithDiscrepancies = new List<Nomenclature>();

		private IUnitOfWorkGeneric<InventoryDocument> documentUoW;

		public IUnitOfWorkGeneric<InventoryDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<InventoryDocumentItem> ();

				FillDiscrepancies();

				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
				UpdateButtonState();
				if (DocumentUoW.Root.Warehouse != null && DocumentUoW.Root.Items.Count == 0)
					buttonFillItems.Click();
				DocumentUoW.Root.PropertyChanged += DocumentUoW_Root_PropertyChanged;
			}
		}

		void DocumentUoW_Root_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			List<string> propertys = new List<string> {
				DocumentUoW.Root.GetPropertyName(x => x.Warehouse),
			};

			if(propertys.Contains(e.PropertyName))
			{
				if (DocumentUoW.Root.Warehouse != null)
					buttonFillItems.Click();
				UpdateButtonState();
			}
				
		}

		private void FillDiscrepancies()
		{
			if(DocumentUoW.Root.Warehouse != null && DocumentUoW.Root.Warehouse.Id > 0) {
				var warehouseRepository = new WarehouseRepository();
				nomenclaturesWithDiscrepancies = warehouseRepository.GetDiscrepancyNomenclatures(UoW, DocumentUoW.Root.Warehouse.Id);
			}
		}

		private void UpdateButtonState()
		{
			buttonFillItems.Sensitive = DocumentUoW.Root.Warehouse != null;
			if (DocumentUoW.Root.Items.Count == 0)
				buttonFillItems.Label = "Заполнить по складу";
			else
				buttonFillItems.Label = "Обновить остатки";
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var nomenclatureSelectDlg = new OrmReference(nomenclatureRepository.NomenclatureOfGoodsOnlyQuery());
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
			//Мы здесь не можем выполнить просто рефреш, так как если удалить сотрудника из штрафа, получаем эксепшен.
			int id = FineEditItem.Fine.Id;
			DocumentUoW.Session.Evict(FineEditItem.Fine);
			FineEditItem.Fine = DocumentUoW.GetById<Fine>(id);
		}

		protected void OnButtonDeleteFineClicked(object sender, EventArgs e)
		{
			var item = ytreeviewItems.GetSelectedObject<InventoryDocumentItem>();
			DocumentUoW.Delete(item.Fine);
			item.Fine = null;
			YtreeviewItems_Selection_Changed(null, EventArgs.Empty);
		}
	}
}

