using System;
using QSOrmProject;
using Gamma.GtkWidgets;
using Vodovoz.Domain.Documents;
using System.Collections.Generic;
using QSProjectsLib;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarLoadDocumentView : WidgetOnDialogBase
	{
		public CarLoadDocumentView()
		{
			this.Build();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<CarLoadDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("С/Н оборудования").AddTextRenderer (x => x.Equipment != null ? x.Equipment.Serial : String.Empty)
				.AddColumn("Кол-во на складе").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInStock))
				.AddColumn("В маршрутнике").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInRouteList))
				.AddColumn("В других отгрузках").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountLoaded))
				.AddColumn("Кол-во").AddNumericRenderer(x => x.Amount).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddSetter((w, x) => w.Digits = (uint)x.Nomenclature.Unit.Digits)
				.AddSetter((w, x) => w.Foreground = CalculateAmountColor(x))
				.Finish();

			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;

		}

		void YtreeviewItems_Selection_Changed (object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<CarLoadDocumentItem>();
			buttonDelete.Sensitive = selected != null;
		}

		private IUnitOfWorkGeneric<CarLoadDocument> documentUoW;

		public IUnitOfWorkGeneric<CarLoadDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<CarLoadDocumentItem> ();

				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
				DocumentUoW.Root.PropertyChanged += DocumentUoW_Root_PropertyChanged;
				UpdateButtonState();
			}
		}

		void UpdateButtonState()
		{
			bool routeExist = DocumentUoW.Root.RouteList != null;
			bool warehouseExist = DocumentUoW.Root.Warehouse != null;
			buttonFillAllItems.Sensitive = routeExist;
			buttonFillWarehouseItems.Sensitive = routeExist && warehouseExist;
		}

		void DocumentUoW_Root_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateButtonState();
		}

		string CalculateAmountColor(CarLoadDocumentItem item)
		{
			if (item.Amount > item.AmountInStock)
				return "red";
			if(item.Equipment == null)
			{
				if (item.AmountInRouteList < item.AmountLoaded + item.Amount)
					return "orange";
				if (item.AmountInRouteList == item.AmountLoaded + item.Amount)
					return "green";
				if (item.AmountInRouteList > item.AmountLoaded + item.Amount)
					return "blue";
			}
			else
			{
				if (1 < item.AmountLoaded + item.Amount)
					return "orange";
				if (1 == item.AmountLoaded + item.Amount)
					return "green";
				if (1 > item.AmountLoaded + item.Amount)
					return "blue";
			}
			return "black";
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			DocumentUoW.Root.ObservableItems.Remove (ytreeviewItems.GetSelectedObject<CarLoadDocumentItem>());
		}

		protected void OnButtonFillWarehouseItemsClicked(object sender, EventArgs e)
		{
			if(DocumentUoW.Root.Items.Count > 0)
			{
				if (!MessageDialogWorks.RunQuestionDialog("Список будет очищен. Продолжить?"))
					return;
			}
			DocumentUoW.Root.FillFromRouteList(DocumentUoW, true);
			DocumentUoW.Root.UpdateInRouteListAmount(DocumentUoW);
			if (DocumentUoW.Root.Warehouse != null)
				DocumentUoW.Root.UpdateStockAmount(DocumentUoW);
		}

		protected void OnButtonFillAllItemsClicked(object sender, EventArgs e)
		{
			if(DocumentUoW.Root.Items.Count > 0)
			{
				if (!MessageDialogWorks.RunQuestionDialog("Список будет очищен. Продолжить?"))
					return;
			}
			DocumentUoW.Root.FillFromRouteList(DocumentUoW, false);
			DocumentUoW.Root.UpdateInRouteListAmount(DocumentUoW);
			if (DocumentUoW.Root.Warehouse != null)
				DocumentUoW.Root.UpdateStockAmount(DocumentUoW);
		}
	}
}

