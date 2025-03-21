using System;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Factories;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrderContractUpdater : IOrderContractUpdater
	{
		private readonly IGetOrganizationForOrder _orderOrganizationManager;
		private readonly ICounterpartyContractFactory _contractFactory;
		private readonly ICounterpartyContractRepository _contractRepository;

		public OrderContractUpdater(
			IGetOrganizationForOrder orderOrganizationManager,
			ICounterpartyContractFactory contractFactory,
			ICounterpartyContractRepository contractRepository)
		{
			_orderOrganizationManager = orderOrganizationManager ?? throw new ArgumentNullException(nameof(orderOrganizationManager));
			_contractFactory = contractFactory ?? throw new ArgumentNullException(nameof(contractFactory));
			_contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
		}
		
		public void UpdateContract(IUnitOfWork uow, Order order, bool onPaymentTypeChanged = false)
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
		
		public void ForceUpdateContract(IUnitOfWork uow, Order order, Organization organization = null)
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

			/*OrganizationsByOrderItems =
				ScopeProvider.Scope.Resolve<IGetOrganizationForOrder>()
					.GetOrganizationsWithOrderItems(DateTime.Now.TimeOfDay, order, uow);

			if(organization is null)
			{
				organization = OrganizationsByOrderItems.FirstOrDefault().Organization;
			}*/

			//TODO перепроверить условие
			var counterpartyContract = _contractRepository.GetCounterpartyContractByOrganization(uow, order, organization);
			//: contractRepository.GetCounterpartyContract(uow, this, ErrorReporter.Instance);

			if(counterpartyContract == null)
			{
				counterpartyContract = _contractFactory.CreateContract(uow, order, organization);
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
	}
}
