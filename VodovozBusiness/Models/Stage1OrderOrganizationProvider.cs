using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Models
{
    public class Stage1OrderOrganizationProvider : IOrderOrganizationProvider
    {
        private readonly IOrganizationParametersProvider organizationParametersProvider;

        public Stage1OrderOrganizationProvider(IOrganizationParametersProvider organizationParametersProvider)
        {
            this.organizationParametersProvider = organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));
        }
        
        public Organization GetOrganization(IUnitOfWork uow, Order order)
        {
            if (uow == null) throw new ArgumentNullException(nameof(uow));
            if (order == null) throw new ArgumentNullException(nameof(order));
            
            if (order.SelfDelivery) {
                return GetOrganizationForSelfDelivery(uow, order.PaymentType);
            }

            if(IsOnlineStoreOrder(order)) {
                return GetOrganizationForOnlineStore(uow);
            }
            
            return GetOrganizationForOtherOptions(uow, order.PaymentType);
        }

        private Organization GetOrganizationForSelfDelivery(IUnitOfWork uow, PaymentType paymentType)
        {
            int organizationId = 0;
            switch(paymentType) {
                case PaymentType.barter:
                case PaymentType.cashless:
                case PaymentType.ContractDoc:
                    organizationId = organizationParametersProvider.VodovozOrganizationId;
                    break;
                case PaymentType.cash:
                case PaymentType.Terminal:
                case PaymentType.ByCard:
                    organizationId = organizationParametersProvider.VodovozSouthOrganizationId;
                    break;
                case PaymentType.BeveragesWorld:
                    organizationId = organizationParametersProvider.BeveragesWorldOrganizationId;
                    break;
                default:
                    throw new NotSupportedException($"Невозможно подобрать организацию, так как тип оплаты {paymentType} не поддерживается.");
            }

            return uow.GetById<Organization>(organizationId);
        }

        private bool IsOnlineStoreOrder(Order order)
        {
            return order.OrderItems.Any(x => x.Nomenclature.OnlineStore != null);
        }
        
        private Organization GetOrganizationForOnlineStore(IUnitOfWork uow)
        {
            return uow.GetById<Organization>(organizationParametersProvider.VodovozSouthOrganizationId);
        }
        
        private Organization GetOrganizationForOtherOptions(IUnitOfWork uow, PaymentType paymentType)
        {
            int organizationId = 0;
            switch(paymentType) {
                case PaymentType.barter:
                case PaymentType.cashless:
                case PaymentType.ContractDoc:
                    organizationId = organizationParametersProvider.VodovozOrganizationId;
                    break;
                case PaymentType.cash:
                    organizationId = organizationParametersProvider.SosnovcevOrganizationId;
                    break;
                case PaymentType.Terminal:
                case PaymentType.ByCard:
                    organizationId = organizationParametersProvider.VodovozSouthOrganizationId;
                    break;
                case PaymentType.BeveragesWorld:
                    organizationId = organizationParametersProvider.BeveragesWorldOrganizationId;
                    break;
                default:
                    throw new NotSupportedException($"Тип оплаты {paymentType} не поддерживается, невозможно подобрать организацию.");
            }

            return uow.GetById<Organization>(organizationId);
        }
    }
}