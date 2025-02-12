using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Docflow.Factories
{
	public class OrderUpdOperationFactory : IOrderUpdOperationFactory, IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly IGenericRepository<RouteListItemEntity> _routeListAddressRepository;
		private readonly IGenericRepository<OrderEntity> _orderRepository;

		public OrderUpdOperationFactory(
			IUnitOfWorkFactory unitOfWorkFactory,
			IGenericRepository<RouteListItemEntity> routeListAddressRepository,
			IGenericRepository<OrderEntity> orderRepository)
		{
			_uow = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory))).CreateWithoutRoot(nameof(OrderUpdOperationFactory));
			_routeListAddressRepository = routeListAddressRepository ?? throw new ArgumentNullException(nameof(routeListAddressRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}

		public OrderUpdOperation CreateOrUpdateOrderUpdOperation(OrderEntity order)
		{
			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			if(order.DeliveryDate is null)
			{
				throw new InvalidOperationException("Нельзя сделать УПД без даты заказа");
			}

			if(order.Client.PersonType == PersonType.natural)
			{
				throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}

			var client = order.Client;
			var contract = order.Contract;
			var deliveryPoint = order.DeliveryPoint;
			var organization = contract.Organization;

			var organizationAccountant = organization.OrganizationVersionOnDate(order.DeliveryDate.Value)?.Accountant;
			var organizationLeader = organization.OrganizationVersionOnDate(order.DeliveryDate.Value)?.Leader;

			var routeListAddresses = _routeListAddressRepository
				.Get(_uow, x => x.Order.Id == order.Id)
				.ToList();

			var orderPayments = _orderRepository.GetOrderPayments(_uow, order.Id)
				.Where(x => x.Date < order.DeliveryDate.Value.Date.AddDays(1));

			var isSpecialAndAllSpecialContractDataFilled =
				client.UseSpecialDocFields
				&& !string.IsNullOrWhiteSpace(client.SpecialContractName)
				&& !string.IsNullOrWhiteSpace(client.SpecialContractNumber)
				&& client.SpecialContractDate.HasValue;

			var orderUpdOperation = new OrderUpdOperation();

			orderUpdOperation.OrderId = order.Id;
			orderUpdOperation.OrderDeliveryDate = order.DeliveryDate.Value;
			orderUpdOperation.CounterpartyExternalOrderId = order.CounterpartyExternalOrderId;
			orderUpdOperation.IsOrderForOwnNeeds = client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds;

			orderUpdOperation.ClientContractDocumentName = isSpecialAndAllSpecialContractDataFilled ? client.SpecialContractName : "Договор";
			orderUpdOperation.ClientContractNumber = isSpecialAndAllSpecialContractDataFilled ? client.SpecialContractNumber : contract.Number;
			orderUpdOperation.ClientContractDate = isSpecialAndAllSpecialContractDataFilled ? client.SpecialContractDate.Value : contract.IssueDate;

			orderUpdOperation.ClientId = client.Id;
			orderUpdOperation.ClientName = client.UseSpecialDocFields && !string.IsNullOrWhiteSpace(client.SpecialCustomer) ? client.SpecialCustomer : client.FullName;
			orderUpdOperation.ClientAddress = client.JurAddress;
			orderUpdOperation.ClientGovContract = client.UseSpecialDocFields && !string.IsNullOrWhiteSpace(client.GovContract) ? client.GovContract : "";
			orderUpdOperation.ClientInn = client.INN;
			orderUpdOperation.ClientKpp = client.UseSpecialDocFields && !string.IsNullOrWhiteSpace(client.PayerSpecialKPP) ? client.PayerSpecialKPP : client.KPP;

			orderUpdOperation.ConsigneeName = client.FullName;
			orderUpdOperation.ConsigneeInn = client.INN;

			switch(client.CargoReceiverSource)
			{
				case CargoReceiverSource.FromDeliveryPoint:
					orderUpdOperation.ConsigneeAddress = deliveryPoint != null ? deliveryPoint.ShortAddress : client.JurAddress;
					orderUpdOperation.ConsigneeKpp = deliveryPoint?.KPP ?? client.KPP;
					orderUpdOperation.ConsigneeSummary = string.Concat(
						orderUpdOperation.ConsigneeName,
						", ",
						orderUpdOperation.ConsigneeInn,
						"/",
						orderUpdOperation.ConsigneeKpp,
						", ",
						deliveryPoint.ShortAddress);
					break;
				case CargoReceiverSource.Special:
					if(!string.IsNullOrWhiteSpace(client.CargoReceiver) && client.UseSpecialDocFields)
					{
						orderUpdOperation.ConsigneeAddress = client.CargoReceiver;
						orderUpdOperation.ConsigneeKpp = client.PayerSpecialKPP ?? client.KPP;
						orderUpdOperation.ConsigneeSummary = client.CargoReceiver;
					}
					else
					{
						orderUpdOperation.ConsigneeName = orderUpdOperation.ClientName;
						orderUpdOperation.ConsigneeAddress = client.JurAddress;
						orderUpdOperation.ConsigneeKpp = client.KPP;
						orderUpdOperation.ConsigneeSummary = string.Concat(
							orderUpdOperation.ConsigneeName,
							", ",
							orderUpdOperation.ConsigneeAddress);
					}
					break;
				default:
					orderUpdOperation.ConsigneeName = orderUpdOperation.ClientName;
					orderUpdOperation.ConsigneeAddress = client.JurAddress;
					orderUpdOperation.ConsigneeKpp = client.KPP;
					orderUpdOperation.ConsigneeSummary = string.Concat(
						orderUpdOperation.ConsigneeName,
						", ",
						orderUpdOperation.ConsigneeAddress);
					break;
			}

			orderUpdOperation.OrganizationName = organization.Name;
			orderUpdOperation.OrganizationAddress = organization.ActiveOrganizationVersion.JurAddress;
			orderUpdOperation.OrganizationInn = organization.INN;
			orderUpdOperation.OrganizationKpp = organization.KPP;
			orderUpdOperation.OrganizationTaxcomEdoAccountId = organization.TaxcomEdoAccountId;

			orderUpdOperation.BuhLastName = organizationAccountant?.LastName;
			orderUpdOperation.BuhName = organizationAccountant?.Name;
			orderUpdOperation.BuhPatronymic = organizationAccountant?.Patronymic;
			orderUpdOperation.LeaderLastName = organizationLeader?.LastName;
			orderUpdOperation.LeaderName = organizationLeader?.Name;
			orderUpdOperation.LeaderPatronymic = organizationLeader?.Patronymic;

			orderUpdOperation.BottlesInFact = order.OrderStatus == OrderStatus.Closed ? routeListAddresses.Max(x => x.BottlesReturned).ToString() : "";
			orderUpdOperation.IsSelfDelivery = order.SelfDelivery;

			orderUpdOperation.Payments.Clear();

			if(order.OrderPaymentStatus == OrderPaymentStatus.Paid)
			{
				foreach(var payment in orderPayments)
				{
					var orderUpdOperationPayment = new OrderUpdOperationPayment
					{
						OrderUpdOperation = orderUpdOperation,
						PaymentNum = payment.PaymentNum.ToString(),
						PaymentDate = payment.Date
					};

					orderUpdOperation.Payments.Add(orderUpdOperationPayment);
				}
			}

			orderUpdOperation.Goods.Clear();

			foreach(var orderItem in order.OrderItems)
			{
				var nomenclature = orderItem.Nomenclature;

				var product = new OrderUpdOperationProduct
				{
					OrderUpdOperation = orderUpdOperation,
					NomenclatureId = nomenclature.Id,
					NomenclatureName = nomenclature.OfficialName,
					IsService =
						nomenclature.Category == NomenclatureCategory.master
						|| nomenclature.Category == NomenclatureCategory.service,
					MeasurementUnitName = nomenclature.Unit.Name,
					OKEI = nomenclature.Unit.OKEI,
					Count = orderItem.Count,
					ItemPrice = orderItem.Price,
					IncludeVat = orderItem.IncludeNDS ?? 0,
					Vat = orderItem.ValueAddedTax,
					ItemDiscountMoney = orderItem.DiscountMoney
				};

				orderUpdOperation.Goods.Add(product);
			}

			return orderUpdOperation;
		}

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
