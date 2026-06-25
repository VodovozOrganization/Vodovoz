using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Errors.PromoSets;
using VodovozBusiness.Factories;
using VodovozBusiness.Handlers;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services.ItemsHandlers
{
	public abstract class ProductHandler : IProductHandler
	{
		protected ProductHandler(
			ISaleItemFactory saleItemFactory,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureService nomenclatureService,
			INomenclatureRepository nomenclatureRepository,
			IGoodsPriceCalculator goodsPriceCalculator,
			IFixedPriceHandler fixedPriceHandler,
			IAddProductValidator addProductValidator
			)
		{
			SaleItemFactory = saleItemFactory ?? throw new ArgumentNullException(nameof(saleItemFactory));
			NomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			NomenclatureService = nomenclatureService ?? throw new ArgumentNullException(nameof(nomenclatureService));
			NomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			GoodsPriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			FixedPriceHandler = fixedPriceHandler ?? throw new ArgumentNullException(nameof(fixedPriceHandler));
			AddProductValidator = addProductValidator ?? throw new ArgumentNullException(nameof(addProductValidator));
		}

		protected IUnitOfWork UoW { get; private set; }
		protected IAddSaleItemSource SaleItemSource { get; private set; }
		protected Nomenclature FastDeliveryNomenclature { get; private set; }
		protected bool Initialized { get; private set; }
		protected INomenclatureSettings NomenclatureSettings { get; }
		protected INomenclatureService NomenclatureService { get; }
		protected IGoodsPriceCalculator GoodsPriceCalculator { get; }
		protected IFixedPriceHandler FixedPriceHandler { get; }
		protected IAddProductValidator AddProductValidator { get; }
		protected INomenclatureRepository NomenclatureRepository { get; }
		protected ISaleItemFactory SaleItemFactory { get; }
		private bool HasPermissionsForAlternativePrice { get; }

		public void Initialize(
			IUnitOfWork uow,
			IAddSaleItemSource saleItemSource)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			SaleItemSource = saleItemSource ?? throw new ArgumentNullException(nameof(saleItemSource));
			FastDeliveryNomenclature = NomenclatureRepository.GetFastDeliveryNomenclature(UoW);
			Initialized = true;
		}

		public virtual Result TryAddProduct(
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			IEnumerable<DiscountReason> discountReasons = null
		)
		{
			ThrowIfNotInitialized();
			var addProductValidateResult = AddProductValidator.Validate(nomenclature, SaleItemSource);
			
			if(addProductValidateResult.IsFailure)
			{
				return addProductValidateResult;
			}

			AddSaleItem(nomenclature, count, discount);
			return Result.Success();
		}

		#region Промонаборы

		/// <summary>
		/// Активация промонабора при добавлении в продажу(заказ, шаблон)
		/// </summary>
		/// <param name="proSet">Добавляемый промонабор</param>
		public virtual void ActivatePromotionalSet(PromotionalSet proSet)
		{
			TryAddNomenclatureFromPromoSet(proSet);
		}

		protected virtual Result TryAddNomenclatureFromPromoSet(PromotionalSet proSet)
		{
			string promoSetMessage = null;
			
			if(proSet is null)
			{
				promoSetMessage = "Промонабор не найден";
			}
			else
			{
				if(proSet.IsArchive)
				{
					promoSetMessage = "Промонабор в архиве";
				}

				if(!proSet.PromotionalSetItems.Any())
				{
					promoSetMessage = "Промонабор без товаров";
				}
			}
			
			if(promoSetMessage != null)
			{
				return Result.Failure(new Error(
						"InvalidPromotionalSet",
						$"Невалидный промонабор: {promoSetMessage}"
					)
				);
			}

			foreach(var proSetItem in proSet.PromotionalSetItems)
			{
				var nomenclature = proSetItem.Nomenclature;
				var addProductResult = AddProductValidator.Validate(nomenclature, SaleItemSource);

				if(addProductResult.IsFailure)
				{
					return addProductResult;
				}

				AddSaleItem(
					proSetItem.Nomenclature,
					proSetItem.Count,
					proSetItem.IsDiscountInMoney ? proSetItem.DiscountMoney : proSetItem.Discount,
					proSetItem.IsDiscountInMoney,
					true,
					null,
					proSetItem.PromoSet
				);
			}

			//TODO уточнить по поводу расчета стоимости доставки
			return Result.Success();
		}

		/// <summary>
		/// Попытка найти и удалить промонабор, если нет больше позиций с промонабором
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		public virtual void TryToRemovePromotionalSet(IProduct saleItem)
		{
			//TODO возможно стоит переделать на удаление всего промонабора, если позиция принадлежит ему
			var proSetFromOrderItem = saleItem.PromoSet;
			if(proSetFromOrderItem != null)
			{
				var proSetToRemove = ObservablePromotionalSets.FirstOrDefault(s => s == proSetFromOrderItem);
				if(proSetToRemove != null && !OrderItems.Any(i => i.PromoSet == proSetToRemove))
				{
					foreach(PromotionalSetActionBase action in proSetToRemove.ObservablePromotionalSetActions)
					{
						action.Deactivate(this);
					}

					ObservablePromotionalSets.Remove(proSetToRemove);
				}
			}
		}

		#endregion Промонаборы
		
		public virtual void AddSaleItem(
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			bool discountInMoney = false,
			bool needGetFixedPrice = true,
			IEnumerable<DiscountReason> discountReasons = null,
			PromotionalSet proSet = null
		)
		{
			ThrowIfNotInitialized();
			switch(nomenclature.Category)
			{
				case NomenclatureCategory.water:
					AddWaterForSale(
						nomenclature,
						count,
						discount,
						discountInMoney,
						needGetFixedPrice,
						discountReasons?.FirstOrDefault(),
						proSet);
					break;
				case NomenclatureCategory.master:
					//TODO уточнить по поводу добавления сервисного обслуживания в заказ, через на продажу
					//проверить работу, раньше перед добавлением обновлялся договор
					AddMasterNomenclature(count, nomenclature);
					break;
				default:
					var canApplyAlternativePrice = HasPermissionsForAlternativePrice && nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

					var product = SaleItemFactory.Create(
						SaleItemSource.Source,
						count,
						nomenclature.GetPrice(1, canApplyAlternativePrice),
						nomenclature
					);

					var acceptableCategories = NomenclatureEntity.GetCategoriesForSale();
					
					if(product?.Nomenclature is null || !acceptableCategories.Contains(product.Nomenclature.Category))
					{
						return;
					}
					
					AddSaleItem(product);
					break;
			}
		}
		
		public virtual void AddWaterForSale(
			Nomenclature nomenclature,
			decimal count,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			bool needGetFixedPrice = true,
			DiscountReason reason = null,
			PromotionalSet proSet = null
		)
		{
			ThrowIfNotInitialized();
			if(nomenclature.Category != NomenclatureCategory.water && !nomenclature.IsDisposableTare)
			{
				return;
			}

			//Если номенклатура промонабора добавляется по фиксе (без скидки), то у нового OrderItem убирается поле discountReason
			if(proSet != null && discount == 0)
			{
				var fixPricedNomenclaturesId = Array.Empty<int>();//GetNomenclaturesWithFixPrices.Select(n => n.Id);
				if(fixPricedNomenclaturesId.Contains(nomenclature.Id))
				{
					reason = null;
				}
			}

			if(discount > 0 && reason == null && proSet == null)
			{
				throw new ArgumentException("Требуется указать причину скидки (reason), если она (discount) больше 0!");
			}

			var price = GoodsPriceCalculator.CalculatePrice(
				new List<ICalculatingPriceWithManyDiscounts>(),//addProductSource.Products,
				SaleItemSource.Counterparty,
				SaleItemSource.DeliveryPoint,
				nomenclature,
				proSet != null,
				HasPermissionsForAlternativePrice,
				count,
				needGetFixedPrice);
			
			AddSaleItem(
				SaleItemFactory.Create(
					SaleItemSource.Source,
					count,
					price,
					nomenclature
				)
			);
		}
	
		/// <summary>
		/// Добавление в заказ номенклатуры типа "Сервисное обслуживание"
		/// </summary>
		/// <param name="nomenclature">Номенклатура типа "Сервисное обслуживание"</param>
		/// <param name="count">Количество</param>
		/// <param name="quantityOfFollowingNomenclatures">Колличество номенклатуры, указанной в параметрах БД,
		/// которые будут добавлены в заказ вместе с мастером</param>
		public virtual void AddMasterNomenclature(
			decimal count,
			Nomenclature nomenclature,
			int quantityOfFollowingNomenclatures = 0)
		{
			ThrowIfNotInitialized();
			if(nomenclature.Category != NomenclatureCategory.master)
			{
				return;
			}

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			AddSaleItem(
				SaleItemFactory.Create(SaleItemSource.Source, count, nomenclature.GetPrice(1, canApplyAlternativePrice), nomenclature)
			);

			if(quantityOfFollowingNomenclatures > 0)
			{
				var followingNomenclature = NomenclatureRepository.GetNomenclatureToAddWithMaster(UoW);
				if(!SaleItemSource.Products.Any(i => i.Nomenclature.Id == followingNomenclature.Id))
				{
					AddAnyGoodsNomenclatureForSale(
						followingNomenclature,
						false,
						1);
				}
			}
		}
	
		public virtual void AddAnyGoodsNomenclatureForSale(
			Nomenclature nomenclature,
			bool isChangeOrder = false,
			int? cnt = null)
		{
			ThrowIfNotInitialized();
			var acceptableCategories = NomenclatureEntity.GetCategoriesForSale();
			if(!acceptableCategories.Contains(nomenclature.Category))
			{
				return;
			}

			//TODO убрать отсюда назначение количества в UI
			var count = (nomenclature.Category == NomenclatureCategory.service
				|| nomenclature.Category == NomenclatureCategory.deposit) && !isChangeOrder ? 1 : 0;

			if(cnt.HasValue)
			{
				count = cnt.Value;
			}

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			AddSaleItem(
				SaleItemFactory.Create(
					SaleItemSource.Source,
					count,
					nomenclature.GetPrice(1, canApplyAlternativePrice),
					nomenclature
				)
			);
		}
	
		public virtual void AddSaleItem(
			IProduct addingItem,
			bool forceUseAlternativePrice = false)
		{
			ThrowIfNotInitialized();
			if(SaleItemSource.Products.Contains(addingItem))
			{
				return;
			}

			var curCount = SaleItemSource.TotalItemCount(addingItem);
			
			//TODO: уточнить по поводу альтернативных цен, некоторые пользователи смогут их проставить если будут создавать шаблоны
			var isAlternativePriceCopiedFromUndelivery = addingItem.IsCopiedFromUndelivery && addingItem.IsAlternativePrice;
			var canApplyAlternativePrice =
				isAlternativePriceCopiedFromUndelivery
				|| (HasPermissionsForAlternativePrice
					&& addingItem.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount)
					&& FixedPriceHandler.GetWaterFixedPrice(SaleItemSource, addingItem) == null);

			addingItem.IsAlternativePrice = canApplyAlternativePrice;

			SaleItemSource.Products.Add(addingItem);
			Recalculate();

			if(SaleItemSource.Products.Any(x => x.Nomenclature.Id == NomenclatureSettings.MasterCallNomenclatureId))
			{
				NomenclatureService.CalculateMasterCallNomenclaturePriceIfNeeded(UoW, SaleItemSource.Source as Order);
			}
		}

		/// <summary>
		/// Добавить товары из выбранного предыдущего заказа.
		/// </summary>
		/// <param name="addingItem">Элемент заказа.</param>
		public virtual void AddNomenclatureForSaleFromPreviousOrder(IProduct addingItem)
		{
			ThrowIfNotInitialized();
			if(addingItem.Nomenclature.Category != NomenclatureCategory.additional)
			{
				return;
			}

			AddSaleItem(SaleItemFactory.Create(SaleItemSource, addingItem.Count, addingItem.Price, addingItem.Nomenclature));
		}
		
		public virtual void AddFastDeliveryNomenclatureIfNeeded()
		{
			ThrowIfNotInitialized();
			if(SaleItemSource.IsFastDelivery && SaleItemSource.Products.All(x => x.Nomenclature.Id != FastDeliveryNomenclature.Id))
			{
				var canApplyAlternativePrice = HasPermissionsForAlternativePrice
					&& FastDeliveryNomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= 1);

				AddSaleItem(
					SaleItemFactory.Create(
						SaleItemSource, 1, FastDeliveryNomenclature.GetPrice(1, canApplyAlternativePrice), FastDeliveryNomenclature));
			}
		}
		
		public virtual void UpdateDeliveryItem(
			Nomenclature nomenclature,
			decimal price)
		{
			ThrowIfNotInitialized();
			//Т.к. запускается пересчет различных параметров, который может привести к добавлению платной доставки
			//создание строки с платной доставкой лучше запускать до ее поиска в коллекции
			var newDeliveryItem = SaleItemFactory.CreateDeliveryOrderItem(this, nomenclature, price);
			var currentDeliveryItem = SaleItemSource.Products
				.SingleOrDefault(x => x.Nomenclature.Id == NomenclatureSettings.PaidDeliveryNomenclatureId);

			if(price > 0)
			{
				AddOrUpdateDeliveryItem(currentDeliveryItem, newDeliveryItem, price);
				return;
			}
			
			if(currentDeliveryItem != null)
			{
				RemoveSaleItem(currentDeliveryItem);
			}
		}

		/// <summary>
		/// Добавление/удаление номенклатуры для вызова мастера в зависимости от типа адреса
		/// </summary>
		public virtual void UpdateMasterCallNomenclatureIfNeeded()
		{
			ThrowIfNotInitialized();
			var masterCallNomenclature = NomenclatureRepository.GetMasterCallNomenclature(UoW);

			if(OrderAddressType == OrderAddressType.Service
				&& !SaleItemSource.IsSelfDelivery)
			{
				AddMasterCallNomenclatureIfNeeded(masterCallNomenclature);
			}
			else
			{
				RemoveMasterCallNomenclature(masterCallNomenclature);
			}
		}
		
		#region Аренда

        #region NonFreeRent

        public virtual void AddNonFreeRent(
			PaidRentPackage paidRentPackage,
			Nomenclature equipmentNomenclature)
		{
			ThrowIfNotInitialized();
			AddNonFreeRentDepositItems(paidRentPackage, out var orderRentServiceItem);
			SaleItemSource.OnSumPropertiesChanged();
		}

		protected IProduct AddNonFreeRentDepositItems(
			PaidRentPackage paidRentPackage,
			out IProduct orderRentServiceItem
			)
		{
			ThrowIfNotInitialized();
			var orderRentDepositItem = GetExistingNonFreeRentDepositItem(paidRentPackage);
			if(orderRentDepositItem == null)
			{
				orderRentDepositItem = SaleItemFactory.CreateNewNonFreeRentDepositItem(this, paidRentPackage);
				AddSaleItem(orderRentDepositItem);
			}

			orderRentServiceItem = GetExistingNonFreeRentServiceItem(paidRentPackage);
			if(orderRentServiceItem == null)
			{
				orderRentServiceItem = SaleItemFactory.CreateNewNonFreeRentServiceItem(this, paidRentPackage);
				AddSaleItem(orderRentServiceItem);
			}

			return orderRentDepositItem;
		}

		private IProduct GetExistingNonFreeRentDepositItem(PaidRentPackage paidRentPackage)
		{
			var orderRentDepositItem = SaleItemSource.Products
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == SaleRentType.NonFreeRent)
				.FirstOrDefault(x => x.OrderItemRentSubType == OrderItemRentSubType.RentDepositItem);
			
			return orderRentDepositItem;
		}

		private IProduct GetExistingNonFreeRentServiceItem(PaidRentPackage paidRentPackage)
		{
			var orderRentServiceItem = SaleItemSource.Products
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == SaleRentType.NonFreeRent)
				.FirstOrDefault(x => x.OrderItemRentSubType == OrderItemRentSubType.RentServiceItem);
			
			return orderRentServiceItem;
		}

		#endregion NonFreeRent

		#region DailyRent

		public virtual void AddDailyRent(
			PaidRentPackage paidRentPackage,
			Nomenclature equipmentNomenclature)
		{
			ThrowIfNotInitialized();
			AddDailyRentDepositItems(paidRentPackage, out var orderRentServiceItem);
			SaleItemSource.OnSumPropertiesChanged();
		}

		protected IProduct AddDailyRentDepositItems(
			PaidRentPackage paidRentPackage,
			out IProduct orderRentServiceItem)
		{
			ThrowIfNotInitialized();
			var orderRentDepositItem = GetExistingDailyRentDepositItem(paidRentPackage);
			if(orderRentDepositItem is null)
			{
				orderRentDepositItem = SaleItemFactory.CreateNewDailyRentDepositItem(this, paidRentPackage);
				AddSaleItem(orderRentDepositItem);
			}

			orderRentServiceItem = GetExistingDailyRentServiceItem(paidRentPackage);
			if(orderRentServiceItem is null)
			{
				orderRentServiceItem = SaleItemFactory.CreateNewDailyRentServiceItem(this, paidRentPackage);
				AddSaleItem(orderRentServiceItem);
			}

			return orderRentDepositItem;
		}

		private IProduct GetExistingDailyRentDepositItem(PaidRentPackage paidRentPackage)
		{
			var orderRentDepositItem = SaleItemSource.Products
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == SaleRentType.DailyRent)
				.FirstOrDefault(x => x.OrderItemRentSubType == OrderItemRentSubType.RentDepositItem);
			
			return orderRentDepositItem;
		}

		private IProduct GetExistingDailyRentServiceItem(PaidRentPackage paidRentPackage)
		{
			var orderRentServiceItem = SaleItemSource.Products
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == SaleRentType.DailyRent)
				.FirstOrDefault(x => x.OrderItemRentSubType == OrderItemRentSubType.RentServiceItem);
			
			return orderRentServiceItem;
		}

		#endregion DailyRent

		#region FreeRent

		public virtual void AddFreeRent(
			FreeRentPackage freeRentPackage,
			Nomenclature equipmentNomenclature)
		{
			ThrowIfNotInitialized();
			AddFreeRentDepositItem(freeRentPackage);
			SaleItemSource.OnSumPropertiesChanged();
		}

		protected IProduct AddFreeRentDepositItem(FreeRentPackage freeRentPackage)
		{
			ThrowIfNotInitialized();
			var orderRentDepositItem = GetExistingFreeRentDepositItem(freeRentPackage);
			if(orderRentDepositItem == null)
			{
				orderRentDepositItem = SaleItemFactory.CreateNewFreeRentDepositItem(this, freeRentPackage);
				AddSaleItem(orderRentDepositItem);
			}

			return orderRentDepositItem;
		}

		protected IProduct GetExistingFreeRentDepositItem(FreeRentPackage freeRentPackage)
		{
			ThrowIfNotInitialized();
			var orderRentDepositItem = SaleItemSource.Products
				.Where(x => x.FreeRentPackage != null && x.FreeRentPackage.Id == freeRentPackage.Id)
				.Where(x => x.RentType == SaleRentType.FreeRent)
				.FirstOrDefault(x => x.OrderItemRentSubType == OrderItemRentSubType.RentDepositItem);
			
			return orderRentDepositItem;
		}

		#endregion FreeRent

		#endregion Аренда

		public virtual void RemoveSaleItem(IProduct removableProduct)
		{
			ThrowIfNotInitialized();
			var items = SaleItemSource.Products;
			
			if(!items.Contains(removableProduct))
			{
				return;
			}

			if(removableProduct.PromoSet != null)
			{
				var itemsToRemove = items.Where(oi => oi.PromoSet == removableProduct.PromoSet).ToList();
				foreach (var item in itemsToRemove)
				{
					items.Remove(item);
				}
			}
			else
			{
				items.Remove(removableProduct);
			}
		}
		
		public virtual void RemoveFastDeliveryNomenclature()
		{
			ThrowIfNotInitialized();
			var fastDeliveryItemToRemove = SaleItemSource.Products.SingleOrDefault(x => x.Nomenclature.Id == FastDeliveryNomenclature.Id);
			RemoveSaleItem(fastDeliveryItemToRemove);
		}
		
		public virtual void RemoveItem(IProduct removableItem)
		{
			ThrowIfNotInitialized();
			RemoveSaleItem(removableItem);
			NomenclatureService.CalculateMasterCallNomenclaturePriceIfNeeded(UoW, SaleItemSource);
		}

		protected virtual void Recalculate()
		{
			ThrowIfNotInitialized();
			SaleItemSource.RecalculateItemsPrice();
			SaleItemSource.UpdateRentsCount();
		}

		protected void ThrowIfNotInitialized()
		{
			if(!Initialized)
			{
				throw new InvalidOperationException(
					$"Класс {nameof(IProductHandler)} не инициализирован. Перед началом работы надо вызвать метод {nameof(Initialize)}");
			}
		}

		private void AddOrUpdateDeliveryItem(
			IProduct currentDeliveryItem,
			IProduct newDeliveryItem,
			decimal price
			)
		{
			if(currentDeliveryItem is null)
			{
				AddSaleItem(newDeliveryItem);
				return;
			}

			if(currentDeliveryItem.Price == price)
			{
				return;
			}

			currentDeliveryItem.SetPrice(price);
		}

		private void AddMasterCallNomenclatureIfNeeded(Nomenclature masterCallNomenclature)
		{
			if(SaleItemSource.Products.Any(x => x.Nomenclature.Id == masterCallNomenclature.Id))
			{
				return;
			}

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
				&& masterCallNomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= 1);

			AddSaleItem(SaleItemFactory.Create(SaleItemSource, 1, 0, masterCallNomenclature));
		}

		private void RemoveMasterCallNomenclature(Nomenclature masterCallNomenclature)
		{
			var fastDeliveryItemToRemove = SaleItemSource.Products.SingleOrDefault(x => x.Nomenclature.Id == masterCallNomenclature.Id);

			RemoveSaleItem(fastDeliveryItemToRemove);
		}
	}
}
