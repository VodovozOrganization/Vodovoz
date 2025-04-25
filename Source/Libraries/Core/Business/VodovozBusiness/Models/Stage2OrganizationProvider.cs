using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Models
{
	public class Stage2OrganizationProvider : IOrganizationProvider
	{
		private readonly DateTime _terminalVodovozSouthStartDate = new DateTime(2025, 4, 18, 0, 0, 0, DateTimeKind.Local);

		private readonly IOrganizationSettings _organizationSettings;
		private readonly IOrderSettings _orderSettings;
		private readonly IGeographicGroupSettings _geographicGroupSettings;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly ICashReceiptRepository _cashReceiptRepository;

		public Stage2OrganizationProvider(
			IOrganizationSettings organizationSettings,
			IOrderSettings orderSettings,
			IGeographicGroupSettings geographicGroupSettings,
			IFastPaymentRepository fastPaymentRepository,
			ICashReceiptRepository cashReceiptRepository)
		{
			_organizationSettings = organizationSettings
				?? throw new ArgumentNullException(nameof(organizationSettings));
			_orderSettings = orderSettings
				?? throw new ArgumentNullException(nameof(orderSettings));
			_geographicGroupSettings = geographicGroupSettings
				?? throw new ArgumentNullException(nameof(geographicGroupSettings));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
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
				? _organizationSettings.VodovozNorthOrganizationId
				: _organizationSettings.VodovozOrganizationId;

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

			if(order.OurOrganization != null)
			{
				return order.OurOrganization;
			}

			if(order.Client.WorksThroughOrganization != null)
			{
				return order.Client.WorksThroughOrganization;
			}

			if(order.Id != 0 && order.Contract != null && _cashReceiptRepository.HasNeededReceipt(order.Id))
			{
				return order.Contract.Organization;
			}

			var isSelfDelivery = order.SelfDelivery || order.DeliveryPoint == null;

			return GetOrganizationForOrderParameters(uow, paymentType ?? order.PaymentType, isSelfDelivery, order.CreateDate, order.DeliveryDate,
				order.OrderItems, paymentFrom ?? order.PaymentByCardFrom, order.DeliveryPoint?.District?.GeographicGroup, order.OnlineOrder);
		}

		private Organization GetOrganizationForOrderParameters(IUnitOfWork uow, PaymentType paymentType, bool isSelfDelivery,
			DateTime? orderCreateDate, DateTime? orderDeliveryDate, IEnumerable<OrderItem> orderItems, PaymentFrom paymentFrom, GeoGroup geographicGroup,
			int? onlineOrderId)
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
					uow, paymentType, orderCreateDate, orderDeliveryDate, paymentFrom, geographicGroup, onlineOrderId)
				: GetOrganizationForOtherOptions(
					uow, paymentType, orderCreateDate, orderDeliveryDate, paymentFrom, geographicGroup, onlineOrderId);
		}

		private Organization GetOrganizationForSelfDelivery(IUnitOfWork uow, PaymentType paymentType, DateTime? orderCreateDate, DateTime? orderDeliveryDate,
			PaymentFrom paymentFrom, GeoGroup geographicGroup, int? onlineOrderId)
		{
			int organizationId;
			switch(paymentType)
			{
				case PaymentType.Barter:
				case PaymentType.Cashless:
				case PaymentType.ContractDocumentation:
					organizationId = _organizationSettings.VodovozOrganizationId;
					break;
				case PaymentType.Cash:
					organizationId = _organizationSettings.VodovozNorthOrganizationId;
					break;
				case PaymentType.Terminal:
					if(orderDeliveryDate >= _terminalVodovozSouthStartDate)
					{
						organizationId = _organizationSettings.VodovozSouthOrganizationId;
						break;
					}
					organizationId = _organizationSettings.BeveragesWorldOrganizationId;
					break;
				case PaymentType.DriverApplicationQR:
				case PaymentType.SmsQR:
					organizationId = GetOrganizationIdForByCard(uow, uow.GetById<PaymentFrom>(_orderSettings.GetPaymentByCardFromFastPaymentServiceId), geographicGroup, orderCreateDate, onlineOrderId); 
					break;
				case PaymentType.PaidOnline:
					organizationId = GetOrganizationIdForByCard(uow, paymentFrom, geographicGroup, orderCreateDate, onlineOrderId);
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
					x.Nomenclature.OnlineStore.Id != _orderSettings.OldInternalOnlineStoreId);
		}

		private Organization GetOrganizationForOnlineStore(IUnitOfWork uow)
		{
			return uow.GetById<Organization>(_organizationSettings.VodovozNorthOrganizationId);
		}

		private Organization GetOrganizationForOtherOptions(IUnitOfWork uow, PaymentType paymentType, DateTime? orderCreateDate, DateTime? orderDeliveryDate,
			PaymentFrom paymentFrom, GeoGroup geographicGroup, int? onlineOrderId)
		{
			int organizationId;
			switch(paymentType)
			{
				case PaymentType.Barter:
				case PaymentType.Cashless:
				case PaymentType.ContractDocumentation:
					organizationId = _organizationSettings.VodovozOrganizationId;
					break;
				case PaymentType.Cash:
					organizationId = _organizationSettings.VodovozNorthOrganizationId;
					break;
				case PaymentType.Terminal:
					if(orderDeliveryDate >= _terminalVodovozSouthStartDate)
					{
						organizationId = _organizationSettings.VodovozSouthOrganizationId;
						break;
					}
					organizationId = _organizationSettings.BeveragesWorldOrganizationId;
					break;
				case PaymentType.DriverApplicationQR:
				case PaymentType.SmsQR:
					organizationId = GetOrganizationIdForByCard(uow, uow.GetById<PaymentFrom>(_orderSettings.GetPaymentByCardFromFastPaymentServiceId), geographicGroup, orderCreateDate, onlineOrderId);
					break;
				case PaymentType.PaidOnline:
					organizationId = GetOrganizationIdForByCard(uow, paymentFrom, geographicGroup, orderCreateDate, onlineOrderId);
					break;
				default:
					throw new NotSupportedException($"Тип оплаты {paymentType} не поддерживается, невозможно подобрать организацию.");
			}

			return uow.GetById<Organization>(organizationId);
		}

		private bool IsOnlineStoreOrderWithoutShipment(OrderWithoutShipmentForAdvancePayment order)
		{
			return order.OrderWithoutDeliveryForAdvancePaymentItems.Any(x =>
				x.Nomenclature.OnlineStore != null && x.Nomenclature.OnlineStore.Id != _orderSettings.OldInternalOnlineStoreId);
		}

		private int GetOrganizationIdForByCard(IUnitOfWork uow, PaymentFrom paymentFrom, GeoGroup geographicGroup,
			DateTime? orderCreateDate, int? onlineOrderId)
		{
			if(paymentFrom == null)
			{
				return _organizationSettings.VodovozNorthOrganizationId;
			}
			
			if(_orderSettings.PaymentsByCardFromForNorthOrganization.Contains(paymentFrom.Id)
				&& orderCreateDate.HasValue
				&& orderCreateDate.Value < new DateTime(2022, 08, 30, 13, 00, 00))
			{
				return _organizationSettings.VodovozNorthOrganizationId;
			}

			if(_orderSettings.PaymentsByCardFromAvangard.Contains(paymentFrom.Id))
			{
				if(!onlineOrderId.HasValue)
				{
					return GetPaymentFromOrganisationIdOrDefault(paymentFrom);
				}

				var fastPayment = _fastPaymentRepository.GetPerformedFastPaymentByExternalId(uow, onlineOrderId.Value);
				if(fastPayment == null)
				{
					return GetPaymentFromOrganisationIdOrDefault(paymentFrom);
				}

				return fastPayment.Organization?.Id ?? _organizationSettings.VodovozNorthOrganizationId;
			}

			return GetPaymentFromOrganisationIdOrDefault(paymentFrom);
		}

		private int GetPaymentFromOrganisationIdOrDefault(PaymentFrom paymentFrom) =>
			paymentFrom.OrganizationForOnlinePayments?.Id ?? _organizationSettings.VodovozNorthOrganizationId;
	}
}
