using System;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Tools;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.Receipts;

namespace Vodovoz.Application.Orders.Services
{
	public class OrderContractUpdater : IOrderContractUpdater
	{
		private readonly IOrderOrganizationManager _orderOrganizationManager;
		private readonly IOrderReceiptHandler _orderReceiptHandler;
		private readonly ICounterpartyContractRepository _contractRepository;

		public OrderContractUpdater(
			IOrderOrganizationManager orderOrganizationManager,
			IOrderReceiptHandler orderReceiptHandler,
			ICounterpartyContractRepository contractRepository)
		{
			_orderOrganizationManager = orderOrganizationManager ?? throw new ArgumentNullException(nameof(orderOrganizationManager));
			_orderReceiptHandler = orderReceiptHandler ?? throw new ArgumentNullException(nameof(orderReceiptHandler));
			_contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
		}
		
		public void UpdateContract(
			IUnitOfWork uow,
			Order order,
			bool onPaymentTypeChanged = false)
		{
			//Если Initialize вызывается при создании сущности NHibernate'ом,
			//то почему-то не загружаются OrderItems и OrderDocuments (А возможно и вообще все коллекции Order)
			if(!NHibernateUtil.IsInitialized(order.Client))
			{
				NHibernateUtil.Initialize(order.Client);
			}
			if(!NHibernateUtil.IsInitialized(order.Contract))
			{
				NHibernateUtil.Initialize(order.Contract);
			}
			if(!NHibernateUtil.IsInitialized(order.Client) || !NHibernateUtil.IsInitialized(order.Contract))
			{
				return;
			}

			if(order.CreateDate != null
				&& order.CreateDate <= new DateTime(2020, 12, 16)
				&& order.Contract != null
				&& !onPaymentTypeChanged
				&& order.Contract.Counterparty == order.Client)
			{
				return;
			}

			ForceUpdateContract(uow, order);
		}
		
		public void ForceUpdateContract(
			IUnitOfWork uow,
			Order order,
			Organization organization = null)
		{
			UpdateOrCreateContract(uow, order, organization);
		}
		
		public void UpdateOrCreateContract(
			IUnitOfWork uow,
			Order order,
			Organization organization = null)
		{
			if(!NHibernateUtil.IsInitialized(order.Client))
			{
				NHibernateUtil.Initialize(order.Client);
			}
			if(!NHibernateUtil.IsInitialized(order.Contract))
			{
				NHibernateUtil.Initialize(order.Contract);
			}

			if(order.Client == null)
			{
				return;
			}

			if(organization is null)
			{
				organization = GetOrganization(uow, order);
			}

			//TODO: перепроверить условие
			var counterpartyContract = organization != null 
				? GetCounterpartyContractByOrganization(uow, order, organization)
				: GetCounterpartyContract(uow, order, ErrorReporter.Instance);

			if(counterpartyContract == null)
			{
				counterpartyContract = CreateContract(uow, order, organization);
			}
			else
			{
				if(order.DeliveryDate.HasValue && order.DeliveryDate.Value < counterpartyContract.IssueDate)
				{
					counterpartyContract.IssueDate = order.DeliveryDate.Value;
				}
			}

			order.Contract = counterpartyContract;

			for(var i = 0; i < order.OrderItems.Count; i++)
			{
				order.OrderItems[i].CalculateVATType();
			}
			
			order.UpdateContractDocument();
			order.UpdateDocuments();
		}

		private Organization GetOrganization(IUnitOfWork uow, Order order)
		{
			var firstPartOrder = _orderOrganizationManager
				.SplitOrderByOrganizations(uow, DateTime.Now.TimeOfDay, OrderOrganizationChoice.Create(order))
				.FirstOrDefault();

			if(firstPartOrder is null)
			{
				throw new NullReferenceException(
					$"Не удалось получить организацию для заказа. Скорее всего не хватает какой-то настройки {nameof(ISplitOrderByOrganizations)}");
			}
				
			return firstPartOrder.Organization;
		}

		public CounterpartyContract CreateContract(IUnitOfWork unitOfWork, Order order, Organization organization = null)
		{
			var contractType = CounterpartyContractEntity.GetContractTypeForPaymentType(order.Client.PersonType, order.PaymentType);
			var org = organization ?? GetOrganization(unitOfWork, order);

			var contract = new CounterpartyContract
			{
				Counterparty = order.Client,
				Organization = org,
				IsArchive = false,
				ContractType = contractType
			};
			
			contract.UpdateNumber();

			if(order.DeliveryDate.HasValue)
			{
				contract.IssueDate = order.DeliveryDate.Value;
			}

			return contract;
		}
		
		private CounterpartyContract GetCounterpartyContract(IUnitOfWork uow, Order order, IErrorReporter errorReporter = null)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			var changedContract = GetChangedCounterpartyContract(uow, order, errorReporter);
			if(changedContract == order.Contract)
			{
				return order.Contract;
			}

			_orderReceiptHandler.RenewOrderCashReceipts(uow, order);

			return changedContract;
		}
		
		private CounterpartyContract GetChangedCounterpartyContract(IUnitOfWork uow, Order order, IErrorReporter errorReporter = null)
		{
			if(order.Client == null)
			{
				return null;
			}

			var cantChangeContract = CantChangeContractWithReceipts(order);
			if(cantChangeContract)
			{
				return order.Contract;
			}

			var personType = order.Client.PersonType;
			var paymentType = order.PaymentType;
			var contractType = CounterpartyContractEntity.GetContractTypeForPaymentType(personType, paymentType);

			try
			{
				var organization = GetOrganization(uow, order);

				if(organization == null)
				{
					return null;
				}

				var result = _contractRepository.GetActiveContractsWithOrganization(
					uow, order.Client, organization, contractType, true);
				
				var firstResult = result.FirstOrDefault();

				if(result.Count > 1 && errorReporter != null)
				{
					Exception ex = new ArgumentException("Query returned > 1 CounterpartyContract");
					errorReporter.AutomaticSendErrorReport(
						"В базе содержится больше одного контракта:" +
						$" клиент {firstResult.Counterparty.Id} тип {firstResult.ContractType} организация {firstResult.Organization.Name}",
						ex);
				}

				return firstResult;

			}
			catch(NotSupportedException)
			{
				return null;
			}
		}
		
		private CounterpartyContract GetCounterpartyContractByOrganization(IUnitOfWork uow, Order order, Organization organization)
		{
			var changedContract = GetChangedCounterpartyContractByOrganization(uow, order, organization);
			if(changedContract == order.Contract)
			{
				return order.Contract;
			}

			_orderReceiptHandler.RenewOrderCashReceipts(uow, order);

			return changedContract;
		}
		
		private CounterpartyContract GetChangedCounterpartyContractByOrganization(IUnitOfWork uow, Order order, Organization organization)
		{
			var cantChangeContract = CantChangeContractWithReceipts(order);
			
			if(cantChangeContract)
			{
				return order.Contract;
			}

			var personType = order.Client.PersonType;
			var paymentType = order.PaymentType;
			var contractType = CounterpartyContractEntity.GetContractTypeForPaymentType(personType, paymentType);

			var result = _contractRepository.GetActiveContractsWithOrganization(
				uow, order.Client, organization, contractType, true);
			
			return result.FirstOrDefault();
		}
		
		private bool CantChangeContractWithReceipts(Order order)
		{
			if(order.Contract == null)
			{
				return false;
			}

			if(order.Id == 0)
			{
				return false;
			}

			return _orderReceiptHandler.HasNeededReceipt(order.Id);
		}
	}
}
