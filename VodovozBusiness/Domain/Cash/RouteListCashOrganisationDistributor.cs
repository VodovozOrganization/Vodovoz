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

        public void DistributeIncomeCash(IUnitOfWork uow, RouteList routeList, Income income, decimal amount)
        {
            var cashAddresses = 
                routeList.Addresses.Where(x => x.TotalCash > 0);

            //var addressCashSum = cashAddresses.Sum(x => x.AddressCashSum);
            //var orderSum = cashAddresses.Sum(x => x.Order.ActualTotalSum);
            
            foreach (var address in cashAddresses)
            {
                var doc =
                    uow.Session.QueryOver<RouteListItemCashDistributionDocument>()
                        .Where(x => x.RouteListItem.Id == address.Id)
                        .SingleOrDefault();

                if (doc != null && doc.Amount == address.Order.ActualTotalSum) {
                    continue;
                }

                if (doc != null && doc.Amount != address.Order.ActualTotalSum)
                {
                    var oldSum = doc.Amount;

                    doc.Amount = doc.Amount + amount >= address.Order.ActualTotalSum
                        ? address.Order.ActualTotalSum
                        : doc.Amount + amount;
                    doc.LastEditedTime = DateTime.Now;
                    //doc.LastEditor = ;
                    doc.OrganisationCashMovementOperation.Amount = doc.Amount;

                    Save(uow, doc.OrganisationCashMovementOperation, doc);

                    amount -= address.Order.ActualTotalSum - oldSum;
                    
                    if (amount <= 0) {
                        break;
                    }
                    
                    continue;
                }
                
                var operation = CreateOrganisationCashMovementOperation(uow, address);
                operation.Amount = amount > address.Order.ActualTotalSum
                    ? address.Order.ActualTotalSum
                    : amount;
                
                var routeListItemCashdistributionDoc = CreateRouteListItemCashDistributionDocument(operation, address, income);
                routeListItemCashdistributionDoc.CashIncomeCategory = income.IncomeCategory;

                Save(uow, operation, routeListItemCashdistributionDoc);

                if (amount > address.Order.ActualTotalSum)
                {
                    amount -= address.Order.ActualTotalSum;
                }
                else
                {
                    break;
                }
            }
        }

        public void DistributeExpenseCash(IUnitOfWork uow, RouteList routeList, Expense expense, decimal amount)
        {
            var cashAddresses = 
                routeList.Addresses.Where(x => x.TotalCash > 0);
            
            foreach (var address in cashAddresses)
            {
                var doc =
                    uow.Session.QueryOver<RouteListItemCashDistributionDocument>()
                        .Where(x => x.RouteListItem.Id == address.Id)
                        .SingleOrDefault();

                if (doc != null && doc.Amount == address.Order.ActualTotalSum) {
                    continue;
                }

                if (doc != null && doc.Amount != address.Order.ActualTotalSum)
                {
                    var oldSum = doc.Amount;

                    doc.Amount = Math.Abs(doc.Amount - amount) >= address.Order.ActualTotalSum
                        ? -address.Order.ActualTotalSum
                        : doc.Amount - amount;
                    doc.LastEditedTime = DateTime.Now;
                    //doc.LastEditor = ;
                    doc.OrganisationCashMovementOperation.Amount = doc.Amount;

                    Save(uow, doc.OrganisationCashMovementOperation, doc);

                    amount -= address.Order.ActualTotalSum + oldSum;
                    
                    if (amount <= 0) {
                        break;
                    }
                    
                    continue;
                }

                var operation = CreateOrganisationCashMovementOperation(uow, address);
                operation.Amount = amount > address.Order.ActualTotalSum
                    ? -address.Order.ActualTotalSum
                    : -amount;
                
                var routeListItemCashdistributionDoc = CreateRouteListItemCashDistributionDocument(operation, address, expense);
                routeListItemCashdistributionDoc.CashExpenseCategory = expense.ExpenseCategory;
                
                Save(uow, operation, routeListItemCashdistributionDoc);

                if (amount > address.Order.ActualTotalSum) {
                    amount -= address.Order.ActualTotalSum;
                }
                else {
                    break;
                }
            }
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
                    : cashDistributionCommonOrganisationProvider.GetCommonOrganisation(uow),
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
                CashIncomeCategory = income.IncomeCategory,
                CashIncomeOperationType = income.TypeOperation,
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
                CashExpenseCategory = expense.ExpenseCategory,
                CashExpenseOperationType = expense.TypeOperation,
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