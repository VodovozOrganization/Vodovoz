using System;
using System.ComponentModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.Tools.Logistic;
using VodovozBusiness.Domain.Service;
using IDeliveryPointInfoProvider = Vodovoz.ViewModels.Infrastructure.InfoProviders.IDeliveryPointInfoProvider;

namespace Vodovoz.SidePanel.InfoViews
{
	[ToolboxItem(true)]
	public partial class DeliveryPricePanelView : Gtk.Bin, IPanelView
	{
		private readonly IDeliveryPriceCalculator _deliveryPriceCalculator;
		private DeliveryPoint _deliveryPoint;
		private OrderAddressType? _orderAddressType;

		public DeliveryPricePanelView(IDeliveryPriceCalculator deliveryPriceCalculator)
		{
			_deliveryPriceCalculator = deliveryPriceCalculator ?? throw new ArgumentNullException(nameof(deliveryPriceCalculator));

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

			if(changedObject is OrderAddressType orderAddressType)
			{
				_orderAddressType = orderAddressType;
				Refresh();
			}
		}

		public void Refresh()
		{
			_deliveryPoint = (InfoProvider as IDeliveryPointInfoProvider)?.DeliveryPoint;
			if(_deliveryPoint == null || string.IsNullOrWhiteSpace(_deliveryPoint?.City)) {
				return;
			}

			_orderAddressType = (InfoProvider as IDeliveryPointInfoProvider)?.TypeOfAddress;

			DeliveryPriceNode deliveryPrice;

			if(_orderAddressType == OrderAddressType.Service)
			{
				deliveryPrice = _deliveryPriceCalculator.CalculateForService(_deliveryPoint);
				deliverypriceview.ServiceDistrict = InfoProvider.UoW.GetById<ServiceDistrict>(deliveryPrice.ServiceDistrictId);
			}
			else
			{
				deliveryPrice = _deliveryPriceCalculator.Calculate(_deliveryPoint);
				deliverypriceview.District = _deliveryPoint.District;
			}

			labelError.Visible = deliveryPrice.HasError;
			labelError.Markup = $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\"><b>{deliveryPrice.ErrorMessage}</b></span>";

			deliverypriceview.Visible = !deliveryPrice.HasError;
			deliverypriceview.TypeOfAddress = _orderAddressType;	
			deliverypriceview.DeliveryPrice = deliveryPrice;
		}

		#endregion IPanelView implementation
	}
}
