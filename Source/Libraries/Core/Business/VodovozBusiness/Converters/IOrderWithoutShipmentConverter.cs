using System;
using TaxcomEdo.Contracts.OrdersWithoutShipment;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Converters
{
	public interface IOrderWithoutShipmentConverter
	{
		OrderWithoutShipmentInfo ConvertOrderWithoutShipmentForDebtToOrderWithoutShipmentInfo(
			OrderWithoutShipmentForDebt orderForDebt, Organization organization, DateTime dateTime);

		OrderWithoutShipmentInfo ConvertOrderWithoutShipmentForPaymentToOrderWithoutShipmentInfo(
			OrderWithoutShipmentForPayment orderForPayment, Organization organization, DateTime dateTime);

		OrderWithoutShipmentInfo ConvertOrderWithoutShipmentForAdvancePaymentToOrderWithoutShipmentInfo(
			OrderWithoutShipmentForAdvancePayment orderForAdvancePayment, Organization organization, DateTime dateTime);
	}
}
