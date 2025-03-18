using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Обработчик для подбора организации по авторам заказа
	/// </summary>
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="setsOrganizations"></param>
		/// <param name="order"></param>
		/// <param name="processingOrderItems"></param>
		/// <param name="uow"></param>
		/// <returns></returns>
		public IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			TimeSpan requestTime,
			Dictionary<short, OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> setsOrganizations,
			Order order,
			IEnumerable<OrderItem> processingOrderItems,
			IUnitOfWork uow)
		{
			var result = new List<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits>();
			var organizationByOrderAuthorSettings = uow.GetAll<OrganizationByOrderAuthorSettings>().SingleOrDefault();

			if(organizationByOrderAuthorSettings is null)
			{
				//TODO проверить условие
				return setsOrganizations.Values;
			}

			if(ContainsSubdivision(order.Author.Subdivision, organizationByOrderAuthorSettings.OrderAuthorsSubdivisions))
			{
				if(setsOrganizations.TryGetValue(
					organizationByOrderAuthorSettings.OrganizationBasedOrderContentSettings.OrderContentSet, out var setSettings))
				{
					UpdateOrganizationOrderItems(processingOrderItems, setSettings);
					return setsOrganizations.Values;
				}

				//подумать, что делаем если каким-то образом там нет такого множества

				return result;
			}

			if(setsOrganizations.TryGetValue(OrganizationByOrderAuthorSettings.DefaultSetForAuthorNotIncludedSet, out var defaultSetSettings))
			{
				UpdateOrganizationOrderItems(processingOrderItems, defaultSetSettings);
				return result;
			}

			var org = _organizationByPaymentTypeForOrderHandler.GetOrganizationForOrder(requestTime, order, uow);
	
			foreach(var orgWithOrderItems in setsOrganizations.Values)
			{
				if(orgWithOrderItems.Organization.Id != org.Id)
				{
					continue;
				}
				
				UpdateOrganizationOrderItems(processingOrderItems, orgWithOrderItems);
				return setsOrganizations.Values;
			}
			
			//TODO проверить условие
			result.AddRange(setsOrganizations.Values);
			result.Add(new OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(org, processingOrderItems));

			return result;
		}

		private bool ContainsSubdivision(Subdivision authorSubdivision, IEnumerable<Subdivision> setSubdivisions) => 
			authorSubdivision != null
			&& setSubdivisions.Any(authorSubdivision.IsChildOf);
		
		private void UpdateOrganizationOrderItems(
			IEnumerable<OrderItem> processingOrderItems,
			OrganizationForOrderWithGoodsAndEquipmentsAndDeposits setSettings)
		{
			var list = setSettings.OrderItems as List<OrderItem>;
			list.AddRange(processingOrderItems);
		}
	}
}
