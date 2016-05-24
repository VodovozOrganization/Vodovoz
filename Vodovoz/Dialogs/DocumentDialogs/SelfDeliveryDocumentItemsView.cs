using System;
using QSOrmProject;
using Gamma.GtkWidgets;
using Vodovoz.Domain.Documents;
using System.Collections.Generic;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelfDeliveryDocumentItemsView : WidgetOnDialogBase
	{
		public SelfDeliveryDocumentItemsView()
		{
			this.Build();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<SelfDeliveryDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("С/Н оборудования").AddTextRenderer (x => x.Equipment != null ? x.Equipment.Serial : String.Empty)
				.AddColumn("Кол-во на складе").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInStock))
				.AddColumn("В заказе").AddTextRenderer(x => (x.OrderItem != null || x.Equipment != null) ? x.Nomenclature.Unit.MakeAmountShortStr(x.OrderItem != null ? x.OrderItem.Count : 1) : "дайте щелбан программисту")
				.AddColumn("Уже отгружено").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountUnloaded))
				.AddColumn("Кол-во").AddNumericRenderer(x => x.Amount).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddSetter((w, x) => w.Digits = (uint)x.Nomenclature.Unit.Digits)
				.AddSetter((w, x) => w.Foreground = CalculateAmountColor(x))
				.Finish();

			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;

		}

		void YtreeviewItems_Selection_Changed (object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<SelfDeliveryDocumentItem>();
			buttonDelete.Sensitive = selected != null;
		}

		private IUnitOfWorkGeneric<SelfDeliveryDocument> documentUoW;

		public IUnitOfWorkGeneric<SelfDeliveryDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<SelfDeliveryDocumentItem> ();

				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
			}
		}

		string CalculateAmountColor(SelfDeliveryDocumentItem item)
		{
			if (item.Amount > item.AmountInStock)
				return "red";
			if(item.OrderItem != null)
			{
				if (item.OrderItem.Count < item.AmountUnloaded + item.Amount)
					return "orange";
				if (item.OrderItem.Count == item.AmountUnloaded + item.Amount)
					return "green";
				if (item.OrderItem.Count > item.AmountUnloaded + item.Amount)
					return "blue";
			}
			if(item.Equipment != null)
			{
				if (1 < item.AmountUnloaded + item.Amount)
					return "orange";
				if (1 == item.AmountUnloaded + item.Amount)
					return "green";
				if (1 > item.AmountUnloaded + item.Amount)
					return "blue";
			}
			return "black";
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			DocumentUoW.Root.ObservableItems.Remove (ytreeviewItems.GetSelectedObject<SelfDeliveryDocumentItem>());
		}
	}
}

