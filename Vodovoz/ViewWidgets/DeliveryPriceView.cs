using System;
using QSOrmProject;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPriceView : WidgetOnDialogBase
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
			if(deliveryPriceNode.HaveError) {
				OnError?.Invoke(this, deliveryPriceNode.ErrorMessage);
			}
			ylabelDistance.LabelProp = deliveryPriceNode.Distance;
			labelPrice.LabelProp = deliveryPriceNode.Price;
			labelMinBottles.LabelProp = deliveryPriceNode.MinBottles;
			labelSchedule.LabelProp = deliveryPriceNode.Schedule;
			hbox1.Visible = deliveryPriceNode.ByDistance;
			label2.Visible = labelPrice.Visible = deliveryPriceNode.WithPrice;
			ytreeviewPrices.SetItemsSource<DeliveryPriceRow>(deliveryPriceNode.Prices);
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
