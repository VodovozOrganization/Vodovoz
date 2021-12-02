using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.Services;

namespace Vodovoz.Models
{
	public class Stage2OrganizationProvider : IOrganizationProvider
	{
		private readonly IOrganizationParametersProvider _organizationParametersProvider;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly IGeographicGroupParametersProvider _geographicGroupParametersProvider;

		public Stage2OrganizationProvider(
			IOrganizationParametersProvider organizationParametersProvider,
			IOrderParametersProvider orderParametersProvider,
			IGeographicGroupParametersProvider geographicGroupParametersProvider)
		{
			_organizationParametersProvider = organizationParametersProvider
				?? throw new ArgumentNullException(nameof(organizationParametersProvider));
			_orderParametersProvider = orderParametersProvider
				?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_geographicGroupParametersProvider = geographicGroupParametersProvider
				?? throw new ArgumentNullException(nameof(geographicGroupParametersProvider));
		}
		
		/// <summary>
		/// Метод подбора организации.
		/// Если в заказе установлена наша организация - берем ее,
		/// Иначе, если у клиента прописана организация с которой он работает - возвращаем ее
		/// Иначе подбираем организацию по параметрам: если заполнены параметры paymentFrom и paymentType,
		/// то они берутся для подбора организации вместо соответствующих полей заказа
		/// </summary>
		/// <param name="uow">Unit Of Work</param>
		/// <param name="order">Заказ, для которого подбираем организацию</param>
		/// <param name="paymentFrom">Откуда произведена оплата, если не null берем его для подбора организации</param>
		/// <param name="paymentType">Тип оплаты, если не null берем его для подбора организации</param>
		/// <returns>Организация для заказа</returns>
		/// <exception cref="ArgumentNullException">Исключение при order = null</exception>
		/// <exception cref="InvalidOperationException">Исключение при дате доставки = null</exception>
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

			if(paymentFrom != null && paymentType != null)
			{
				return GetOrganizationForOrderParameters(uow, paymentType.Value, isSelfDelivery, order.DeliveryDate.Value, order.OrderItems,
					paymentFrom, order.DeliveryPoint?.District?.GeographicGroup);
			}
			
			return GetOrganizationForOrderParameters(uow, order.PaymentType, isSelfDelivery, order.DeliveryDate.Value, order.OrderItems,
				order.PaymentByCardFrom, order.DeliveryPoint?.District?.GeographicGroup);
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
		/// Подбор организации исходя из переданных параметров
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="paymentType">Тип оплаты заказа</param>
		/// <param name="isSelfDelivery">Самовывоз, или нет</param>
		/// <param name="deliveryDate">Дата доставки</param>
		/// <param name="orderItems">Список строк заказа</param>
		/// <param name="paymentFrom">Откуда произведена оплата</param>
		/// <param name="geographicGroup">Часть города</param>
		/// <returns>Организация для заказа</returns>
		/// <exception cref="ArgumentNullException">Исключение при uow = null</exception>
		private Organization GetOrganizationForOrderParameters(IUnitOfWork uow, PaymentType paymentType, bool isSelfDelivery,
			DateTime deliveryDate, IEnumerable<OrderItem> orderItems = null, PaymentFrom paymentFrom = null,
			GeographicGroup geographicGroup = null)
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
				? GetOrganizationForSelfDelivery(uow, paymentType, deliveryDate, paymentFrom, geographicGroup)
				: GetOrganizationForOtherOptions(uow, paymentType, deliveryDate, paymentFrom, geographicGroup);
		}

		private Organization GetOrganizationForSelfDelivery(IUnitOfWork uow, PaymentType paymentType, DateTime deliveryDate,
			PaymentFrom paymentFrom = null, GeographicGroup geographicGroup = null)
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
					organizationId = GetOrganizationIdForTerminalByDeliveryDate(deliveryDate);
					break;
				case PaymentType.ByCard:
					organizationId = GetOrganizationIdForByCard(paymentFrom, geographicGroup);
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

		private Organization GetOrganizationForOtherOptions(IUnitOfWork uow, PaymentType paymentType, DateTime deliveryDate,
			PaymentFrom paymentFrom = null, GeographicGroup geographicGroup = null)
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
					organizationId = GetOrganizationIdForTerminalByDeliveryDate(deliveryDate);
					break;
				case PaymentType.ByCard:
					organizationId = GetOrganizationIdForByCard(paymentFrom, geographicGroup);
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

		//FIXME убрать проверку после 2021-10-27. Выставить для терминала по умолчанию всегда Север
		private int GetOrganizationIdForTerminalByDeliveryDate(DateTime deliveryDate)
		{
			if(deliveryDate <= Convert.ToDateTime("2021-10-12"))
			{
				return _organizationParametersProvider.VodovozSouthOrganizationId;
			}
			if(deliveryDate >= Convert.ToDateTime("2021-10-14"))
			{
				return _organizationParametersProvider.VodovozNorthOrganizationId;
			}
			return _organizationParametersProvider.VodovozNorthOrganizationId;
		}

		private int GetOrganizationIdForByCard(PaymentFrom paymentFrom = null, GeographicGroup geographicGroup = null)
		{
			if(paymentFrom == null)
			{
				return _organizationParametersProvider.VodovozNorthOrganizationId;
			}
			if(paymentFrom.Id == _orderParametersProvider.PaymentByCardFromSmsId)
			{
				if(geographicGroup == null)
				{
					return _organizationParametersProvider.VodovozNorthOrganizationId;
				}
				return geographicGroup.Id == _geographicGroupParametersProvider.NorthGeographicGroupId
					? _organizationParametersProvider.VodovozSouthOrganizationId
					: _organizationParametersProvider.VodovozNorthOrganizationId;
			}
			return _orderParametersProvider.PaymentsByCardFromForNorthOrganization.Contains(paymentFrom.Id)
				? _organizationParametersProvider.VodovozNorthOrganizationId
				: _organizationParametersProvider.VodovozSouthOrganizationId;
		}
	}
}
