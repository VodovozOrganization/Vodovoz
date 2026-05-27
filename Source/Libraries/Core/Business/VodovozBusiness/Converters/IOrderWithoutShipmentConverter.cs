using System;
using TaxcomEdo.Contracts.OrdersWithoutShipment;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Converters
{
	public interface IOrderWithoutShipmentConverter
	{
		OrderWithoutShipmentInfo ConvertOrderWithoutShipmentForDebtToOrderWithoutShipmentInfo(
			OrderWithoutShipmentForDebt orderForDebt, DateTime dateTime);

		OrderWithoutShipmentInfo ConvertOrderWithoutShipmentForPaymentToOrderWithoutShipmentInfo(
			OrderWithoutShipmentForPayment orderForPayment, DateTime dateTime);

		OrderWithoutShipmentInfo ConvertOrderWithoutShipmentForAdvancePaymentToOrderWithoutShipmentInfo(
			OrderWithoutShipmentForAdvancePayment orderForAdvancePayment, DateTime dateTime);
	}
}
