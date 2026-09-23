using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Domain.Service;
using VodovozBusiness.Extensions;
using VodovozBusiness.Factories;
using VodovozBusiness.Services.Sale;
using VodovozBusiness.Specifications.Sale;
using VodovozBusiness.Validation;

namespace Vodovoz.Core.Application.Sale
{
	public class SaleHandler : ISaleHandler
	{
		public SaleHandler(
			SaleItemHandler saleItemHandler,
			IGoodsPriceCalculator goodsPriceCalculator,
			IDeliveryPriceService deliveryPriceService,
			IFixedPriceGetter fixedPriceGetter,
			IDeliveryRepository deliveryRepository,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureRepository nomenclatureRepository,
			IAddNomenclatureToSaleValidator addNomenclatureToSaleValidator,
			IAddPromoSetValidator addPromoSetValidator,
			ISaleItemFactory saleItemFactory
			)
		{
			GoodsPriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			DeliveryPriceService = deliveryPriceService ?? throw new ArgumentNullException(nameof(deliveryPriceService));
			FixedPriceGetter = fixedPriceGetter ?? throw new ArgumentNullException(nameof(fixedPriceGetter));
			DeliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			NomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			NomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			AddNomenclatureToSaleValidator = addNomenclatureToSaleValidator ?? throw new ArgumentNullException(nameof(addNomenclatureToSaleValidator));
			AddPromoSetValidator = addPromoSetValidator ?? throw new ArgumentNullException(nameof(addPromoSetValidator));
			SaleItemFactory = saleItemFactory ?? throw new ArgumentNullException(nameof(saleItemFactory));
			SaleItemHandler = saleItemHandler ?? throw new ArgumentNullException(nameof(saleItemHandler));
		}
		
		protected SaleItemHandler SaleItemHandler { get; }
		protected ISaleSource Source { get; set; }
		protected IGoodsPriceCalculator GoodsPriceCalculator { get; }
		protected IDeliveryPriceService DeliveryPriceService { get; }
		protected IFixedPriceGetter FixedPriceGetter { get; }
		protected IDeliveryRepository DeliveryRepository { get; }
		protected INomenclatureSettings NomenclatureSettings { get; }
		protected INomenclatureRepository NomenclatureRepository { get; }
		protected IAddNomenclatureToSaleValidator AddNomenclatureToSaleValidator { get; }
		protected IAddPromoSetValidator AddPromoSetValidator { get; }
		protected ISaleItemFactory SaleItemFactory { get; }

		public void SetSource(ISaleSource source)
		{
			Source = source;
		}
		
		public virtual void Recalculate()
		{
			ThrowIfSaleItemsIsNull();
			RecalculateItemsPrice();
		}
		
		public virtual void RecalculateDiscounts(IDataContext context)
		{
			SaleItemHandler.RecalculateDiscounts(context);
		}
		
		public virtual bool SetCount(INomenclatureCount saleItem, decimal count)
		{
			if(!SaleItemHandler.SetCount(saleItem, count))
			{
				return false;
			}

			Recalculate();
			return true;
		}
		
		public virtual void SetPrice(ISaleItem saleItem, (SaleItemPriceType PriceType, decimal Price) priceData)
		{
			var priceByTotalCount = GoodsPriceCalculator
				.CalculateItemPrice(
					Source.SaleItems,
					Source.DeliveryPoint,
					Source.Counterparty,
					saleItem,
					Source.HasPermissionsForAlternativePrice);

			var discountItem = saleItem.ToApplyDiscountReasonItem();
			UpdatePriceType(saleItem, priceData, priceByTotalCount, SaleItemSpecification.Create(priceByTotalCount));

			SaleItemHandler.SetPrice(
				CommonRecalculateDiscount.CreateDataContext(discountItem),
				priceData.Price);
		}

		public virtual void SetPriceForNewSaleItem(
			ISaleItem newItem,
			(SaleItemPriceType PriceType, decimal Price) priceData)
		{
			var priceByTotalCount = GoodsPriceCalculator
				.CalculateItemPrice(
					Source.SaleItems,
					Source.DeliveryPoint,
					Source.Counterparty,
					newItem,
					Source.HasPermissionsForAlternativePrice);

			var discountItem = newItem.ToApplyDiscountReasonItem();
			UpdatePriceType(newItem, priceData, priceByTotalCount, SaleItemSpecification.Create(priceByTotalCount));
			
			SaleItemHandler.SetPriceForNewSaleItem(
				CommonRecalculateDiscount.CreateDataContext(discountItem),
				priceData.Price);
		}

		public virtual void RecalculatePrice(ISaleItem saleItem)
		{
			if(saleItem.IsUserPrice || saleItem.PromoSet != null)
			{
				return;
			}

			SetPrice(
				saleItem,
				GoodsPriceCalculator.CalculateItemPrice(
					Source.SaleItems,
					Source.DeliveryPoint,
					Source.Counterparty,
					saleItem,
					Source.HasPermissionsForAlternativePrice
				)
			);
		}

		protected virtual void RecalculateItemsPrice()
		{
			//TODO-5967 проверить работу метода, т.к. в заказе могут добавляться или исключаться позиции по вызовам событий
			foreach(var saleItem in Source.SaleItems.ToList())
			{
				if(saleItem.Nomenclature.Category == NomenclatureCategory.water)
				{
					RecalculatePrice(saleItem);
				}
			}
		}

		protected void ThrowIfSaleItemsIsNull()
		{
			if(Source?.SaleItems is null)
			{
				throw new InvalidOperationException("SaleItems cannot be null.");
			}
		}

		protected virtual void UpdatePriceType(
			ISaleItem saleItem,
			(SaleItemPriceType PriceType, decimal Price) receivedPriceData,
			(SaleItemPriceType PriceType, decimal Price) calculatedPriceData,
			ISpecificationTwoArgs specification)
		{
			//TODO-5967 проверить алгоритм установки булевых параметров. Также пользователь может вернуть цену у позиции, где есть фикса, поэтому нужно проработать и этот вариант
			
			/*if(specification.IsSatisfiedBy(saleItem, receivedPriceData))
			{
				saleItem.IsUserPrice = true;
				saleItem.IsFixedPrice = false;
				saleItem.IsAlternativePrice = false;
				return;
			}
			
			switch(receivedPriceData.PriceType)
			{
				case SaleItemPriceType.Fixed:
					saleItem.IsFixedPrice = true;
					saleItem.IsUserPrice = false;
					saleItem.IsAlternativePrice = false;
					break;
				case SaleItemPriceType.User:
					if(calculatedPriceData.PriceType == SaleItemPriceType.Fixed)
					{
						
					}
					saleItem.IsUserPrice = false;
					saleItem.IsFixedPrice = false;
					saleItem.IsAlternativePrice = false;
					break;
				case SaleItemPriceType.Alternative:
					saleItem.IsAlternativePrice = true;
					saleItem.IsUserPrice = false;
					saleItem.IsFixedPrice = false;
					break;
				default:
					saleItem.IsAlternativePrice = false;
					saleItem.IsUserPrice = false;
					saleItem.IsFixedPrice = false;
					break;
			}*/
			
			switch(receivedPriceData.PriceType)
			{
				case SaleItemPriceType.Fixed:
					saleItem.IsFixedPrice = true;
					saleItem.IsUserPrice = false;
					saleItem.IsAlternativePrice = false;
					break;
				case SaleItemPriceType.User:
					if(specification.IsSatisfiedBy(saleItem, receivedPriceData))
					{
						saleItem.IsUserPrice = true;
						saleItem.IsFixedPrice = false;
						saleItem.IsAlternativePrice = false;
					}
					else
					{
						saleItem.IsUserPrice = false;
						saleItem.IsFixedPrice = false;
						saleItem.IsAlternativePrice = false;
					}
					break;
				case SaleItemPriceType.Alternative:
					saleItem.IsAlternativePrice = true;
					saleItem.IsUserPrice = false;
					saleItem.IsFixedPrice = false;
					break;
				default:
					saleItem.IsAlternativePrice = false;
					saleItem.IsUserPrice = false;
					saleItem.IsFixedPrice = false;
					break;
			}
		}

		#region Работа с товарами/услугами

		public virtual Result TryAddNomenclature(
			IUnitOfWork uow,
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			IEnumerable<DiscountReasonBase> discountReasons = null
			)
		{
			var canAddNomenclatureResult = AddNomenclatureToSaleValidator
				.CanAddNomenclature(nomenclature, Source);

			if(canAddNomenclatureResult.IsFailure)
			{
				return canAddNomenclatureResult;
			}

			AddNomenclature(
				uow,
				NewOrderSaleItem.Create(
					nomenclature,
					count,
					priceData: default,
					discount,
					false,
					discountReasons: discountReasons
				)
			);

			return Result.Success();
		}

		public virtual Result TryAddPromoSet(IUnitOfWork uow, PromotionalSet proSet)
		{
			var addPromoValidationResult = AddPromoSetValidator.CanAddPromotionalSet(uow, Source, proSet);
			
			if(addPromoValidationResult.IsFailure)
			{
				return addPromoValidationResult;
			}

			return TryAddNomenclatureFromPromoSet(uow, proSet);
		}
		
		public virtual Result TryAddNomenclatureFromPromoSet(IUnitOfWork uow, PromotionalSet proSet)
		{
			if(proSet is { IsArchive: false } && proSet.PromotionalSetItems.Any())
			{
				foreach(var proSetItem in proSet.PromotionalSetItems)
				{
					
					//TODO нужно перенести проверки выше, перед добавлением всего промонабора иначе может быть ситуация с добавлением части промика
					
					/*var nomenclature = proSetItem.Nomenclature;
					if(Source.OrderItems.Any(x =>
							!Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
						&& nomenclature.Category == NomenclatureCategory.master)
					{
						MessageDialogHelper.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
						return;
					}

					if(Entity.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
						&& !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category))
					{
						MessageDialogHelper.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
						return;
					}*/

					AddNomenclature(
						uow,
						NewOrderSaleItem.Create(
							proSetItem.Nomenclature,
							proSetItem.Count,
							priceData: default,
							proSetItem.IsDiscountInMoney ? proSetItem.DiscountMoney : proSetItem.Discount,
							proSetItem.IsDiscountInMoney,
							null,
							proSetItem.PromoSet
						)
					);
				}
				
				return UpdateDeliveryCost(uow);
			}
			
			return Result.Success();
		}
		
		public virtual void AddNomenclature(IUnitOfWork uow, NewOrderSaleItem newOrderSaleItem)
		{
			switch(newOrderSaleItem.Nomenclature.Category) {
				case NomenclatureCategory.water:
					AddWaterForSale(uow, newOrderSaleItem);
					break;
				case NomenclatureCategory.master:
					AddMasterNomenclature(uow, newOrderSaleItem);
					break;
				default:
					var canApplyAlternativePrice = Source.HasPermissionsForAlternativePrice
						&& newOrderSaleItem.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= newOrderSaleItem.Count);

					newOrderSaleItem.PriceData = newOrderSaleItem.Nomenclature.GetPrice(1, canApplyAlternativePrice);
					
					var saleItem = SaleItemFactory.Create(
						Source,
						newOrderSaleItem);

					var acceptableCategories = NomenclatureEntity.GetCategoriesForSale();
					if(saleItem?.Nomenclature == null
						|| !acceptableCategories.Contains(saleItem.Nomenclature.Category))
					{
						return;
					}
					
					AddSaleItem(uow, saleItem, newOrderSaleItem.PriceData);

					break;
			}
		}
		
		public virtual void AddWaterForSale(IUnitOfWork uow, NewOrderSaleItem newOrderSaleItem)
		{
			if(newOrderSaleItem.Nomenclature.Category != NomenclatureCategory.water && !newOrderSaleItem.Nomenclature.IsDisposableTare)
			{
				return;
			}

			//Если номенклатура промонабора добавляется по фиксе (без скидки), то у нового OrderItem убирается поле discountReason
			if(newOrderSaleItem.PromoSet != null && newOrderSaleItem.Discount == 0)
			{
				var fixPricedNomenclaturesId = FixedPriceGetter
					.GetNomenclaturesWithFixedPrices(Source)
					.Select(n => n.Id);
				
				if(fixPricedNomenclaturesId.Contains(newOrderSaleItem.Nomenclature.Id))
				{
					newOrderSaleItem.DiscountReasons = null;
				}
			}

			if(newOrderSaleItem.Discount > 0
				&& (newOrderSaleItem.DiscountReasons is null || !newOrderSaleItem.DiscountReasons.Any())
				&& newOrderSaleItem.PromoSet is null)
			{
				throw new ArgumentException("Требуется указать причину скидки (reason), если она (discount) больше 0!");
			}

			newOrderSaleItem.PriceData = GoodsPriceCalculator.CalculateItemPrice(
				Source.SaleItems,
				Source.DeliveryPoint,
				Source.Counterparty,
				newOrderSaleItem,
				Source.HasPermissionsForAlternativePrice
			);
			
			var saleItem = SaleItemFactory.Create(Source, newOrderSaleItem);
			
			AddSaleItem(uow, saleItem, newOrderSaleItem.PriceData);
		}

		/// <summary>
		/// Добавление в заказ номенклатуры типа "Сервисное обслуживание"
		/// </summary>
		/// <param name="nomenclature">Номенклатура типа "Сервисное обслуживание"</param>
		/// <param name="uow">unit of work</param>
		/// <param name="count">Количество</param>
		/// <param name="quantityOfFollowingNomenclatures">Колличество номенклатуры, указанной в параметрах БД,
		/// которые будут добавлены в заказ вместе с мастером</param>
		public virtual void AddMasterNomenclature(
			IUnitOfWork uow,
			NewOrderSaleItem newOrderSaleItem,
			int quantityOfFollowingNomenclatures = 0)
		{
			var nomenclature = newOrderSaleItem.Nomenclature;
			
			if(nomenclature.Category != NomenclatureCategory.master)
			{
				return;
			}

			var canApplyAlternativePrice = Source.HasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= newOrderSaleItem.Count);
			
			newOrderSaleItem.PriceData = newOrderSaleItem.Nomenclature.GetPrice(1, canApplyAlternativePrice);
			var saleItem = SaleItemFactory.Create(Source, newOrderSaleItem);

			AddSaleItem(uow, saleItem, newOrderSaleItem.PriceData);

			if(quantityOfFollowingNomenclatures > 0)
			{
				var followingNomenclature = NomenclatureRepository.GetNomenclatureToAddWithMaster(uow);
				if(!Source.SaleItems.Any(i => i.Nomenclature.Id == followingNomenclature.Id))
				{
					AddAnyGoodsNomenclatureForSale(
						uow,
						followingNomenclature,
						false,
						1);
				}
			}
		}
		
		public virtual void AddAnyGoodsNomenclatureForSale(
			IUnitOfWork uow,
			Nomenclature nomenclature,
			bool isChangeOrder = false,
			int? cnt = null)
		{
			var acceptableCategories = NomenclatureEntity.GetCategoriesForSale();
			if(!acceptableCategories.Contains(nomenclature.Category))
			{
				return;
			}

			var count = (nomenclature.Category == NomenclatureCategory.service
				|| nomenclature.Category == NomenclatureCategory.deposit) && !isChangeOrder ? 1 : 0;

			if(cnt.HasValue)
			{
				count = cnt.Value;
			}

			var canApplyAlternativePrice = Source.HasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			var newSaleItemData = NewOrderSaleItem.Create(nomenclature, count, nomenclature.GetPrice(1, canApplyAlternativePrice));
			var newSaleItem = SaleItemFactory.Create(Source, newSaleItemData);

			AddSaleItem(uow, newSaleItem, newSaleItemData.PriceData);
		}
		
		public virtual void AddSaleItem(
			IUnitOfWork uow,
			ISaleItem saleItem,
			(SaleItemPriceType PriceType, decimal Price) priceData
			)
		{
			if(Source.SaleItems.Contains(saleItem))
			{
				return;
			}

			SetPriceForNewSaleItem(saleItem, priceData);
			Source.SaleItemsList.Add(saleItem);

			Recalculate();
			TrySetMasterCallNomenclaturePrice(uow);
		}
		
		public void TrySetMasterCallNomenclaturePrice(IUnitOfWork unitOfWork)
		{
			var masterCallOrderItem = Source.SaleItems
				.FirstOrDefault(x => x.Nomenclature.Id == NomenclatureSettings.MasterCallNomenclatureId);

			if(masterCallOrderItem is null)
			{
				return;
			}

			if(masterCallOrderItem.IsUserPrice)
			{
				return;
			}

			var deliveryPoint = Source.DeliveryPoint;
			var deliveryDate = Source.DeliveryDate;

			if(deliveryPoint is null || !deliveryDate.HasValue)
			{
				SetPrice(masterCallOrderItem, masterCallOrderItem.Nomenclature.GetPrice(1));
				return;
			}

			var serviceDistrict = DeliveryRepository.GetServiceDistrictByCoordinates(
				unitOfWork,
				deliveryPoint.Latitude.Value,
				deliveryPoint.Longitude.Value);

			if(serviceDistrict is null)
			{
				SetPrice(masterCallOrderItem, masterCallOrderItem.Nomenclature.GetPrice(1));
				return;
			}

			decimal price = 0;

			if(Source.SaleItems.Any(x => x.Nomenclature.MasterServiceType ==  MasterServiceType.Cleaning))
			{
				price = GoodsPriceCalculator.GetMasterServiceTypePrice(serviceDistrict, MasterServiceType.Cleaning, deliveryDate);
			}
			else if(Source.SaleItems.Any(x => x.Nomenclature.MasterServiceType == MasterServiceType.Repair))
			{
				price = GoodsPriceCalculator.GetMasterServiceTypePrice(serviceDistrict, MasterServiceType.Repair, deliveryDate);
			}

			SetPrice(masterCallOrderItem, (SaleItemPriceType.General, price));
		}
		
		public virtual Result TryRemoveSaleItem(IUnitOfWork uow, ISaleItem item)
		{
			RemoveItem(uow, item);
			return Result.Success();
		}
		
		protected virtual void RemoveItem(IUnitOfWork uow, ISaleItem item)
		{
			RemoveSaleItem(uow, item);
			AfterRemoveSaleItem(item);
			TrySetMasterCallNomenclaturePrice(uow);
		}

		protected virtual void AfterRemoveSaleItem(ISaleItem item)
		{
			
		}
		
		protected virtual void RemoveSaleItem(IUnitOfWork uow, ISaleItem saleItem)
		{
			if(!Source.SaleItems.Contains(saleItem))
			{
				return;
			}

			if(saleItem.PromoSet != null)
			{
				var itemsToRemove = Source.SaleItems.Where(oi => oi.PromoSet == saleItem.PromoSet).ToList();
				foreach (var item in itemsToRemove)
				{
					Source.SaleItemsList.Remove(item);
				}
			}
			else
			{
				Source.SaleItemsList.Remove(saleItem);
			}
		}
		
		public Result UpdateDeliveryCost(IUnitOfWork unitOfWork)
		{
			var deliveryPriceResult = DeliveryPriceService.GetDeliveryPrice(unitOfWork, Source);

			if(deliveryPriceResult.IsFailure)
			{
				return Result.Failure(deliveryPriceResult.Errors);
			}
			
			UpdateDeliveryItem(
				unitOfWork,
				NomenclatureRepository.GetPaidDelivery(unitOfWork), deliveryPriceResult.Value);
			
			return Result.Success();
		}
		
		public virtual void UpdateDeliveryItem(IUnitOfWork uow, Nomenclature nomenclature, decimal price)
		{
			//TODO возможно стоит переделать пересчет стоимости платной доставки, т.к. она сейчас считается отдельно и ей ставится флаг IsUserPrice, что не совсем корректно
			//Т.к. запускается пересчет различных параметров, который может привести к добавлению платной доставки
			//создание строки с платной доставкой лучше запускать до ее поиска в коллекции
			var priceData = (SaleItemPriceType.User, price);
			var newDeliveryItem = SaleItemFactory.Create(Source, NewOrderSaleItem.Create(nomenclature, 1, priceData));
			var currentDeliveryItem = Source.SaleItems
				.SingleOrDefault(x => x.Nomenclature.Id == NomenclatureSettings.PaidDeliveryNomenclatureId);

			if(price > 0)
			{
				AddOrUpdateDeliveryItem(uow, currentDeliveryItem, newDeliveryItem, priceData);
				return;
			}
			
			if(currentDeliveryItem != null)
			{
				RemoveSaleItem(uow, currentDeliveryItem);
			}
		}
		
		private void AddOrUpdateDeliveryItem(
			IUnitOfWork uow,
			ISaleItem currentDeliveryItem,
			ISaleItem newDeliveryItem,
			(SaleItemPriceType PriceType, decimal Price) priceData)
		{
			if(currentDeliveryItem is null)
			{
				AddSaleItem(uow, newDeliveryItem, priceData);
				return;
			}

			if(currentDeliveryItem.Price == priceData.Price)
			{
				return;
			}

			SetPrice(currentDeliveryItem, priceData);
		}

		#endregion
	}
}
