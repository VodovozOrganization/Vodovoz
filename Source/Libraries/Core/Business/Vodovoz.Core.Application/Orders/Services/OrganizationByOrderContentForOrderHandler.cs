using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Settings;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
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
		private readonly IFlyerRepository _flyerRepository;
		private readonly IOrganizationSettings _organizationSettings;

		public OrganizationByOrderContentForOrderHandler(
			OrganizationByOrderAuthorHandler organizationByOrderAuthorHandler,
			OrganizationByPaymentTypeForOrderHandler organizationByPaymentTypeForOrderHandler,
			IOrganizationForOrderFromSet organizationForOrderFromSet,
			INomenclatureSettings nomenclatureSettings,
			IFlyerRepository flyerRepository,
			IOrganizationSettings organizationSettings)
		{
			_organizationByOrderAuthorHandler =
				organizationByOrderAuthorHandler ?? throw new ArgumentNullException(nameof(organizationByOrderAuthorHandler));
			_organizationByPaymentTypeForOrderHandler =
				organizationByPaymentTypeForOrderHandler
				?? throw new ArgumentNullException(nameof(organizationByPaymentTypeForOrderHandler));
			_organizationForOrderFromSet =
				organizationForOrderFromSet ?? throw new ArgumentNullException(nameof(organizationForOrderFromSet));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
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

			var orderParts = SplitOrderByOrganizationsFromOrderContent(uow, requestTime, organizationChoice);

			return ReplaceOrganizationsInPaidOrder(uow, requestTime, organizationChoice, orderParts);
		}

		private IEnumerable<PartOrderWithGoods> SplitOrderByOrganizationsFromOrderContent(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
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
							x.OrderItem != null && x.OrderItem.Id == processingGoodsWithoutPaidDelivery[i].Id
							|| x.OrderRentDepositItem != null && x.OrderRentDepositItem.Id == processingGoodsWithoutPaidDelivery[i].Id
							|| x.OrderRentServiceItem != null && x.OrderRentServiceItem.Id == processingGoodsWithoutPaidDelivery[i].Id)
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

			if(TryProcessPaidDelivery(
				uow, requestTime, organizationChoice, paidDelivery, goodsAndEquipmentsBySets, out var splitOrderByPaidDelivery))
			{
				return splitOrderByPaidDelivery;
			}

			ProcessDocumentsNomenclatureFromEquipments(processingEquipments, goodsAndEquipmentsBySets);

			foreach(var keyPairValue in goodsAndEquipmentsBySets)
			{
				var organization =
					_organizationForOrderFromSet.GetOrganizationForOrderFromSet(requestTime, keyPairValue.Key, true)
					?? _organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice);
					
				setsOrganizations.Add(
					keyPairValue.Key.OrderContentSet,
					new PartOrderWithGoods(organization, keyPairValue.Value.Goods, keyPairValue.Value.Equipments));
			}

			//если товары только Кулер Сервиса и в оборудовании листовки, убираем их и выставляем один заказ без разбиений
			if(setsOrganizations.Count == 1
				&& setsOrganizations.ContainsKey(1)
				&& !processingGoodsWithoutPaidDelivery.Any()
				&& processingEquipments.Any())
			{
				if(organizationChoice.DeliveryDate.HasValue)
				{
					var activeFlyers = _flyerRepository.GetAllActiveFlyersByDate(uow, organizationChoice.DeliveryDate.Value);
					var allFlyersNomenclaturesIds = _flyerRepository.GetAllFlyersNomenclaturesIds(uow);

					var i = 0;
					var orderEquipmentsList = organizationChoice.OrderEquipments as IList<OrderEquipment>;
					
					while(i < processingEquipments.Count)
					{
						if(processingEquipments[i].Nomenclature == null)
						{
							i++;
							continue;
						}

						var flyerFromEquipment = activeFlyers
							.FirstOrDefault(x => x.FlyerNomenclature != null
								&& x.FlyerNomenclature.Id == processingEquipments[i].Nomenclature.Id);

						if(flyerFromEquipment != null || allFlyersNomenclaturesIds.Contains(processingEquipments[i].Nomenclature.Id))
						{
							RemoveFlyerFromEquipment(processingEquipments, i, orderEquipmentsList);
							continue;
						}

						i++;
					}

					if(!processingEquipments.Any())
					{
						return setsOrganizations.Values;
					}
				}
			}

			if(processingGoodsWithoutPaidDelivery.Any() || processingEquipments.Any())
			{
				return _organizationByOrderAuthorHandler.SplitOrderByOrganizations(
					uow, requestTime, setsOrganizations, organizationChoice, processingGoodsWithoutPaidDelivery, processingEquipments);
			}

			return setsOrganizations.Values;
		}

		/// <summary>
		/// В уже оплаченном заказе организация по составу заказа (Кулер Сервис) не подставляется
		/// Деньги ушли на организацию, подобранную по форме и источнику оплаты, поэтому все части заказа
		/// получают именно её. Разбиение заказа на части при этом сохраняется, чтобы сервисную часть заказа
		/// выполнил мастер, а воду привез курьер
		/// </summary>
		private IEnumerable<PartOrderWithGoods> ReplaceOrganizationsInPaidOrder(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice,
			IEnumerable<PartOrderWithGoods> orderParts)
		{
			if(!organizationChoice.IsOrderPaid)
			{
				return orderParts;
			}

			var orderPartsList = orderParts.ToList();

			if(orderPartsList.All(x => x.Organization?.Id != _organizationSettings.KulerServiceOrganizationId))
			{
				return orderPartsList;
			}

			var organizationByPaymentType =
				_organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice);

			return orderPartsList
				.Select(orderPart => ReplaceOrganizationInOrderPart(orderPart, organizationByPaymentType))
				.ToList();
		}

		private PartOrderWithGoods ReplaceOrganizationInOrderPart(PartOrderWithGoods orderPart, Organization organization)
		{
			if(orderPart.Organization?.Id == organization.Id)
			{
				return orderPart;
			}

			return new PartOrderWithGoods(organization, orderPart.Goods, orderPart.OrderEquipments)
			{
				OrderDepositItems = orderPart.OrderDepositItems
			};
		}

		public bool OrderHasGoodsFromSeveralOrganizations(
			IUnitOfWork uow,
			IList<int> nomenclatureIds)
		{
			var kulerServiceSet = GetKulerServiceSet(uow);

			if(kulerServiceSet is null)
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

		/// <summary>
		/// Проверка наличия в заказе товаров, продающихся от Кулер Сервис
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="nomenclatures">Номенклатуры, находящиеся в заказе</param>
		/// <returns><c>true</c> если заказ содержит товары сервисной организации, иначе <c>false</c></returns>
		public bool IsOrderContainsKulerServiceGoods(IUnitOfWork uow, IEnumerable<OrderItem> orderItems)
		{
			if(orderItems is null)
			{
				throw new ArgumentNullException(nameof(orderItems));
			}

			var kulerServiceSet = GetKulerServiceSet(uow);

			if(kulerServiceSet is null)
			{
				return false;
			}

			var nomenclatures = orderItems.Select(x => x.Nomenclature).ToList();

			return nomenclatures.Any(nomenclature => NomenclatureBelongsSet(nomenclature, kulerServiceSet));
		}

		/// <summary>
		/// Множество настроек с товарами, продающимися от сервисной организации(Кулер Сервис)
		/// <c>null</c>, если множество не настроено
		/// </summary>
		private OrganizationBasedOrderContentSettings GetKulerServiceSet(IUnitOfWork uow)
		{
			var kulerServiceSet =
				uow.GetAll<OrganizationBasedOrderContentSettings>()
					.FirstOrDefault(x => x.OrderContentSet == OrganizationBasedOrderContentSettings.KulerServiceOrderContentSet);

			if(kulerServiceSet is null
				|| (!kulerServiceSet.Nomenclatures.Any() && !kulerServiceSet.ProductGroups.Any()))
			{
				return null;
			}

			return kulerServiceSet;
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
		
		private void RemoveFlyerFromEquipment(List<OrderEquipment> processingEquipments, int i, IList<OrderEquipment> orderEquipmentsList)
		{
			var removingEquipment = processingEquipments[i];
							
			orderEquipmentsList?.Remove(removingEquipment);
			processingEquipments.Remove(removingEquipment);
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
		
		private bool TryProcessPaidDelivery(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice,
			IProduct paidDelivery,
			Dictionary<OrganizationBasedOrderContentSettings, (IList<IProduct> Goods, IList<OrderEquipment> Equipments)> goodsAndEquipmentsBySets,
			out IEnumerable<PartOrderWithGoods> splitOrderByOrganizations)
		{
			splitOrderByOrganizations = Enumerable.Empty<PartOrderWithGoods>();
			
			if(paidDelivery is null)
			{
				if(goodsAndEquipmentsBySets.Keys.All(x => x.OrderContentSet != 1))
				{
					splitOrderByOrganizations = new[]
					{
						new PartOrderWithGoods(_organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice))
					};
					return true;
				}
			}
			else
			{
				var setGoodsAndEquipments = goodsAndEquipmentsBySets.First().Value;
				setGoodsAndEquipments.Goods.Add(paidDelivery);
				
				if(goodsAndEquipmentsBySets.Keys.All(x => x.OrderContentSet != 1))
				{
					splitOrderByOrganizations = new[]
					{
						new PartOrderWithGoods(_organizationByPaymentTypeForOrderHandler.GetOrganization(uow, requestTime, organizationChoice))
					};
					return true;
				}
			}

			return false;
		}
		
		private void ProcessDocumentsNomenclatureFromEquipments(
			IList<OrderEquipment> processingEquipments,
			Dictionary<OrganizationBasedOrderContentSettings, (IList<IProduct> Goods, IList<OrderEquipment> Equipments)> goodsAndEquipmentsBySets)
		{
			if(!processingEquipments.Any())
			{
				return;
			}
			
			var setGoodsAndEquipments = goodsAndEquipmentsBySets.First().Value;
			var i = 0;

			while(i < processingEquipments.Count)
			{
				if(processingEquipments[i].Nomenclature != null
					&& processingEquipments[i].Nomenclature.Id == _nomenclatureSettings.DocumentsNomenclatureId)
				{
					setGoodsAndEquipments.Equipments.Add(processingEquipments[i]);
					processingEquipments.RemoveAt(i);
					continue;
				}

				i++;
			}
		}
	}
}
