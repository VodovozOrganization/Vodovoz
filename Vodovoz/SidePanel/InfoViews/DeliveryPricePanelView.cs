using Vodovoz.Domain.Client;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.Tools.Logistic;
using System;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPricePanelView : Gtk.Bin, IPanelView
	{
		DeliveryPoint DeliveryPoint { get; set; }

		public DeliveryPricePanelView()
		{
			this.Build();
		}

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel {
			get {
				return DeliveryPoint != null;
			}
		}

		public void OnCurrentObjectChanged(object changedObject)
		{
			var deliveryPoint = changedObject as DeliveryPoint;
			if(deliveryPoint != null) {
				DeliveryPoint = deliveryPoint;
				Refresh();
			}
		}

		public void Refresh()
		{
			DeliveryPoint = (InfoProvider as IDeliveryPointInfoProvider)?.DeliveryPoint;
			if(DeliveryPoint == null) {
				return;
			}

			var deliveryPrice = DeliveryPriceCalculator.Calculate(DeliveryPoint);
			labelError.Visible = deliveryPrice.HasError;
			labelError.Markup = String.Format("<span foreground=\"red\"><b>{0}</b></span>", deliveryPrice.ErrorMessage);
			deliverypriceview.Visible = !deliveryPrice.HasError;
			deliverypriceview.DeliveryPrice = deliveryPrice;
		}

		#endregion
	}
}
