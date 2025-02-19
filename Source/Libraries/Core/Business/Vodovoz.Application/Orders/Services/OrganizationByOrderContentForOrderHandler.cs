using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	public class OrganizationByOrderContentForOrderHandler : IGetOrganizationForOrder
	{
		private readonly OrganizationByOrderAuthorHandler _organizationByOrderAuthorHandler;
		private readonly OrganizationByPaymentTypeForOrderHandler _organizationByPaymentTypeForOrderHandler;
		
		public OrganizationByOrderContentForOrderHandler(
			OrganizationByOrderAuthorHandler organizationByOrderAuthorHandler,
			OrganizationByPaymentTypeForOrderHandler organizationByPaymentTypeForOrderHandler)
		{
			_organizationByOrderAuthorHandler =
				organizationByOrderAuthorHandler ?? throw new ArgumentNullException(nameof(organizationByOrderAuthorHandler));
			_organizationByPaymentTypeForOrderHandler =
				organizationByPaymentTypeForOrderHandler
				?? throw new ArgumentNullException(nameof(organizationByPaymentTypeForOrderHandler));
		}
		
		public IEnumerable<OrganizationForOrderWithOrderItems> GetOrganizationsWithOrderItems(
			Order order,
			IUnitOfWork uow = null)
		{
			var result = new List<OrganizationForOrderWithOrderItems>();
			var setsOrganizations = new Dictionary<short, OrganizationForOrderWithOrderItems>();
			
			var organizationsBasedOrderContent = uow.GetAll<OrganizationBasedOrderContentSettings>().ToArray();

			// может стоит убрать, т.к. ниже должно отработать правильно при таком раскладе
			if(!organizationsBasedOrderContent.Any())
			{
				//подбираем по форме оплаты
			}

			var processingOrderItems = order.OrderItems.ToList();
			
			/*	1 - заполнено первое множество
				1.1 - с организацией
				1.2 - без организации
				2 - заполнены оба множества
				2.1 - оба без организаций
				2.2 - первое с, второе без
				2.3 - первое без второе с
				2.4 - оба с организациями
			 */
			
			foreach(var set in organizationsBasedOrderContent)
			{
				if(!set.Nomenclatures.Any() && !set.ProductGroups.Any())
				{
					continue;
				}
				
				var i = 0;
				var orderItemsForOrganization = new List<OrderItem>();

				while(i < processingOrderItems.Count)
				{
					if(OrderItemBelongsSet(processingOrderItems[i], set))
					{
						orderItemsForOrganization.Add(processingOrderItems[i]);
						processingOrderItems.RemoveAt(i);
					}

					i++;
				}

				if(!orderItemsForOrganization.Any())
				{
					continue;
				}

				var org =
					set.Organization ?? _organizationByPaymentTypeForOrderHandler.GetOrganizationForOrder(order, uow);
				
				setsOrganizations.Add(set.OrderContentSet, new OrganizationForOrderWithOrderItems(org, orderItemsForOrganization));
			}

			if(!processingOrderItems.Any())
			{
				if(!setsOrganizations.Any())
				{
					var org = _organizationByPaymentTypeForOrderHandler.GetOrganizationForOrder(order, uow);
					result.Add(new OrganizationForOrderWithOrderItems(org, null));
					return result;
				}

				result.AddRange(setsOrganizations.Values);
				return result;
			}

			return _organizationByOrderAuthorHandler.GetOrganizationsWithOrderItems(
				setsOrganizations, order, processingOrderItems, uow);
			
			//TODO это должно вызываться в менеджере последним из обработчиков
			/*var orgByPaymentType = _organizationByPaymentTypeForOrderHandler.GetOrganizationForOrder(order, uow, paymentType);

			foreach(var orgWithOrderItems in setsOrganizations.Values)
			{
				if(orgWithOrderItems.Organization.Id == orgByPaymentType.Id)
				{
					var list = orgWithOrderItems.orderItems as List<OrderItem>;
					list.AddRange(processingOrderItems);
					
					result.AddRange(setsOrganizations.Values);
					return result;
				}
			}
			
			result.AddRange(setsOrganizations.Values);
			result.Add((orgByPaymentType, processingOrderItems));

			return result;*/
		}
		
		private bool OrderItemBelongsSet(OrderItem orderItem, OrganizationBasedOrderContentSettings organizationBasedOrderContentSettings)
		{
			if(organizationBasedOrderContentSettings.Nomenclatures.Contains(orderItem.Nomenclature))
			{
				return true;
			}

			if(ContainsProductGroup(orderItem.Nomenclature.ProductGroup, organizationBasedOrderContentSettings.ProductGroups))
			{
				return true;
			}
			
			return false;
		}

		private bool ContainsProductGroup(ProductGroup itemProductGroup, IEnumerable<ProductGroup> productGroups) =>
			itemProductGroup != null
			&& productGroups.Any(discountProductGroup => ContainsProductGroup(itemProductGroup, discountProductGroup));
		
		private bool ContainsProductGroup(ProductGroup itemProductGroup, ProductGroup productGroup)
		{
			while(true)
			{
				if(itemProductGroup.Id == productGroup.Id)
				{
					return true;
				}

				if(itemProductGroup.Parent != null)
				{
					itemProductGroup = itemProductGroup.Parent;
					continue;
				}

				return false;
			}
		}
	}
}
