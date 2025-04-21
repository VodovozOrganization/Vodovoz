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
		/// <param name="setsPartsOrders"></param>
		/// <param name="organizationChoice">Данные для подбора организации</param>
		/// <param name="processingProducts">Обрабатываемые товары</param>
		/// <param name="uow">unit of work</param>
		/// <returns></returns>
		public IEnumerable<PartOrderWithGoods> SplitOrderByOrganizations(
			IUnitOfWork uow,
			TimeSpan requestTime,
			Dictionary<short, PartOrderWithGoods> setsPartsOrders,
			OrderOrganizationChoice organizationChoice,
			IEnumerable<IProduct> processingProducts)
		{
			var organizationByOrderAuthorSettings = uow.GetAll<OrganizationByOrderAuthorSettings>().SingleOrDefault();

			if(organizationByOrderAuthorSettings is null)
			{
				CreateNewPartOrderOrUpdateExisting(uow, requestTime, setsPartsOrders, organizationChoice, processingProducts);
				return setsPartsOrders.Values;
			}

			if(ContainsSubdivision(organizationChoice.AuthorSubdivision, organizationByOrderAuthorSettings.OrderAuthorsSubdivisions))
			{
				if(setsPartsOrders.TryGetValue(
					organizationByOrderAuthorSettings.OrganizationBasedOrderContentSettings.OrderContentSet, out var setSettings))
				{
					UpdatePartOrder(processingProducts, setSettings);
					return setsPartsOrders.Values;
				}

				throw new InvalidOperationException("Organization by order authors subdivision not found");
			}

			if(setsPartsOrders.TryGetValue(OrganizationByOrderAuthorSettings.DefaultSetForAuthorNotIncludedSet, out var defaultSetSettings))
			{
				UpdatePartOrder(processingProducts, defaultSetSettings);
				return setsPartsOrders.Values;
			}

			CreateNewPartOrderOrUpdateExisting(uow, requestTime, setsPartsOrders, organizationChoice, processingProducts);
			return setsPartsOrders.Values;
		}

		private void CreateNewPartOrderOrUpdateExisting(
			IUnitOfWork uow,
			TimeSpan requestTime,
			Dictionary<short, PartOrderWithGoods> setsPartsOrders,
			OrderOrganizationChoice organizationChoice,
			IEnumerable<IProduct> processingProducts)
		{
			var org = _organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice);

			foreach(var partOrder in setsPartsOrders.Values)
			{
				if(partOrder.Organization.Id != org.Id)
				{
					continue;
				}
				
				UpdatePartOrder(processingProducts, partOrder);
				return;
			}

			setsPartsOrders.Add(short.MaxValue, new PartOrderWithGoods(org, processingProducts));
		}

		private bool ContainsSubdivision(Subdivision authorSubdivision, IEnumerable<Subdivision> setSubdivisions) => 
			authorSubdivision != null
			&& setSubdivisions.Any(authorSubdivision.IsChildOf);
		
		private void UpdatePartOrder(
			IEnumerable<IProduct> processingProducts,
			PartOrderWithGoods partOrderFromSet)
		{
			var list = partOrderFromSet.Goods as List<IProduct>;
			list.AddRange(processingProducts);
		}
	}
}
