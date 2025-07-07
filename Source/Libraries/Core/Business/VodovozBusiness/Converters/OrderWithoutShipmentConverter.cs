using System;
using System.Linq;
using TaxcomEdo.Contracts.OrdersWithoutShipment;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Controllers;

namespace Vodovoz.Converters
{
	public class OrderWithoutShipmentConverter : IOrderWithoutShipmentConverter
	{
		private readonly ICounterpartyConverter _counterpartyConverter;
		private readonly IOrganizationConverter _organizationConverter;
		private readonly ICounterpartyEdoAccountController _counterpartyEdoAccountController;

		public OrderWithoutShipmentConverter(
			ICounterpartyConverter counterpartyConverter,
			IOrganizationConverter organizationConverter,
			ICounterpartyEdoAccountController counterpartyEdoAccountController)
		{
			_counterpartyConverter = counterpartyConverter ?? throw new ArgumentNullException(nameof(counterpartyConverter));
			_organizationConverter = organizationConverter ?? throw new ArgumentNullException(nameof(organizationConverter));
			_counterpartyEdoAccountController = counterpartyEdoAccountController ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountController));
		}
		
		public OrderWithoutShipmentInfo ConvertOrderWithoutShipmentForDebtToOrderWithoutShipmentInfo(
			OrderWithoutShipmentForDebt orderForDebt, Organization organization, DateTime dateTime)
		{
			var counterpartyInfo = _counterpartyConverter.ConvertCounterpartyToCounterpartyInfoForEdo(
				orderForDebt.Client,
				_counterpartyEdoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(orderForDebt.Client, organization.Id));
			
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
			var counterpartyInfo = _counterpartyConverter.ConvertCounterpartyToCounterpartyInfoForEdo(
				orderForPayment.Client,
				_counterpartyEdoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(orderForPayment.Client, organization.Id));
			
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
			var counterpartyInfo = _counterpartyConverter.ConvertCounterpartyToCounterpartyInfoForEdo(
				orderForAdvancePayment.Client,
				_counterpartyEdoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(orderForAdvancePayment.Client, organization.Id));
			
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
