using System;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPriceView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		DeliveryPriceNode deliveryPrice;
		public DeliveryPriceNode DeliveryPrice {
			get {
				if(deliveryPrice == null) {
					deliveryPrice = new DeliveryPriceNode();
				}
				return deliveryPrice;
			}
			set {
				deliveryPrice = value;
				ShowResults(DeliveryPrice);
			}
		}

		public event EventHandler<string> OnError;

		void ShowResults(DeliveryPriceNode deliveryPriceNode)
		{
			labelPrice.LabelProp = deliveryPriceNode.Price;
			labelMinBottles.LabelProp = deliveryPriceNode.MinBottles;
			ytextviewSchedule.Buffer.Text = deliveryPriceNode.Schedule;
			yTxtWarehouses.Buffer.Text = deliveryPriceNode.WarehousesList;
			hboxTreeView.Visible = deliveryPriceNode.ByDistance;
			label2.Visible = labelPrice.Visible = hbox5.Visible = deliveryPriceNode.WithPrice;
			ytreeviewPrices.SetItemsSource<DeliveryPriceRow>(deliveryPriceNode.Prices);
			lblDistrict.LabelProp = deliveryPriceNode.DistrictName;
		}

		public DeliveryPriceView()
		{
			this.Build();

			ytreeviewPrices.CreateFluentColumnsConfig<DeliveryPriceRow>()
						   .AddColumn("Количество").AddNumericRenderer(x => x.Amount)
						   .AddColumn("Цена за бутыль").AddTextRenderer(x => x.Price)
						   .Finish();

		}


	}
}
