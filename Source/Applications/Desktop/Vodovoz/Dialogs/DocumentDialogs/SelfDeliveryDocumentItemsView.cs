using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelfDeliveryDocumentItemsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		public SelfDeliveryDocumentItemsView()
		{
			this.Build();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<SelfDeliveryDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Кол-во на складе").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInStock))
				.AddColumn("В заказе").AddTextRenderer(x => GetItemsCount(x))
				.AddColumn("Уже отгружено").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountUnloaded))
				.AddColumn("Количество").AddNumericRenderer(x => x.Amount).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddSetter((w, x) => w.Digits = (uint)x.Nomenclature.Unit.Digits)
				.AddSetter((w, x) => w.Foreground = CalculateAmountColor(x))
				.AddColumn("")
				.Finish();

			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;
		}

		void YtreeviewItems_Selection_Changed(object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<SelfDeliveryDocumentItem>();
			buttonDelete.Sensitive = selected != null;
		}

		string GetItemsCount(SelfDeliveryDocumentItem item)
		{
			decimal cnt = item.Document.GetNomenclaturesCountInOrder(item.Nomenclature);
			return item.Nomenclature.Unit.MakeAmountShortStr(cnt);
		}

		private IUnitOfWorkGeneric<SelfDeliveryDocument> documentUoW;

		public IUnitOfWorkGeneric<SelfDeliveryDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if(documentUoW == value)
					return;
				documentUoW = value;
				if(DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<SelfDeliveryDocumentItem>();

				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
			}
		}

		string CalculateAmountColor(SelfDeliveryDocumentItem item)
		{
			if(item.Amount > item.AmountInStock)
				return "red";

			var cnt = item.Document.GetNomenclaturesCountInOrder(item.Nomenclature);
			if(cnt < item.AmountUnloaded + item.Amount)
				return "orange";
			if(cnt == item.AmountUnloaded + item.Amount)
				return "green";
			if(cnt > item.AmountUnloaded + item.Amount)
				return "blue";

			return "black";
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			DocumentUoW.Root.ObservableItems.Remove(ytreeviewItems.GetSelectedObject<SelfDeliveryDocumentItem>());
		}
	}
}

