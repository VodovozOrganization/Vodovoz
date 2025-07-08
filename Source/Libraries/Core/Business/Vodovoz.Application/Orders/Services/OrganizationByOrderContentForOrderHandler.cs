﻿using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Nomenclature;
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
		private readonly INomenclatureSettings _nomenclatureSettings;

		public OrganizationByOrderContentForOrderHandler(
			OrganizationByOrderAuthorHandler organizationByOrderAuthorHandler,
			OrganizationByPaymentTypeForOrderHandler organizationByPaymentTypeForOrderHandler,
			IOrganizationForOrderFromSet organizationForOrderFromSet,
			INomenclatureSettings nomenclatureSettings)
		{
			_organizationByOrderAuthorHandler =
				organizationByOrderAuthorHandler ?? throw new ArgumentNullException(nameof(organizationByOrderAuthorHandler));
			_organizationByPaymentTypeForOrderHandler =
				organizationByPaymentTypeForOrderHandler
				?? throw new ArgumentNullException(nameof(organizationByPaymentTypeForOrderHandler));
			_organizationForOrderFromSet =
				organizationForOrderFromSet ?? throw new ArgumentNullException(nameof(organizationForOrderFromSet));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}

		public override IEnumerable<PartOrderWithGoods> SplitOrderByOrganizations(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			if(organizationChoice.IsSplitedOrder && organizationChoice.CurrentOrderOrganization != null)
			{
				return new[]
				{
					new PartOrderWithGoods(organizationChoice.CurrentOrderOrganization)
				};
			}
			
			var setsOrganizations = new Dictionary<short, PartOrderWithGoods>();
			IProduct paidDelivery = null;
			IList<IProduct> processingGoodsWithoutPaidDelivery = new List<IProduct>();

			foreach(var product in organizationChoice.Goods)
			{
				if(product.Nomenclature.Id == _nomenclatureSettings.PaidDeliveryNomenclatureId)
				{
					paidDelivery = product;
					continue;
				}
				
				processingGoodsWithoutPaidDelivery.Add(product);
			}
			
			var processingEquipments = organizationChoice.OrderEquipments.ToList();
			var organizationBasedOrderContentSettings = uow.GetAll<OrganizationBasedOrderContentSettings>().ToArray();
			var goodsAndEquipmentsBySets =
				new Dictionary<OrganizationBasedOrderContentSettings, (IList<IProduct> Goods, IList<OrderEquipment> Equipments)>();
			
			foreach(var setSettings in organizationBasedOrderContentSettings)
			{
				var i = 0;
				
				while(i < processingGoodsWithoutPaidDelivery.Count)
				{
					if(!ProductBelongsSet(processingGoodsWithoutPaidDelivery[i], setSettings))
					{
						i++;
						continue;
					}

					if(!goodsAndEquipmentsBySets.TryGetValue(setSettings, out var processingGoodsAndEquipments))
					{
						processingGoodsAndEquipments =
							new ValueTuple<IList<IProduct>, IList<OrderEquipment>>(new List<IProduct>(), new List<OrderEquipment>());
						goodsAndEquipmentsBySets.Add(setSettings, processingGoodsAndEquipments);
					}

					processingGoodsAndEquipments.Goods.Add(processingGoodsWithoutPaidDelivery[i]);

					var dependentEquipments = processingEquipments.Where(x =>
							(x.OrderItem != null && x.OrderItem.Id == processingGoodsWithoutPaidDelivery[i].Id)
							|| (x.OrderRentDepositItem != null && x.OrderRentDepositItem.Id == processingGoodsWithoutPaidDelivery[i].Id)
							|| (x.OrderRentServiceItem != null && x.OrderRentServiceItem.Id == processingGoodsWithoutPaidDelivery[i].Id))
						.ToList();

					foreach(var dependentEquipment in dependentEquipments)
					{
						processingGoodsAndEquipments.Equipments.Add(dependentEquipment);
						processingEquipments.Remove(dependentEquipment);
					}

					processingGoodsWithoutPaidDelivery.Remove(processingGoodsWithoutPaidDelivery[i]);
				}
				
				ProcessEquipmentsNotDependsOrderItems(processingEquipments, setSettings, goodsAndEquipmentsBySets);
			}

			if(goodsAndEquipmentsBySets.Count == 0)
			{
				return new[]
				{
					new PartOrderWithGoods(_organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice))
				};
			}
			
			if(paidDelivery is null)
			{
				if(goodsAndEquipmentsBySets.Keys.All(x => x.OrderContentSet != 1))
				{
					return new[]
					{
						new PartOrderWithGoods(_organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice))
					};
				}
			}
			else
			{
				var setGoodsAndEquipments = goodsAndEquipmentsBySets.First().Value;
				setGoodsAndEquipments.Goods.Add(paidDelivery);
				
				if(goodsAndEquipmentsBySets.Keys.All(x => x.OrderContentSet != 1))
				{
					return new[]
					{
						new PartOrderWithGoods(_organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice))
					};
				}
			}
			
			foreach(var keyPairValue in goodsAndEquipmentsBySets)
			{
				var organization =
					_organizationForOrderFromSet.GetOrganizationForOrderFromSet(requestTime, keyPairValue.Key, true)
					?? _organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice);
					
				setsOrganizations.Add(
					keyPairValue.Key.OrderContentSet,
					new PartOrderWithGoods(organization, keyPairValue.Value.Goods, keyPairValue.Value.Equipments));
			}

			if(processingGoodsWithoutPaidDelivery.Any() || processingEquipments.Any())
			{
				return _organizationByOrderAuthorHandler.SplitOrderByOrganizations(
					uow, requestTime, setsOrganizations, organizationChoice, processingGoodsWithoutPaidDelivery, processingEquipments);
			}

			return setsOrganizations.Values;
		}

		private void ProcessEquipmentsNotDependsOrderItems(
			IList<OrderEquipment> processingEquipments,
			OrganizationBasedOrderContentSettings setSettings,
			IDictionary<OrganizationBasedOrderContentSettings, (IList<IProduct> Goods, IList<OrderEquipment> Equipments)> goodsAndEquipmentsBySets)
		{
			var j = 0;
				
			while(j < processingEquipments.Count)
			{
				if(processingEquipments[j].OrderItem != null
					|| processingEquipments[j].OrderRentServiceItem != null
					|| processingEquipments[j].OrderRentDepositItem != null)
				{
					j++;
					continue;
				}
				
				if(!NomenclatureBelongsSet(processingEquipments[j].Nomenclature, setSettings))
				{
					j++;
					continue;
				}

				if(!goodsAndEquipmentsBySets.TryGetValue(setSettings, out var goodsAndEquipments))
				{
					goodsAndEquipments =
						new ValueTuple<IList<IProduct>, IList<OrderEquipment>>(new List<IProduct>(), new List<OrderEquipment>());
					goodsAndEquipmentsBySets.Add(setSettings, goodsAndEquipments);
				}
				
				goodsAndEquipments.Equipments.Add(processingEquipments[j]);
				processingEquipments.RemoveAt(j);
			}
		}

		public bool OrderHasGoodsFromSeveralOrganizations(
			IUnitOfWork uow,
			IList<int> nomenclatureIds)
		{
			var kulerServiceSet =
				uow.GetAll<OrganizationBasedOrderContentSettings>()
					.FirstOrDefault(x => x.OrderContentSet == 1);

			if(kulerServiceSet is null)
			{
				return false;
			}
			
			if(!kulerServiceSet.Nomenclatures.Any() && !kulerServiceSet.ProductGroups.Any())
			{
				return false;
			}
			
			var i = 0;
			var kulerServiceProductIds = new List<int>();

			while(i < nomenclatureIds.Count)
			{
				var nomenclature = uow.GetAll<Nomenclature>().FirstOrDefault(x => x.Id == nomenclatureIds[i]);
				
				if(NomenclatureBelongsSet(nomenclature, kulerServiceSet))
				{
					kulerServiceProductIds.Add(nomenclatureIds[i]);
					nomenclatureIds.RemoveAt(i);
					continue;
				}

				i++;
			}
			
			return nomenclatureIds.Any() && kulerServiceProductIds.Any();
		}
		
		private bool ProductBelongsSet(IProduct product, OrganizationBasedOrderContentSettings organizationBasedOrderContentSettings)
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
			if(nomenclature is null)
			{
				return false;
			}
			
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
