using Vodovoz.Domain.Client;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.Tools.Logistic;
using IDeliveryPointInfoProvider = Vodovoz.ViewModels.Infrastructure.InfoProviders.IDeliveryPointInfoProvider;

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

		public bool VisibleOnPanel => DeliveryPoint != null;

		public void OnCurrentObjectChanged(object changedObject)
		{
			if(changedObject is DeliveryPoint deliveryPoint) {
				DeliveryPoint = deliveryPoint;
				Refresh();
			}
		}

		public void Refresh()
		{
			DeliveryPoint = (InfoProvider as IDeliveryPointInfoProvider)?.DeliveryPoint;
			if(DeliveryPoint == null || string.IsNullOrWhiteSpace(DeliveryPoint?.City)) {
				return;
			}

			var deliveryPrice = DeliveryPriceCalculator.Calculate(DeliveryPoint);
			labelError.Visible = deliveryPrice.HasError;
			labelError.Markup = $"<span foreground=\"{GdkColors.Red.ToHtmlColor()}\"><b>{deliveryPrice.ErrorMessage}</b></span>";
			deliverypriceview.Visible = !deliveryPrice.HasError;
			deliverypriceview.DeliveryPrice = deliveryPrice;
		}

		#endregion
	}
}
