namespace Vodovoz.Domain.Logistic.Drivers
{
	public enum DriverMobileAppActionType
	{
		OpenOrderInfoPanel,
		OpenOrderDeliveryPanel,
		OpenOrderReceiptionPanel,
		CompleteOrderClicked,
		ChangeOrderPaymentTypeClicked,
		RollbackRouteListAddressStatusEnRouteClicked,
		PayBySmsClicked,
		PayByQRClicked
	}
}
