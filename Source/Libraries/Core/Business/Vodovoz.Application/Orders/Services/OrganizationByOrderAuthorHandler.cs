using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
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
		private readonly IOrganizationForOrderFromSet _organizationForOrderFromSet;

		public OrganizationByOrderAuthorHandler(
			OrganizationByPaymentTypeForOrderHandler organizationByPaymentTypeForOrderHandler,
			IOrganizationForOrderFromSet organizationForOrderFromSet)
		{
			_organizationByPaymentTypeForOrderHandler =
				organizationByPaymentTypeForOrderHandler
				?? throw new ArgumentNullException(nameof(organizationByPaymentTypeForOrderHandler));
			_organizationForOrderFromSet =
				organizationForOrderFromSet ?? throw new ArgumentNullException(nameof(organizationForOrderFromSet));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="setsPartsOrders"></param>
		/// <param name="organizationChoice">Данные для подбора организации</param>
		/// <param name="processingProducts">Обрабатываемые товары</param>
		/// <param name="uow">unit of work</param>
		/// <param name="processingEquipments">Обрабатываемое оборудование</param>
		/// <returns></returns>
		public IEnumerable<PartOrderWithGoods> SplitOrderByOrganizations(
			IUnitOfWork uow,
			TimeSpan requestTime,
			Dictionary<short, PartOrderWithGoods> setsPartsOrders,
			OrderOrganizationChoice organizationChoice,
			IEnumerable<IProduct> processingProducts,
			IEnumerable<OrderEquipment> processingEquipments)
		{
			var organizationByOrderAuthorSettings = uow.GetAll<OrganizationByOrderAuthorSettings>().SingleOrDefault();

			if(organizationByOrderAuthorSettings is null)
			{
				CreateNewPartOrderOrUpdateExisting(
					uow, requestTime, setsPartsOrders, organizationChoice, processingProducts, processingEquipments);
				return setsPartsOrders.Values;
			}

			if(ContainsSubdivision(organizationChoice.AuthorSubdivision, organizationByOrderAuthorSettings.OrderAuthorsSubdivisions))
			{
				var orderContentSet = organizationByOrderAuthorSettings.OrganizationBasedOrderContentSettings.OrderContentSet;
				
				if(!setsPartsOrders.TryGetValue(orderContentSet, out var partOrderWithGoods))
				{
					var setSettings =
						uow
							.GetAll<OrganizationBasedOrderContentSettings>()
							.FirstOrDefault(x => x.OrderContentSet == orderContentSet);

					var organization =
						_organizationForOrderFromSet.GetOrganizationForOrderFromSet(requestTime, setSettings, true)
						?? _organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice);
					
					CreateNewPartOrderOrUpdateExisting(
						uow, requestTime, setsPartsOrders, organizationChoice, processingProducts, processingEquipments, organization);
					return setsPartsOrders.Values;
				}

				UpdatePartOrder(processingProducts, processingEquipments, partOrderWithGoods);
				return setsPartsOrders.Values;
			}

			if(setsPartsOrders.TryGetValue(OrganizationByOrderAuthorSettings.DefaultSetForAuthorNotIncludedSet, out var defaultSetSettings))
			{
				UpdatePartOrder(processingProducts, processingEquipments, defaultSetSettings);
				return setsPartsOrders.Values;
			}

			CreateNewPartOrderOrUpdateExisting(
				uow, requestTime, setsPartsOrders, organizationChoice, processingProducts, processingEquipments);
			return setsPartsOrders.Values;
		}

		private void CreateNewPartOrderOrUpdateExisting(
			IUnitOfWork uow,
			TimeSpan requestTime,
			Dictionary<short, PartOrderWithGoods> setsPartsOrders,
			OrderOrganizationChoice organizationChoice,
			IEnumerable<IProduct> processingProducts,
			IEnumerable<OrderEquipment> processingEquipments,
			Organization organization = null)
		{
			if(organization is null)
			{
				organization = _organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice);
			}

			foreach(var partOrder in setsPartsOrders.Values)
			{
				if(partOrder.Organization.Id != organization.Id)
				{
					continue;
				}
				
				UpdatePartOrder(processingProducts, processingEquipments, partOrder);
				return;
			}

			setsPartsOrders.Add(short.MaxValue, new PartOrderWithGoods(organization, processingProducts, processingEquipments));
		}

		private bool ContainsSubdivision(Subdivision authorSubdivision, IEnumerable<Subdivision> setSubdivisions) => 
			authorSubdivision != null
			&& setSubdivisions.Any(x => x.Id == authorSubdivision.Id || authorSubdivision.IsChildOf(x));
		
		private void UpdatePartOrder(
			IEnumerable<IProduct> processingProducts,
			IEnumerable<OrderEquipment> processingEquipments,
			PartOrderWithGoods partOrderFromSet)
		{
			if(processingProducts.Any())
			{
				var list = partOrderFromSet.Goods as List<IProduct>;
				list?.AddRange(processingProducts);
			}

			if(processingEquipments.Any())
			{
				var list = partOrderFromSet.OrderEquipments as List<OrderEquipment>;
				list?.AddRange(processingEquipments);
			}
		}
	}
}
