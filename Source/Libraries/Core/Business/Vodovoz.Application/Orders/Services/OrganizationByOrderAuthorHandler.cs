using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	public class OrganizationByOrderAuthorHandler
	{
		private readonly OrganizationByPaymentTypeForOrderHandler _organizationByPaymentTypeForOrderHandler;
		
		public OrganizationByOrderAuthorHandler(
			OrganizationByPaymentTypeForOrderHandler organizationByPaymentTypeForOrderHandler)
		{
			_organizationByPaymentTypeForOrderHandler =
				organizationByPaymentTypeForOrderHandler
				?? throw new ArgumentNullException(nameof(organizationByPaymentTypeForOrderHandler));
		}
		
		public IEnumerable<OrganizationForOrderWithOrderItems> GetOrganizationsWithOrderItems(
			Dictionary<short, OrganizationForOrderWithOrderItems> setsOrganizations,
			Order order,
			IEnumerable<OrderItem> processingOrderItems,
			IUnitOfWork uow)
		{
			var result = new List<OrganizationForOrderWithOrderItems>();
			var organizationByOrderAuthorSettings = uow.GetAll<OrganizationByOrderAuthorSettings>().SingleOrDefault();

			if(organizationByOrderAuthorSettings is null)
			{
				return result;
			}

			if(ContainsSubdivision(order.Author.Subdivision, organizationByOrderAuthorSettings.OrderAuthorsSubdivisions))
			{
				if(setsOrganizations.TryGetValue(
					   organizationByOrderAuthorSettings.OrganizationBasedOrderContentSettings.OrderContentSet, out var setSettings))
				{
					var list = setSettings.OrderItems as List<OrderItem>;
					list.AddRange(processingOrderItems);
						
					result.AddRange(setsOrganizations.Values);
					return result;
				}

				//подумать, что делаем если каким-то образом там нет такого множества

				return result;
			}

			if(setsOrganizations.TryGetValue(OrganizationByOrderAuthorSettings.DefaultSetForAuthorNotIncludedSet, out var defaultSetSettings))
			{
				var list = defaultSetSettings.OrderItems as List<OrderItem>;
				list.AddRange(processingOrderItems);
					
				result.AddRange(setsOrganizations.Values);
				return result;
			}

			var org = _organizationByPaymentTypeForOrderHandler.GetOrganizationForOrder(order, uow);
				
			foreach(var orgWithOrderItems in setsOrganizations.Values)
			{
				if(orgWithOrderItems.Organization.Id != org.Id)
				{
					continue;
				}

				var list = orgWithOrderItems.OrderItems as List<OrderItem>;
				list.AddRange(processingOrderItems);
					
				result.AddRange(setsOrganizations.Values);
				return result;
			}
				
			result.AddRange(setsOrganizations.Values);
			result.Add(new OrganizationForOrderWithOrderItems(org, processingOrderItems));

			return result;
		}
		
		private bool ContainsSubdivision(Subdivision authorSubdivision, IEnumerable<Subdivision> setSubdivisions) => 
			authorSubdivision != null
			&& setSubdivisions.Any(authorSubdivision.IsChildOf);
	}
}
