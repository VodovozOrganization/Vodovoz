using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrderOrganizationManager : IGetOrganizationForOrder
	{
		private readonly OrderOurOrganizationForOrderHandler _orderOurOrganization;
		private readonly OrganizationFromClientForOrderHandler _organizationFromClient;
		private readonly ContractOrganizationForOrderHandler _contractOrganization;
		private readonly OrganizationByOrderContentForOrderHandler _organizationByOrderContentForOrderHandler;
		private readonly OrganizationByPaymentTypeForOrderHandler _organizationByPaymentTypeForOrderHandler;

		private readonly Queue<IGetOrganizationForOrder> _handlers = new Queue<IGetOrganizationForOrder>();

		public OrderOrganizationManager(
			OrderOurOrganizationForOrderHandler orderOurOrganization,
			OrganizationFromClientForOrderHandler organizationFromClient,
			ContractOrganizationForOrderHandler contractOrganization,
			OrganizationByOrderContentForOrderHandler organizationByOrderContentForOrderHandler,
			OrganizationByPaymentTypeForOrderHandler organizationByPaymentTypeForOrderHandler
			)
		{
			_orderOurOrganization = orderOurOrganization ?? throw new ArgumentNullException(nameof(orderOurOrganization));
			_organizationFromClient = organizationFromClient ?? throw new ArgumentNullException(nameof(organizationFromClient));
			_contractOrganization = contractOrganization ?? throw new ArgumentNullException(nameof(contractOrganization));
			_organizationByOrderContentForOrderHandler =
				organizationByOrderContentForOrderHandler ?? throw new ArgumentNullException(nameof(organizationByOrderContentForOrderHandler));
			_organizationByPaymentTypeForOrderHandler =
				organizationByPaymentTypeForOrderHandler ?? throw new ArgumentNullException(nameof(organizationByPaymentTypeForOrderHandler));

			Initialize();
		}

		private void Initialize()
		{
			AddHandler(_orderOurOrganization);
			AddHandler(_organizationFromClient);
			AddHandler(_contractOrganization);
			AddHandler(_organizationByOrderContentForOrderHandler);
			AddHandler(_organizationByPaymentTypeForOrderHandler);
		}
		
		public IEnumerable<OrganizationForOrderWithOrderItems> GetOrganizationsWithOrderItems(
			Order order,
			IUnitOfWork uow = null)
		{
			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}
			
			IEnumerable<OrganizationForOrderWithOrderItems> organizationsWithGoods = null;
			
			while(organizationsWithGoods is null || !organizationsWithGoods.Any())
			{
				if(_handlers.Any())
				{
					organizationsWithGoods =
						_handlers
							.Dequeue()
							.GetOrganizationsWithOrderItems(order, uow);
				}
				else
				{
					break;
				}
			}

			return organizationsWithGoods;
		}

		public void AddHandler(IGetOrganizationForOrder handler)
		{
			_handlers.Enqueue(handler);
		}
	}
	
	/*public interface IManager : IGetOrganizationForOrder
	{
		void AddHandler(IGetOrganizationForOrder handler);
	}*/
}
