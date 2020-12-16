using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;

namespace Vodovoz.Domain.Cash
{
    public class RouteListCashOrganisationDistributor
    {
        private readonly ICashDistributionCommonOrganisationProvider cashDistributionCommonOrganisationProvider;
        private readonly IRouteListItemCashDistributionDocumentRepository routeListItemCashDistributionDocumentRepository;
        private readonly IOrderRepository orderRepository;
        
        public RouteListCashOrganisationDistributor(
            ICashDistributionCommonOrganisationProvider cashDistributionCommonOrganisationProvider,
            IRouteListItemCashDistributionDocumentRepository routeListItemCashDistributionDocumentRepository,
            IOrderRepository orderRepository)
        {
            this.cashDistributionCommonOrganisationProvider =
                cashDistributionCommonOrganisationProvider ?? throw new ArgumentNullException(nameof(cashDistributionCommonOrganisationProvider));
            this.routeListItemCashDistributionDocumentRepository = 
                routeListItemCashDistributionDocumentRepository ?? throw new ArgumentNullException(nameof(routeListItemCashDistributionDocumentRepository));
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public void DistributeIncomeCash(IUnitOfWork uow, RouteList routeList, Income income, decimal amount)
        {
            if (amount == 0) return;
            
            var cashAddresses = routeList.Addresses.Where(x => x.TotalCash > 0);

            if (routeList.Total >
                routeListItemCashDistributionDocumentRepository.GetDistributedAmountOnRouteList(uow, routeList))
            {
                foreach (var address in cashAddresses)
                {
                    var addressDistributedSum =
                        routeListItemCashDistributionDocumentRepository.GetDistributedAmountOnRouteListItem(uow,
                            address);

                    if (addressDistributedSum == address.TotalCash) {
                        continue;
                    }

                    if (addressDistributedSum < address.TotalCash)
                    {
                        var oldSum = addressDistributedSum;
                        var sum = (addressDistributedSum + amount) >= address.TotalCash
                            ? address.TotalCash
                            : addressDistributedSum + amount;

                        var newOperation = CreateOrganisationCashMovementOperation(uow, address);
                        newOperation.Amount = sum;
                        var doc = 
                            CreateRouteListItemCashDistributionDocument(newOperation, address, income);
                        Save(uow, newOperation, doc);

                        amount -= address.TotalCash - oldSum;

                        if (amount <= 0) {
                            break;
                        }
                    }
                }

                if (amount > 0)
                    DistributeIncomeCashRemainingAmount(uow, routeList, income, amount);
            }
            else {
                DistributeIncomeCashRemainingAmount(uow, routeList, income, amount);
            }
        }

        private void DistributeIncomeCashRemainingAmount(IUnitOfWork uow, RouteList routeList, Income income, decimal amount)
        {
            var operation = new OrganisationCashMovementOperation
            {
                OperationTime = DateTime.Now,
                Organisation = cashDistributionCommonOrganisationProvider.GetCommonOrganisation(uow),
                Amount = amount
            };

            var address = routeList.Addresses.First();
            var document = CreateRouteListItemCashDistributionDocument(operation, address, income);
            
            Save(uow, operation, document);
        }

        public void DistributeExpenseCash(IUnitOfWork uow, RouteList routeList, Expense expense, decimal amount)
        {
            if (amount == 0) return;
            
            var cashAddresses = routeList.Addresses.Where(x => x.TotalCash > 0);

            if (routeList.Total <= routeListItemCashDistributionDocumentRepository.GetDistributedAmountOnRouteList(uow, routeList))
            {
                foreach (var address in cashAddresses)
                {
                    var addressDistributedSum =
                        routeListItemCashDistributionDocumentRepository.GetDistributedAmountOnRouteListItem(uow,
                            address);
                    
                    var sum = (addressDistributedSum - amount) >= 0
                        ? -amount
                        : -addressDistributedSum;
                    
                    var newOperation = CreateOrganisationCashMovementOperation(uow, address);
                    newOperation.Amount = sum;
                    var routeListItemCashdistributionDoc =
                        CreateRouteListItemCashDistributionDocument(newOperation, address, expense);
                    Save(uow, newOperation, routeListItemCashdistributionDoc);

                    amount -= addressDistributedSum;

                    if (amount <= 0) {
                        break;
                    }
                }
            }
            else {
                DistributeExpenseCashRemainingAmount(uow, routeList, expense, amount);
            }
        }
        
        private void DistributeExpenseCashRemainingAmount(IUnitOfWork uow, RouteList routeList, Expense expense, decimal amount)
        {
            var operation = new OrganisationCashMovementOperation
            {
                OperationTime = DateTime.Now,
                Organisation = cashDistributionCommonOrganisationProvider.GetCommonOrganisation(uow),
                Amount = -amount
            };

            var address = routeList.Addresses.First();
            var document = CreateRouteListItemCashDistributionDocument(operation, address, expense);
            
            Save(uow, operation, document);
        }

        private OrganisationCashMovementOperation CreateOrganisationCashMovementOperation(
            IUnitOfWork uow, RouteListItem address)
        {
            var hasReceipt = orderRepository.OrderHasSentReceipt(uow, address.Order.Id);

            return new OrganisationCashMovementOperation
            {
                OperationTime = DateTime.Now,
                Organisation = hasReceipt
                    ? address.Order.Contract.Organization
                    : cashDistributionCommonOrganisationProvider.GetCommonOrganisation(uow)
            };
        }

        private RouteListItemCashDistributionDocument CreateRouteListItemCashDistributionDocument(
            OrganisationCashMovementOperation operation, RouteListItem address, Income income)
        {
            return new RouteListItemCashDistributionDocument
            {
                Organisation = operation.Organisation,
                CreationDate = DateTime.Now,
                LastEditedTime = DateTime.Now,
                Author = income.Casher,
                LastEditor = income.Casher,
                RouteListItem = address,
                Employee = income.Employee,
                Income = income,
                OrganisationCashMovementOperation = operation,
                Amount = operation.Amount
            };
        }
        
        private RouteListItemCashDistributionDocument CreateRouteListItemCashDistributionDocument(
            OrganisationCashMovementOperation operation, RouteListItem address, Expense expense)
        {
            return new RouteListItemCashDistributionDocument
            {
                Organisation = operation.Organisation,
                CreationDate = DateTime.Now,
                LastEditedTime = DateTime.Now,
                Author = expense.Casher,
                LastEditor = expense.Casher,
                RouteListItem = address,
                Employee = expense.Employee,
                Expense = expense,
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