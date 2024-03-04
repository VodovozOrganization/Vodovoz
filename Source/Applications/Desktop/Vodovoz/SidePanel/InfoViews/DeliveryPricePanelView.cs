using System.ComponentModel;
using Vodovoz.Domain.Client;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.Tools.Logistic;
using IDeliveryPointInfoProvider = Vodovoz.ViewModels.Infrastructure.InfoProviders.IDeliveryPointInfoProvider;

namespace Vodovoz.SidePanel.InfoViews
{
	[ToolboxItem(true)]
	public partial class DeliveryPricePanelView : Gtk.Bin, IPanelView
	{
		private DeliveryPoint _deliveryPoint;

		public DeliveryPricePanelView()
		{
			Build();
		}

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => _deliveryPoint != null;

		public void OnCurrentObjectChanged(object changedObject)
		{
			if(changedObject is DeliveryPoint deliveryPoint) {
				_deliveryPoint = deliveryPoint;
				Refresh();
			}
		}

		public void Refresh()
		{
			_deliveryPoint = (InfoProvider as IDeliveryPointInfoProvider)?.DeliveryPoint;
			if(_deliveryPoint == null || string.IsNullOrWhiteSpace(_deliveryPoint?.City)) {
				return;
			}

			var deliveryPrice = DeliveryPriceCalculator.Calculate(_deliveryPoint);
			labelError.Visible = deliveryPrice.HasError;
			labelError.Markup = $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\"><b>{deliveryPrice.ErrorMessage}</b></span>";

			deliverypriceview.Visible = !deliveryPrice.HasError;
			deliverypriceview.DeliveryPrice = deliveryPrice;
		}

		#endregion IPanelView implementation
	}
}
