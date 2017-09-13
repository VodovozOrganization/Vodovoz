﻿using System;
using QSOrmProject;
using Gamma.GtkWidgets;
using Vodovoz.Domain.Documents;
using System.Collections.Generic;
using QSProjectsLib;
using System.Linq;

namespace Vodovoz
{
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

		public void FillItemsByWarehouse()
		{
			if (buttonFillWarehouseItems.Sensitive)
				buttonFillWarehouseItems.Click();
		}

		public void SetButtonEditing(bool isEditing)
		{
			buttonFillAllItems.Sensitive = buttonFillWarehouseItems.Sensitive = isEditing;
		}

		void YtreeviewItems_Selection_Changed (object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<CarLoadDocumentItem>();
			buttonDelete.Sensitive = (selected != null && buttonFillAllItems.Sensitive == true);
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

			DocumentUoW.Root.FillFromRouteList(DocumentUoW, false);
			if(DocumentUoW.Root.Items.Any(i => i.Nomenclature.Warehouse == null)) {
				string str = "";
				foreach(var nomenclarure in DocumentUoW.Root.Items.Where(i => i.Nomenclature.Warehouse == null))
					str = string.Join("\n", nomenclarure.Nomenclature.Name);
				MessageDialogWorks.RunErrorDialog("В МЛ есть номенклатура не привязанная к складу.", str);
			}

			DocumentUoW.Root.FillFromRouteList(DocumentUoW, true);
			DocumentUoW.Root.UpdateAlreadyLoaded(DocumentUoW);
			if (DocumentUoW.Root.Warehouse != null)
			{
				DocumentUoW.Root.UpdateStockAmount(DocumentUoW);
				UpdateAmounts();
			}
		}

		protected void OnButtonFillAllItemsClicked(object sender, EventArgs e)
		{
			if(DocumentUoW.Root.Items.Count > 0)
			{
				if (!MessageDialogWorks.RunQuestionDialog("Список будет очищен. Продолжить?"))
					return;
			}
			DocumentUoW.Root.FillFromRouteList(DocumentUoW, false);

			var items = DocumentUoW.Root.Items;
			string errorNomenclatures = string.Empty;
			foreach (var item in items)
			{
				if (item.Nomenclature.Unit == null)
					errorNomenclatures += string.Format("{0} код {1}{2}",
						item.Nomenclature.Name, item.Nomenclature.Id, Environment.NewLine);
			}
			if (!string.IsNullOrEmpty(errorNomenclatures))
			{
				errorNomenclatures = "Не указаны единицы измерения для следующих номенклатур:"
					+ Environment.NewLine + errorNomenclatures;
				MessageDialogWorks.RunErrorDialog(errorNomenclatures);
				DocumentUoW.Root.Items.Clear();
				return;
			}	

			DocumentUoW.Root.UpdateAlreadyLoaded(DocumentUoW);
			if (DocumentUoW.Root.Warehouse != null)
			{
				DocumentUoW.Root.UpdateStockAmount(DocumentUoW);
				UpdateAmounts();
			}
		}

		public void UpdateAmounts()
		{
			foreach(var item in DocumentUoW.Root.Items)
			{
				item.Amount = item.AmountInRouteList - item.AmountLoaded;
				if (item.Amount > item.AmountInStock)
					item.Amount = item.AmountInStock;
			}
		}

	}
}

