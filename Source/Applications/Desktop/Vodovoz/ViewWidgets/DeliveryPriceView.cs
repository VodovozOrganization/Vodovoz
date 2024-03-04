using System;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Logistic;
using Gamma.Utilities;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPriceView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		public DeliveryPriceView()
		{
			this.Build();
			//Отображается только у точек доставки без района
			ytreeviewPrices.CreateFluentColumnsConfig<DeliveryPriceRow>()
				.AddColumn("Количество").AddNumericRenderer(x => x.Amount)
				.AddColumn("Цена за бутыль").AddTextRenderer(x => x.Price)
				.Finish();
		}
		
		private DeliveryPriceNode deliveryPrice;
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

		private void ShowResults(DeliveryPriceNode deliveryPriceNode)
		{
			//ylabelSchedules.Markup = deliveryPriceNode.Schedule;
			yTxtWarehouses.Buffer.Text = deliveryPriceNode.GeographicGroups;
			GtkScrolledWindow.Visible = deliveryPriceNode.ByDistance;
			ytreeviewPrices.SetItemsSource(deliveryPriceNode.Prices);
			lblDistrict.LabelProp = deliveryPriceNode.DistrictName;
			wageTypeValueLabel.Text = deliveryPriceNode.WageDistrict + ",";
		}
	}
}
