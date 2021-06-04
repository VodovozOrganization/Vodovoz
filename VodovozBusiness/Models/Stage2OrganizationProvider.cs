using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Models
{
    public class Stage2OrganizationProvider : IOrganizationProvider
    {
        private readonly IOrganizationParametersProvider _organizationParametersProvider;
        private readonly IOrderParametersProvider _orderParametersProvider;

        public Stage2OrganizationProvider(
            IOrganizationParametersProvider organizationParametersProvider,
            IOrderParametersProvider orderParametersProvider
            )
        {
            _organizationParametersProvider = organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));
            _orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
        }
        
        public Organization GetOrganization(IUnitOfWork uow, Order order)
        {
            if (uow == null)
            {
	            throw new ArgumentNullException(nameof(uow));
            }
            if (order == null)
            {
	            throw new ArgumentNullException(nameof(order));
            }

            if(IsOnlineStoreOrder(order)) {
                return GetOrganizationForOnlineStore(uow);
            }
            
            if (order.SelfDelivery || order.DeliveryPoint == null) {
                return GetOrganizationForSelfDelivery(uow, order);
            }
            
            return GetOrganizationForOtherOptions(uow, order);
        }

        private Organization GetOrganizationForSelfDelivery(IUnitOfWork uow, Order order)
        {
            int organizationId;
            switch(order.PaymentType) {
                case PaymentType.barter:
                case PaymentType.cashless:
                case PaymentType.ContractDoc:
                    organizationId = _organizationParametersProvider.VodovozOrganizationId;
                    break;
                case PaymentType.cash:
	                organizationId = _organizationParametersProvider.VodovozDeshitsOrganizationId;
	                break;
                case PaymentType.Terminal:
                case PaymentType.ByCard:
                    organizationId = _organizationParametersProvider.VodovozSouthOrganizationId;
                    break;
                case PaymentType.BeveragesWorld:
                    organizationId = _organizationParametersProvider.BeveragesWorldOrganizationId;
                    break;
                default:
                    throw new NotSupportedException($"Невозможно подобрать организацию, так как тип оплаты {order.PaymentType} не поддерживается.");
            }

            return uow.GetById<Organization>(organizationId);
        }

        private bool IsOnlineStoreOrder(Order order)
        {
            return order.OrderItems.Any(x => x.Nomenclature.OnlineStore != null && x.Nomenclature.OnlineStore.Id != _orderParametersProvider.OldInternalOnlineStoreId);
        }
        
        private Organization GetOrganizationForOnlineStore(IUnitOfWork uow)
        {
            return uow.GetById<Organization>(_organizationParametersProvider.VodovozSouthOrganizationId);
        }
        
        private Organization GetOrganizationForOtherOptions(IUnitOfWork uow, Order order)
        {
            int organizationId;
            switch(order.PaymentType) {
                case PaymentType.barter:
                case PaymentType.cashless:
                case PaymentType.ContractDoc:
                    organizationId = _organizationParametersProvider.VodovozOrganizationId;
                    break;
                case PaymentType.cash:
	                organizationId = _organizationParametersProvider.VodovozDeshitsOrganizationId;
	                break;
                case PaymentType.Terminal:
                case PaymentType.ByCard:
                    organizationId = _organizationParametersProvider.VodovozSouthOrganizationId;
                    break;
                case PaymentType.BeveragesWorld:
                    organizationId = _organizationParametersProvider.BeveragesWorldOrganizationId;
                    break;
                default:
                    throw new NotSupportedException($"Тип оплаты {order.PaymentType} не поддерживается, невозможно подобрать организацию.");
            }

            return uow.GetById<Organization>(organizationId);
        }
        
        public Organization GetOrganizationForOrderWithoutShipment(IUnitOfWork uow, OrderWithoutShipmentForAdvancePayment order)
        {
            if (uow == null)
            {
	            throw new ArgumentNullException(nameof(uow));
            }
            if (order == null)
            {
	            throw new ArgumentNullException(nameof(order));
            }

            int organizationId = _organizationParametersProvider.VodovozOrganizationId;
            if(IsOnlineStoreOrderWithoutShipment(order)) {
                organizationId = _organizationParametersProvider.VodovozSouthOrganizationId;
            }
            
            return uow.GetById<Organization>(organizationId);
        }
        
        private bool IsOnlineStoreOrderWithoutShipment(OrderWithoutShipmentForAdvancePayment order)
        {
            return order.OrderWithoutDeliveryForAdvancePaymentItems.Any(x => x.Nomenclature.OnlineStore != null && x.Nomenclature.OnlineStore.Id != _orderParametersProvider.OldInternalOnlineStoreId);
        }
        
        public int GetMainOrganization()
        {
            return SingletonParametersProvider.Instance.GetIntValue("main_organization_id");
        }
    }
}
