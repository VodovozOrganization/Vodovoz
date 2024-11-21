using System;
using System.Linq;
using TaxcomEdo.Contracts.OrdersWithoutShipment;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Converters
{
	public class OrderWithoutShipmentConverter : IOrderWithoutShipmentConverter
	{
		private readonly ICounterpartyConverter _counterpartyConverter;
		private readonly IOrganizationConverter _organizationConverter;

		public OrderWithoutShipmentConverter(
			ICounterpartyConverter counterpartyConverter,
			IOrganizationConverter organizationConverter)
		{
			_counterpartyConverter = counterpartyConverter ?? throw new ArgumentNullException(nameof(counterpartyConverter));
			_organizationConverter = organizationConverter ?? throw new ArgumentNullException(nameof(organizationConverter));
		}
		
		public OrderWithoutShipmentInfo ConvertOrderWithoutShipmentForDebtToOrderWithoutShipmentInfo(
			OrderWithoutShipmentForDebt orderForDebt, Organization organization, DateTime dateTime)
		{
			var counterpartyInfo = _counterpartyConverter.ConvertCounterpartyToCounterpartyInfoForEdo(orderForDebt.Client);
			
			return new OrderWithoutShipmentForDebtInfo
			{
				Id = orderForDebt.Id,
				CreationDate = orderForDebt.CreateDate ?? default,
				CounterpartyInfoForEdo = counterpartyInfo,
				OrganizationInfoForEdo = _organizationConverter.ConvertOrganizationToOrganizationInfoForEdo(organization, dateTime),
				Sum = orderForDebt.DebtSum
			};
		}
		
		public OrderWithoutShipmentInfo ConvertOrderWithoutShipmentForPaymentToOrderWithoutShipmentInfo(
			OrderWithoutShipmentForPayment orderForPayment, Organization organization, DateTime dateTime)
		{
			var counterpartyInfo = _counterpartyConverter.ConvertCounterpartyToCounterpartyInfoForEdo(orderForPayment.Client);
			
			return new OrderWithoutShipmentForPaymentInfo
			{
				Id = orderForPayment.Id,
				CreationDate = orderForPayment.CreateDate ?? default,
				CounterpartyInfoForEdo = counterpartyInfo,
				OrganizationInfoForEdo = _organizationConverter.ConvertOrganizationToOrganizationInfoForEdo(organization, dateTime),
				Sum = orderForPayment.OrderWithoutDeliveryForPaymentItems.Sum(x => x.Order.OrderSum)
			};
		}
		
		public OrderWithoutShipmentInfo ConvertOrderWithoutShipmentForAdvancePaymentToOrderWithoutShipmentInfo(
			OrderWithoutShipmentForAdvancePayment orderForAdvancePayment, Organization organization, DateTime dateTime)
		{
			var counterpartyInfo = _counterpartyConverter.ConvertCounterpartyToCounterpartyInfoForEdo(orderForAdvancePayment.Client);
			
			return new OrderWithoutShipmentForAdvancePaymentInfo
			{
				Id = orderForAdvancePayment.Id,
				CreationDate = orderForAdvancePayment.CreateDate ?? default,
				CounterpartyInfoForEdo = counterpartyInfo,
				OrganizationInfoForEdo = _organizationConverter.ConvertOrganizationToOrganizationInfoForEdo(organization, dateTime),
				Sum = orderForAdvancePayment.OrderWithoutDeliveryForAdvancePaymentItems.Sum(x => x.Sum)
			};
		}
	}
}
