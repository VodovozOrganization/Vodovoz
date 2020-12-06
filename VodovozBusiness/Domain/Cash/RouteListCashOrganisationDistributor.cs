using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;

namespace Vodovoz.Domain.Cash
{
    public class RouteListCashOrganisationDistributor
    {
        private readonly ICashDistributionCommonOrganisationProvider cashDistributionCommonOrganisationProvider;
        private readonly IOrderRepository orderRepository;
        
        public RouteListCashOrganisationDistributor(
            ICashDistributionCommonOrganisationProvider cashDistributionCommonOrganisationProvider,
            IOrderRepository orderRepository)
        {
            this.cashDistributionCommonOrganisationProvider =
                cashDistributionCommonOrganisationProvider ?? throw new ArgumentNullException(nameof(cashDistributionCommonOrganisationProvider));
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public void DistributeIncomeCash(IUnitOfWork uow, RouteList routeList, decimal amount)
        {
            var cashAddresses = 
                routeList.Addresses.Where(x => x.Order.PaymentType == PaymentType.cash || 
                                               x.Order.PaymentType == PaymentType.BeveragesWorld);
            
            foreach (var address in cashAddresses)
            {
                var operation = CreateOrganisationCashMovementOperation(uow, address);
                operation.Amount = Math.Abs(amount) > address.Order.ActualTotalSum
                    ? address.Order.ActualTotalSum
                    : amount;
                
                var routeListItemCashdistributionDoc = CreateRouteListItemCashDistributionDocument(operation, address);

                Save(uow, operation, routeListItemCashdistributionDoc);

                if (Math.Abs(amount) > address.Order.ActualTotalSum)
                {
                    amount -= address.Order.ActualTotalSum;
                }
                else
                {
                    break;
                }
            }
        }

        public void DistributeExpenseCash(IUnitOfWork uow, RouteList routeList, decimal amount)
        {
            var cashAddresses = 
                routeList.Addresses.Where(x => x.Order.PaymentType == PaymentType.cash || 
                                               x.Order.PaymentType == PaymentType.BeveragesWorld);
            
            foreach (var address in cashAddresses)
            {
                var operation = CreateOrganisationCashMovementOperation(uow, address);
                operation.Amount = Math.Abs(amount) > address.Order.ActualTotalSum
                    ? -address.Order.ActualTotalSum
                    : -amount;
                
                var routeListItemCashdistributionDoc = CreateRouteListItemCashDistributionDocument(operation, address);

                Save(uow, operation, routeListItemCashdistributionDoc);

                if (Math.Abs(amount) > address.Order.ActualTotalSum)
                {
                    amount += address.Order.ActualTotalSum;
                }
                else
                {
                    break;
                }
            }
        }

        private OrganisationCashMovementOperation CreateOrganisationCashMovementOperation(IUnitOfWork uow, RouteListItem address)
        {
            var hasReceipt = orderRepository.OrderHasSentReceipt(uow, address.Order.Id);

            return new OrganisationCashMovementOperation
            {
                OperationTime = DateTime.Now,
                Organisation = hasReceipt
                    ? address.Order.Contract.Organization
                    : cashDistributionCommonOrganisationProvider.GetCommonOrganisation(uow),
            };
        }

        private RouteListItemCashDistributionDocument CreateRouteListItemCashDistributionDocument(OrganisationCashMovementOperation operation, RouteListItem address)
        {
            return new RouteListItemCashDistributionDocument
            {
                Organisation = operation.Organisation,
                CreationDate = DateTime.Now,
                LastEditedTime = DateTime.Now,
                RouteListItem = address,
                OrganisationCashMovementOperation = operation,
                Amount = operation.Amount
            };
        }
        
        private void Save(IUnitOfWork uow, OrganisationCashMovementOperation operation,
            RouteListItemCashDistributionDocument routeListItemCashdistributionDoc)
        {
            uow.Save(operation);
            uow.Save(routeListItemCashdistributionDoc);
        }
    }
}