using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Settings;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

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
		/// <param name="organizationChoice">Данные для подбора организации</param>
		/// <param name="processingProducts">Обрабатываемые товары</param>
		/// <param name="uow">unit of work</param>
		/// <returns></returns>
		public IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			IUnitOfWork uow,
			TimeSpan requestTime,
			Dictionary<short, OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> setsOrganizations,
			OrderOrganizationChoice organizationChoice,
			IEnumerable<IProduct> processingProducts)
		{
			var result = new List<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits>();
			var organizationByOrderAuthorSettings = uow.GetAll<OrganizationByOrderAuthorSettings>().SingleOrDefault();

			if(organizationByOrderAuthorSettings is null)
			{
				//TODO проверить условие
				return setsOrganizations.Values;
			}

			if(ContainsSubdivision(organizationChoice.AuthorSubdivision, organizationByOrderAuthorSettings.OrderAuthorsSubdivisions))
			{
				if(setsOrganizations.TryGetValue(
					organizationByOrderAuthorSettings.OrganizationBasedOrderContentSettings.OrderContentSet, out var setSettings))
				{
					UpdateOrganizationOrderItems(processingProducts, setSettings);
					return setsOrganizations.Values;
				}

				//подумать, что делаем если каким-то образом там нет такого множества

				return result;
			}

			if(setsOrganizations.TryGetValue(OrganizationByOrderAuthorSettings.DefaultSetForAuthorNotIncludedSet, out var defaultSetSettings))
			{
				UpdateOrganizationOrderItems(processingProducts, defaultSetSettings);
				return result;
			}

			var org = _organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice);
	
			foreach(var orgWithOrderItems in setsOrganizations.Values)
			{
				if(orgWithOrderItems.Organization.Id != org.Id)
				{
					continue;
				}
				
				UpdateOrganizationOrderItems(processingProducts, orgWithOrderItems);
				return setsOrganizations.Values;
			}
			
			//TODO проверить условие
			result.AddRange(setsOrganizations.Values);
			result.Add(new OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(org, processingProducts));

			return result;
		}

		private bool ContainsSubdivision(Subdivision authorSubdivision, IEnumerable<Subdivision> setSubdivisions) => 
			authorSubdivision != null
			&& setSubdivisions.Any(authorSubdivision.IsChildOf);
		
		private void UpdateOrganizationOrderItems(
			IEnumerable<IProduct> processingProducts,
			OrganizationForOrderWithGoodsAndEquipmentsAndDeposits setSettings)
		{
			var list = setSettings.Goods as List<IProduct>;
			list.AddRange(processingProducts);
		}
	}
}
