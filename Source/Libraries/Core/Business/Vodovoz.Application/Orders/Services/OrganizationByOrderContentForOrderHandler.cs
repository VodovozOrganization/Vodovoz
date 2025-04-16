using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Settings;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

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

		public override IEnumerable<PartOrderWithGoods> GetOrganizationsWithOrderItems(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			var result = new List<PartOrderWithGoods>();
			var setsOrganizations = new Dictionary<short, PartOrderWithGoods>();
			
			var organizationsBasedOrderContent = uow.GetAll<OrganizationBasedOrderContentSettings>().ToArray();

			// может стоит убрать, т.к. ниже должно отработать правильно при таком раскладе
			if(!organizationsBasedOrderContent.Any())
			{
				//подбираем по форме оплаты
			}

			var processingGoods = organizationChoice.Goods.ToList();
			var processingEquipments = organizationChoice.OrderEquipments.ToList();
			
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
				var goodsForOrganization = new List<IProduct>();
				var orderEquipmentsForOrganization = new List<OrderEquipment>();

				while(i < processingGoods.Count)
				{
					if(OrderItemBelongsSet(processingGoods[i], set))
					{
						goodsForOrganization.Add(processingGoods[i]);
						
						var dependentEquipments = processingEquipments.Where(
							x =>
								x.OrderItem.Id == processingGoods[i].Id
								|| x.OrderRentDepositItem.Id == processingGoods[i].Id
								|| x.OrderRentServiceItem.Id == processingGoods[i].Id)
							.ToList();

						foreach(var dependentEquipment in dependentEquipments)
						{
							orderEquipmentsForOrganization.Add(dependentEquipment);
							processingEquipments.Remove(dependentEquipment);
						}
						
						processingGoods.RemoveAt(i);
						continue;
					}

					i++;
				}

				if(!goodsForOrganization.Any())
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
					_organizationForOrderFromSet.GetOrganizationForOrderFromSet(requestTime, set)
					?? _organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice);
				
				setsOrganizations.Add(
					set.OrderContentSet,
					new PartOrderWithGoods(org, goodsForOrganization, orderEquipmentsForOrganization));
			}

			if(!processingGoods.Any())
			{
				if(!setsOrganizations.Any())
				{
					var org = _organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice);
					result.Add(new PartOrderWithGoods(org, null));
					return result;
				}

				result.AddRange(setsOrganizations.Values);
				return result;
			}

			var authorsResult = _organizationByOrderAuthorHandler.GetOrganizationsWithOrderItems(
				uow, requestTime, setsOrganizations, organizationChoice, processingGoods);

			if(authorsResult.Any())
			{
				return authorsResult;
			}
			
			return base.GetOrganizationsWithOrderItems(uow, requestTime, organizationChoice);
			
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
		
		public bool OrderHasGoodsFromSeveralOrganizations(
			IUnitOfWork uow, TimeSpan requestTime, IList<int> nomenclatureIds, bool isSelfDelivery, PaymentType paymentType)
		{
			var setsOrganizations = new Dictionary<short, Organization>();
			
			var organizationsBasedOrderContent = uow.GetAll<OrganizationBasedOrderContentSettings>().ToArray();

			if(!organizationsBasedOrderContent.Any())
			{
				return false;
			}
			
			foreach(var set in organizationsBasedOrderContent)
			{
				if(!set.Nomenclatures.Any() && !set.ProductGroups.Any())
				{
					continue;
				}
				
				var i = 0;
				var nomenclatureIdsForOrganization = new List<int>();

				while(i < nomenclatureIds.Count)
				{
					var nomenclature = uow.GetAll<Nomenclature>().FirstOrDefault(x => x.Id == nomenclatureIds[i]);
					
					if(NomenclatureBelongsSet(nomenclature, set))
					{
						nomenclatureIdsForOrganization.Add(nomenclatureIds[i]);
						nomenclatureIds.RemoveAt(i);
						continue;
					}

					i++;
				}

				if(!nomenclatureIdsForOrganization.Any())
				{
					continue;
				}

				var org =
					_organizationForOrderFromSet.GetOrganizationForOrderFromSet(DateTime.Now.TimeOfDay, set)
					?? _organizationByPaymentTypeForOrderHandler.GetOrganization(
						uow, requestTime, isSelfDelivery, paymentType, null);
				
				setsOrganizations.Add(set.OrderContentSet, org);
			}
			
			if(!nomenclatureIds.Any())
			{
				if(!setsOrganizations.Any())
				{
					return false;
				}
			}

			return setsOrganizations.Count > 1;
		}
		
		private bool OrderItemBelongsSet(IProduct product, OrganizationBasedOrderContentSettings organizationBasedOrderContentSettings)
		{
			if(organizationBasedOrderContentSettings.Nomenclatures.Contains(product.Nomenclature))
			{
				return true;
			}

			if(ContainsProductGroup(product.Nomenclature.ProductGroup, organizationBasedOrderContentSettings.ProductGroups))
			{
				return true;
			}
			
			return false;
		}
		
		private bool NomenclatureBelongsSet(
			Nomenclature nomenclature, OrganizationBasedOrderContentSettings organizationBasedOrderContentSettings)
		{
			if(organizationBasedOrderContentSettings.Nomenclatures.Contains(nomenclature))
			{
				return true;
			}

			if(ContainsProductGroup(nomenclature.ProductGroup, organizationBasedOrderContentSettings.ProductGroups))
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
