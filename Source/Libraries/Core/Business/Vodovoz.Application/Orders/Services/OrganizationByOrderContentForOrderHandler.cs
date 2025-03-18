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
	/// <summary>
	/// Обработчик для подбора организации исходя из товаров заказа
	/// </summary>
	public class OrganizationByOrderContentForOrderHandler : OrganizationForOrderHandler
	{
		private readonly OrganizationByOrderAuthorHandler _organizationByOrderAuthorHandler;
		private readonly OrganizationByPaymentTypeForOrderHandler _organizationByPaymentTypeForOrderHandler;
		private readonly IOrganizationForOrderFromSet _organizationForOrderFromSet;

		public OrganizationByOrderContentForOrderHandler(
			OrganizationByOrderAuthorHandler organizationByOrderAuthorHandler,
			OrganizationByPaymentTypeForOrderHandler organizationByPaymentTypeForOrderHandler,
			IOrganizationForOrderFromSet organizationForOrderFromSet)
		{
			_organizationByOrderAuthorHandler =
				organizationByOrderAuthorHandler ?? throw new ArgumentNullException(nameof(organizationByOrderAuthorHandler));
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
		/// <param name="order"></param>
		/// <param name="uow"></param>
		/// <returns></returns>
		public override IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow = null)
		{
			var result = new List<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits>();
			var setsOrganizations = new Dictionary<short, OrganizationForOrderWithGoodsAndEquipmentsAndDeposits>();
			
			var organizationsBasedOrderContent = uow.GetAll<OrganizationBasedOrderContentSettings>().ToArray();

			// может стоит убрать, т.к. ниже должно отработать правильно при таком раскладе
			if(!organizationsBasedOrderContent.Any())
			{
				//подбираем по форме оплаты
			}

			var processingOrderItems = order.OrderItems.ToList();
			var processingEquipments = order.OrderEquipments.ToList();
			
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
				var orderEquipmentsForOrganization = new List<OrderEquipment>();

				while(i < processingOrderItems.Count)
				{
					if(OrderItemBelongsSet(processingOrderItems[i], set))
					{
						orderItemsForOrganization.Add(processingOrderItems[i]);
						
						var dependentEquipments = processingEquipments.Where(
							x =>
								x.OrderItem.Id == processingOrderItems[i].Id
								|| x.OrderRentDepositItem.Id == processingOrderItems[i].Id
								|| x.OrderRentServiceItem.Id == processingOrderItems[i].Id)
							.ToList();

						foreach(var dependentEquipment in dependentEquipments)
						{
							orderEquipmentsForOrganization.Add(dependentEquipment);
							processingEquipments.Remove(dependentEquipment);
						}
						
						processingOrderItems.RemoveAt(i);
						continue;
					}

					i++;
				}

				if(!orderItemsForOrganization.Any())
				{
					continue;
				}

				if(set.OrderContentSet == 1 && processingEquipments.Any())
				{
					orderEquipmentsForOrganization.AddRange(processingEquipments.Where(
						x => x.OrderItem == null
						&& x.OrderRentDepositItem == null
						&& x.OrderRentServiceItem == null));
				}

				var org =
					_organizationForOrderFromSet.GetOrganizationForOrderFromSet(DateTime.Now.TimeOfDay, set)
					?? _organizationByPaymentTypeForOrderHandler.GetOrganizationForOrder(requestTime, order, uow);
				
				setsOrganizations.Add(
					set.OrderContentSet,
					new OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(org, orderItemsForOrganization, orderEquipmentsForOrganization));
			}

			if(!processingOrderItems.Any())
			{
				if(!setsOrganizations.Any())
				{
					var org = _organizationByPaymentTypeForOrderHandler.GetOrganizationForOrder(requestTime, order, uow);
					result.Add(new OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(org, null));
					return result;
				}

				result.AddRange(setsOrganizations.Values);
				return result;
			}

			var authorsResult = _organizationByOrderAuthorHandler.GetOrganizationsWithOrderItems(
				requestTime, setsOrganizations, order, processingOrderItems, uow);

			if(authorsResult.Any())
			{
				return authorsResult;
			}
			
			return base.GetOrganizationsWithOrderItems(requestTime, order, uow);
			
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

		private bool ContainsProductGroup(ProductGroup itemProductGroup, IEnumerable<ProductGroup> settingsProductGroups) =>
			itemProductGroup != null
			&& settingsProductGroups.Any(settingsProductGroup => ContainsProductGroup(itemProductGroup, settingsProductGroup));
		
		private bool ContainsProductGroup(ProductGroup itemProductGroup, ProductGroup settingsProductGroup)
		{
			while(true)
			{
				if(itemProductGroup.Id == settingsProductGroup.Id)
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
