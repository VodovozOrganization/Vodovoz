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

		public override IEnumerable<PartOrderWithGoods> SplitOrderByOrganizations(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			var setsOrganizations = new Dictionary<short, PartOrderWithGoods>();
			var processingGoods = organizationChoice.Goods.ToList();
			var processingEquipments = organizationChoice.OrderEquipments.ToList();

			var organizationBasedOrderContentSettings = uow.GetAll<OrganizationBasedOrderContentSettings>().ToArray();
			var goodsAndEquipmentsBySets =
				new Dictionary<OrganizationBasedOrderContentSettings, (IList<IProduct> Goods, IList<OrderEquipment> Equipments)>();
			
			foreach(var setSettings in organizationBasedOrderContentSettings)
			{
				var i = 0;
				
				while(i < processingGoods.Count)
				{
					if(!ProductBelongsSet(processingGoods[i], setSettings))
					{
						i++;
						continue;
					}

					if(!goodsAndEquipmentsBySets.TryGetValue(setSettings, out var goodsAndEquipments))
					{
						goodsAndEquipments =
							new ValueTuple<IList<IProduct>, IList<OrderEquipment>>(new List<IProduct>(), new List<OrderEquipment>());
						goodsAndEquipmentsBySets.Add(setSettings, goodsAndEquipments);
					}

					goodsAndEquipments.Goods.Add(processingGoods[i]);

					var dependentEquipments = processingEquipments.Where(x =>
							x.OrderItem.Id == processingGoods[i].Id
							|| x.OrderRentDepositItem.Id == processingGoods[i].Id
							|| x.OrderRentServiceItem.Id == processingGoods[i].Id)
						.ToList();

					foreach(var dependentEquipment in dependentEquipments)
					{
						goodsAndEquipments.Equipments.Add(dependentEquipment);
						processingEquipments.Remove(dependentEquipment);
					}

					processingGoods.Remove(processingGoods[i]);
				}
			}
			
			ProcessEquipments(processingEquipments, organizationBasedOrderContentSettings, goodsAndEquipmentsBySets);

			if(goodsAndEquipmentsBySets.Count == 0
				|| goodsAndEquipmentsBySets.Keys.All(x => x.OrderContentSet != 1))
			{
				return new[]
				{
					new PartOrderWithGoods(_organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice))
				};
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

			if(processingGoods.Any())
			{
				return _organizationByOrderAuthorHandler.SplitOrderByOrganizations(
					uow, requestTime, setsOrganizations, organizationChoice, processingGoods);
			}

			return setsOrganizations.Values;
		}

		private void ProcessEquipments(
			IList<OrderEquipment> processingEquipments,
			IEnumerable<OrganizationBasedOrderContentSettings> organizationBasedOrderContentSettings,
			IDictionary<
				OrganizationBasedOrderContentSettings,
				(IList<IProduct> Goods, IList<OrderEquipment> Equipments)> goodsAndEquipmentsBySets)
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

				var firstSetSettings = organizationBasedOrderContentSettings.FirstOrDefault(x => x.OrderContentSet == 1);

				if(firstSetSettings is null)
				{
					throw new InvalidOperationException("Нет настроек выбора организации для первого множества!");
				}
					
				if(!goodsAndEquipmentsBySets.TryGetValue(firstSetSettings, out var goodsAndEquipments))
				{
					goodsAndEquipments =
						new ValueTuple<IList<IProduct>, IList<OrderEquipment>>(new List<IProduct>(), new List<OrderEquipment>());
					goodsAndEquipmentsBySets.Add(firstSetSettings, goodsAndEquipments);
				}

				goodsAndEquipments.Equipments.Add(processingEquipments[j]);
				processingEquipments.RemoveAt(j);
			}
		}

		public bool OrderHasGoodsFromSeveralOrganizations(
			IUnitOfWork uow,
			TimeSpan requestTime,
			IList<int> nomenclatureIds,
			bool isSelfDelivery,
			PaymentType paymentType,
			PaymentFrom paymentFrom)
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
					_organizationForOrderFromSet.GetOrganizationForOrderFromSet(DateTime.Now.TimeOfDay, set, true)
					?? _organizationByPaymentTypeForOrderHandler.GetOrganization(
						uow, requestTime, isSelfDelivery, paymentType, paymentFrom, null);
				
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
