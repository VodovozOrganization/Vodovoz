using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Services;

namespace Vodovoz.Models
{
	public class Stage2OrganizationProvider : IOrganizationProvider
	{
		private readonly IOrganizationParametersProvider _organizationParametersProvider;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly IGeographicGroupParametersProvider _geographicGroupParametersProvider;
		private readonly IFastPaymentRepository _fastPaymentRepository;

		public Stage2OrganizationProvider(
			IOrganizationParametersProvider organizationParametersProvider,
			IOrderParametersProvider orderParametersProvider,
			IGeographicGroupParametersProvider geographicGroupParametersProvider,
			IFastPaymentRepository fastPaymentRepository)
		{
			_organizationParametersProvider = organizationParametersProvider
				?? throw new ArgumentNullException(nameof(organizationParametersProvider));
			_orderParametersProvider = orderParametersProvider
				?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_geographicGroupParametersProvider = geographicGroupParametersProvider
				?? throw new ArgumentNullException(nameof(geographicGroupParametersProvider));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
		}

		public Organization GetOrganizationForOrderWithoutShipment(IUnitOfWork uow, OrderWithoutShipmentForAdvancePayment order)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}
			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			var organizationId = IsOnlineStoreOrderWithoutShipment(order)
				? _organizationParametersProvider.VodovozSouthOrganizationId
				: _organizationParametersProvider.VodovozOrganizationId;

			return uow.GetById<Organization>(organizationId);
		}

		/// <summary>
		/// Метод подбора организации.<br/>
		/// Если в заказе установлена наша организация - берем ее.<br/>
		/// Иначе, если у клиента прописана организация с которой он работает - возвращаем ее.<br/>
		/// Иначе подбираем организацию по параметрам: если заполнены параметры paymentFrom и paymentType,
		/// то они берутся для подбора организации вместо соответствующих полей заказа<br/>
		/// </summary>
		/// <param name="uow">Unit Of Work</param>
		/// <param name="order">Заказ, для которого подбираем организацию</param>
		/// <param name="paymentFrom">Источник оплаты, если не null берем его для подбора организации</param>
		/// <param name="paymentType">Тип оплаты, если не null берем его для подбора организации</param>
		/// <returns>Организация для заказа</returns>
		/// <exception cref="ArgumentNullException">Исключение при order = null</exception>
		public Organization GetOrganization(IUnitOfWork uow, Order order, PaymentFrom paymentFrom = null, PaymentType? paymentType = null)
		{
			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}
			if(!order.DeliveryDate.HasValue)
			{
				throw new InvalidOperationException("Order delivery date cannot be null");
			}
			if(order.OurOrganization != null)
			{
				return order.OurOrganization;
			}
			if(order.Client.WorksThroughOrganization != null)
			{
				return order.Client.WorksThroughOrganization;
			}

			var isSelfDelivery = order.SelfDelivery || order.DeliveryPoint == null;

			return GetOrganizationForOrderParameters(uow, paymentType ?? order.PaymentType, isSelfDelivery, order.DeliveryDate.Value,
				order.CreateDate, order.OrderItems, paymentFrom ?? order.PaymentByCardFrom, order.DeliveryPoint?.District?.GeographicGroup,
				order.OnlineOrder);
		}

		private Organization GetOrganizationForOrderParameters(IUnitOfWork uow, PaymentType paymentType, bool isSelfDelivery,
			DateTime orderDeliveryDate, DateTime? orderCreateDate, IEnumerable<OrderItem> orderItems, PaymentFrom paymentFrom,
			GeographicGroup geographicGroup, int? onlineOrderId)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(HasAnyOnlineStoreNomenclature(orderItems))
			{
				return GetOrganizationForOnlineStore(uow);
			}

			return isSelfDelivery
				? GetOrganizationForSelfDelivery(
					uow, paymentType, orderDeliveryDate, orderCreateDate, paymentFrom, geographicGroup, onlineOrderId)
				: GetOrganizationForOtherOptions(
					uow, paymentType, orderDeliveryDate, orderCreateDate, paymentFrom, geographicGroup, onlineOrderId);
		}

		private Organization GetOrganizationForSelfDelivery(IUnitOfWork uow, PaymentType paymentType, DateTime orderDeliveryDate,
			DateTime? orderCreateDate, PaymentFrom paymentFrom, GeographicGroup geographicGroup, int? onlineOrderId)
		{
			int organizationId;
			switch(paymentType)
			{
				case PaymentType.barter:
				case PaymentType.cashless:
				case PaymentType.ContractDoc:
					organizationId = _organizationParametersProvider.VodovozOrganizationId;
					break;
				case PaymentType.cash:
					organizationId = _organizationParametersProvider.VodovozNorthOrganizationId;
					break;
				case PaymentType.Terminal:
					organizationId = GetOrganizationIdForTerminalPaymentTypeByDeliveryDate(orderDeliveryDate);
					break;
				case PaymentType.ByCard:
					organizationId = GetOrganizationIdForByCard(
						uow, paymentFrom, geographicGroup, orderDeliveryDate, orderCreateDate, onlineOrderId);
					break;
				default:
					throw new NotSupportedException(
						$"Невозможно подобрать организацию, так как тип оплаты {paymentType} не поддерживается.");
			}

			return uow.GetById<Organization>(organizationId);
		}

		private bool HasAnyOnlineStoreNomenclature(IEnumerable<OrderItem> orderItems)
		{
			return orderItems != null
				&& orderItems.Any(x =>
					x.Nomenclature.OnlineStore != null &&
					x.Nomenclature.OnlineStore.Id != _orderParametersProvider.OldInternalOnlineStoreId);
		}

		private Organization GetOrganizationForOnlineStore(IUnitOfWork uow)
		{
			return uow.GetById<Organization>(_organizationParametersProvider.VodovozSouthOrganizationId);
		}

		private Organization GetOrganizationForOtherOptions(IUnitOfWork uow, PaymentType paymentType, DateTime orderDeliveryDate,
			DateTime? orderCreateDate, PaymentFrom paymentFrom, GeographicGroup geographicGroup, int? onlineOrderId)
		{
			int organizationId;
			switch(paymentType)
			{
				case PaymentType.barter:
				case PaymentType.cashless:
				case PaymentType.ContractDoc:
					organizationId = _organizationParametersProvider.VodovozOrganizationId;
					break;
				case PaymentType.cash:
					organizationId = _organizationParametersProvider.VodovozNorthOrganizationId;
					break;
				case PaymentType.Terminal:
					organizationId = GetOrganizationIdForTerminalPaymentTypeByDeliveryDate(orderDeliveryDate);
					break;
				case PaymentType.ByCard:
					organizationId = GetOrganizationIdForByCard(
						uow, paymentFrom, geographicGroup, orderDeliveryDate, orderCreateDate, onlineOrderId);
					break;
				default:
					throw new NotSupportedException($"Тип оплаты {paymentType} не поддерживается, невозможно подобрать организацию.");
			}

			return uow.GetById<Organization>(organizationId);
		}

		private bool IsOnlineStoreOrderWithoutShipment(OrderWithoutShipmentForAdvancePayment order)
		{
			return order.OrderWithoutDeliveryForAdvancePaymentItems.Any(x =>
				x.Nomenclature.OnlineStore != null && x.Nomenclature.OnlineStore.Id != _orderParametersProvider.OldInternalOnlineStoreId);
		}

		private int GetOrganizationIdForByCard(IUnitOfWork uow, PaymentFrom paymentFrom, GeographicGroup geographicGroup,
			DateTime orderDeliveryDate, DateTime? orderCreateDate, int? onlineOrderId)
		{
			if(paymentFrom == null)
			{
				return _organizationParametersProvider.VodovozNorthOrganizationId;
			}
			if(paymentFrom.Id == _orderParametersProvider.GetPaymentByCardFromFastPaymentServiceId
				|| paymentFrom.Id == _orderParametersProvider.GetPaymentByCardFromAvangardId)
			{
				if(!onlineOrderId.HasValue)
				{
					return _organizationParametersProvider.VodovozSouthOrganizationId;
				}

				var fastPayment = _fastPaymentRepository.GetPerformedFastPaymentByExternalId(uow, onlineOrderId.Value);
				if(fastPayment == null)
				{
					return _organizationParametersProvider.VodovozSouthOrganizationId;
				}

				return fastPayment.Organization?.Id ?? _organizationParametersProvider.VodovozNorthOrganizationId;
			}
			if(paymentFrom.Id == _orderParametersProvider.GetPaymentByCardFromMarketplaceId)
			{
				return _organizationParametersProvider.VodovozOrganizationId;
			}
			if(paymentFrom.Id == _orderParametersProvider.PaymentFromTerminalId)
			{
				return GetOrganizationIdForTerminalPaymentFromByDeliveryDate(orderDeliveryDate);
			}
			if(paymentFrom.Id == _orderParametersProvider.PaymentByCardFromSmsId)
			{
				if(geographicGroup == null || orderCreateDate == null)
				{
					return _organizationParametersProvider.VodovozNorthOrganizationId;
				}
				if(geographicGroup.Id == _geographicGroupParametersProvider.NorthGeographicGroupId
					&& orderCreateDate.Value.TimeOfDay < _organizationParametersProvider.LatestCreateTimeForSouthOrganizationInByCardOrder)
				{
					return _organizationParametersProvider.VodovozSouthOrganizationId;
				}
				return _organizationParametersProvider.VodovozNorthOrganizationId;
			}
			return _orderParametersProvider.PaymentsByCardFromForNorthOrganization.Contains(paymentFrom.Id)
				? _organizationParametersProvider.VodovozNorthOrganizationId
				: _organizationParametersProvider.VodovozSouthOrganizationId;
		}
		
		//FIXME убрать проверку после 2022-07-21. Выставить для типа оплаты терминал Юг, начиная с 21.07 включительно
		private int GetOrganizationIdForTerminalPaymentTypeByDeliveryDate(DateTime deliveryDate)
		{
			if(deliveryDate <= Convert.ToDateTime("2022-07-19"))
			{
				return _organizationParametersProvider.VodovozNorthOrganizationId;
			}
			if(deliveryDate >= Convert.ToDateTime("2022-07-21"))
			{
				return _organizationParametersProvider.VodovozSouthOrganizationId;
			}
			return _organizationParametersProvider.VodovozSouthOrganizationId;
		}
		
		//FIXME убрать проверку после 2022-07-21. Выставить для источника оплаты терминал с 20.07 включительно Юг
		private int GetOrganizationIdForTerminalPaymentFromByDeliveryDate(DateTime deliveryDate)
		{
			return deliveryDate >= Convert.ToDateTime("2022-07-20")
				? _organizationParametersProvider.VodovozSouthOrganizationId
				: _organizationParametersProvider.VodovozNorthOrganizationId;
		}
	}
}
