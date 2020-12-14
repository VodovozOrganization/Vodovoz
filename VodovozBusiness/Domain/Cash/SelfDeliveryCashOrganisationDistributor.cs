using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Documents;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;

namespace Vodovoz.Domain.Cash
{
    public class SelfDeliveryCashOrganisationDistributor
    {
        private readonly ICashDistributionCommonOrganisationProvider cashDistributionCommonOrganisationProvider;
        private readonly ISelfDeliveryCashDistributionDocumentRepository selfDeliveryCashDistributionDocumentRepository;
        private readonly IOrderRepository orderRepository;

        public SelfDeliveryCashOrganisationDistributor(
            ICashDistributionCommonOrganisationProvider cashDistributionCommonOrganisationProvider,
            ISelfDeliveryCashDistributionDocumentRepository selfDeliveryCashDistributionDocumentRepository,
            IOrderRepository orderRepository)
        {
            this.cashDistributionCommonOrganisationProvider =
                cashDistributionCommonOrganisationProvider ?? throw new ArgumentNullException(nameof(cashDistributionCommonOrganisationProvider));
            this.selfDeliveryCashDistributionDocumentRepository =
                selfDeliveryCashDistributionDocumentRepository ?? throw new ArgumentNullException(nameof(selfDeliveryCashDistributionDocumentRepository));
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public void DistributeIncomeCash(IUnitOfWork uow, Order selfDeliveryOrder, Income income)
        {
            var operation = CreateOrganisationCashMovementOperation(uow, selfDeliveryOrder, income);
            var selfDeliveryCashDistributionDoc = CreateSelfDeliveryCashDistributionDocument(operation, selfDeliveryOrder, income);

            Save(uow, operation, selfDeliveryCashDistributionDoc);
        }

        public void DistributeExpenseCash(IUnitOfWork uow, Order selfDeliveryOrder, Expense expense)
        {
            var operation = CreateOrganisationCashMovementOperation(uow, selfDeliveryOrder, expense);
            var selfDeliveryCashDistributionDoc = CreateSelfDeliveryCashDistributionDocument(operation, selfDeliveryOrder, expense);

            Save(uow, operation, selfDeliveryCashDistributionDoc);
        }

        private OrganisationCashMovementOperation CreateOrganisationCashMovementOperation(
            IUnitOfWork uow, Order selfDeliveryOrder, Expense expense)
        {
            var hasReceipt = orderRepository.OrderHasSentReceipt(uow, selfDeliveryOrder.Id);

            return new OrganisationCashMovementOperation
            {
                OperationTime = DateTime.Now,
                Organisation = hasReceipt
                    ? selfDeliveryOrder.Contract.Organization
                    : cashDistributionCommonOrganisationProvider.GetCommonOrganisation(uow),
                Amount = -expense.Money
            };
        }
        
        private OrganisationCashMovementOperation CreateOrganisationCashMovementOperation(
            IUnitOfWork uow, Order selfDeliveryOrder, Income income)
        {
            var hasReceipt = orderRepository.OrderHasSentReceipt(uow, selfDeliveryOrder.Id);

            return new OrganisationCashMovementOperation
            {
                OperationTime = DateTime.Now,
                Organisation = hasReceipt
                    ? selfDeliveryOrder.Contract.Organization
                    : cashDistributionCommonOrganisationProvider.GetCommonOrganisation(uow),
                Amount = income.Money
            };
        }

        private SelfDeliveryCashDistributionDocument CreateSelfDeliveryCashDistributionDocument(
            OrganisationCashMovementOperation operation, Order selfDeliveryOrder, Expense expense)
        {
            return new SelfDeliveryCashDistributionDocument
            {
                Organisation = operation.Organisation,
                CreationDate = DateTime.Now,
                Author = expense.Casher,
                LastEditedTime = DateTime.Now,
                LastEditor = expense.Casher,
                Employee = expense.Employee,
                Order = selfDeliveryOrder,
                Expense = expense,
                OrganisationCashMovementOperation = operation,
                Amount = operation.Amount
            };
        }
        
        private SelfDeliveryCashDistributionDocument CreateSelfDeliveryCashDistributionDocument(
            OrganisationCashMovementOperation operation, Order selfDeliveryOrder, Income income)
        {
            return new SelfDeliveryCashDistributionDocument
            {
                Organisation = operation.Organisation,
                CreationDate = DateTime.Now,
                Author = income.Casher,
                LastEditedTime = DateTime.Now,
                LastEditor = income.Casher,
                Employee = income.Employee,
                Order = selfDeliveryOrder,
                Income = income,
                OrganisationCashMovementOperation = operation,
                Amount = operation.Amount
            };
        }
        
        private void Save(IUnitOfWork uow, OrganisationCashMovementOperation operation,
            SelfDeliveryCashDistributionDocument selfDeliveryCashDistributionDoc)
        {
            uow.Save(operation);
            uow.Save(selfDeliveryCashDistributionDoc);
        }

        public void UpdateRecords(IUnitOfWork uow, Order selfDeliveryOrder, Income income, Employee editor)
        {
            var selfDeliveryCashDistributionDoc = 
                selfDeliveryCashDistributionDocumentRepository.GetSelfDeliveryCashDistributionDocument(uow, selfDeliveryOrder.Id);

            if (selfDeliveryCashDistributionDoc == null) return;

            UpdateDocument(uow, selfDeliveryCashDistributionDoc, income, selfDeliveryOrder, editor);
            UpdateOperation(selfDeliveryCashDistributionDoc);
            Save(uow, selfDeliveryCashDistributionDoc.OrganisationCashMovementOperation, selfDeliveryCashDistributionDoc);
        }
        
        public void UpdateRecords(IUnitOfWork uow, Order selfDeliveryOrder, Expense expense, Employee editor)
        {
            var selfDeliveryCashDistributionDoc = 
                selfDeliveryCashDistributionDocumentRepository.GetSelfDeliveryCashDistributionDocument(uow, selfDeliveryOrder.Id);

            if (selfDeliveryCashDistributionDoc == null) return;

            UpdateDocument(uow, selfDeliveryCashDistributionDoc, expense, selfDeliveryOrder, editor);
            UpdateOperation(selfDeliveryCashDistributionDoc);
            Save(uow, selfDeliveryCashDistributionDoc.OrganisationCashMovementOperation, selfDeliveryCashDistributionDoc);
        }

        private void UpdateOperation(SelfDeliveryCashDistributionDocument selfDeliveryCashDistributionDocument)
        {
            selfDeliveryCashDistributionDocument.OrganisationCashMovementOperation.OperationTime = DateTime.Now;
            selfDeliveryCashDistributionDocument.OrganisationCashMovementOperation.Organisation =
                selfDeliveryCashDistributionDocument.Organisation;
            selfDeliveryCashDistributionDocument.OrganisationCashMovementOperation.Amount = 
                selfDeliveryCashDistributionDocument.Amount;
        }

        private void UpdateDocument(IUnitOfWork uow, SelfDeliveryCashDistributionDocument selfDeliveryCashDistributionDocument,
            Income income, Order selfDeliveryOrder, Employee editor)
        {
            selfDeliveryCashDistributionDocument.LastEditedTime = DateTime.Now;
            selfDeliveryCashDistributionDocument.LastEditor = editor;
            selfDeliveryCashDistributionDocument.Amount = income.Money;

            if (selfDeliveryCashDistributionDocument.Order.Id != selfDeliveryOrder.Id) {
                var hasReceipt = orderRepository.OrderHasSentReceipt(uow, selfDeliveryOrder.Id);
                
                selfDeliveryCashDistributionDocument.Order = selfDeliveryOrder;
                selfDeliveryCashDistributionDocument.Organisation = hasReceipt
                    ? selfDeliveryOrder.Contract.Organization
                    : cashDistributionCommonOrganisationProvider.GetCommonOrganisation(uow);
            }
        }
        
        private void UpdateDocument(IUnitOfWork uow, SelfDeliveryCashDistributionDocument selfDeliveryCashDistributionDocument,
            Expense expense, Order selfDeliveryOrder, Employee editor)
        {
            selfDeliveryCashDistributionDocument.LastEditedTime = DateTime.Now;
            selfDeliveryCashDistributionDocument.LastEditor = editor;
            selfDeliveryCashDistributionDocument.Amount = -expense.Money;
            
            if (selfDeliveryCashDistributionDocument.Order.Id != selfDeliveryOrder.Id) {
                var hasReceipt = orderRepository.OrderHasSentReceipt(uow, selfDeliveryOrder.Id);
                
                selfDeliveryCashDistributionDocument.Order = selfDeliveryOrder;
                selfDeliveryCashDistributionDocument.Organisation = hasReceipt
                    ? selfDeliveryOrder.Contract.Organization
                    : cashDistributionCommonOrganisationProvider.GetCommonOrganisation(uow);
            }
        }
    }
}