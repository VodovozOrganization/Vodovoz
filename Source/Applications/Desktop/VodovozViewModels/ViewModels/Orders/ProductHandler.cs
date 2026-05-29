using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Factories;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public abstract class ProductHandler
	{
		private readonly ISaleItemFactory _saleItemFactory;

		protected ProductHandler(
			ISaleItemFactory saleItemFactory,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureService nomenclatureService,
			INomenclatureRepository nomenclatureRepository,
			IGoodsPriceCalculator goodsPriceCalculator,
			IAddProductValidator addProductValidator
			)
		{
			_saleItemFactory = saleItemFactory;
			NomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			NomenclatureService = nomenclatureService ?? throw new ArgumentNullException(nameof(nomenclatureService));
			NomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			GoodsPriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			AddProductValidator = addProductValidator ?? throw new ArgumentNullException(nameof(addProductValidator));
		}

		protected INomenclatureSettings NomenclatureSettings { get; }
		protected INomenclatureService NomenclatureService { get; }
		protected IGoodsPriceCalculator GoodsPriceCalculator { get; }
		protected IAddProductValidator AddProductValidator { get; }
		protected INomenclatureRepository NomenclatureRepository { get; }
	
		public virtual Result TryAddProduct(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			IEnumerable<DiscountReason> discountReasons = null
		)
		{
			var addProductValidateResult = AddProductValidator.Validate(nomenclature, addProductSource);
			
			if(addProductValidateResult.IsFailure)
			{
				return addProductValidateResult;
			}

			AddProduct(uow, addProductSource, nomenclature, count, discount);
			return Result.Success();
		}
		
		protected virtual void AddProduct(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			bool discountInMoney = false,
			bool needGetFixedPrice = true,
			IEnumerable<DiscountReason> discountReasons = null,
			PromotionalSet proSet = null
		)
		{
			switch(nomenclature.Category)
			{
				case NomenclatureCategory.water:
					AddWaterForSale(
						uow,
						addProductSource,
						nomenclature,
						count,
						discount,
						discountInMoney,
						needGetFixedPrice,
						discountReasons?.FirstOrDefault(),
						proSet);
					break;
				case NomenclatureCategory.master:
					//проверить работу, раньше перед добавлением обновлялся договор
					AddMasterNomenclature(uow, addProductSource, 1, nomenclature);
					break;
				default:
					var canApplyAlternativePrice = HasPermissionsForAlternativePrice && nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

					var product = _saleItemFactory.Create(
						addProductSource.Source,
						count,
						nomenclature.GetPrice(1, canApplyAlternativePrice),
						nomenclature
					);

					var acceptableCategories = Nomenclature.GetCategoriesForSale();
					
					if(product?.Nomenclature is null || !acceptableCategories.Contains(product.Nomenclature.Category))
					{
						return;
					}
					
					AddSaleItem(uow, addProductSource, product);

					break;
			}
		}
		
		public virtual void AddWaterForSale(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
			Nomenclature nomenclature,
			decimal count,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			bool needGetFixedPrice = true,
			DiscountReason reason = null,
			PromotionalSet proSet = null
		)
		{
			if(nomenclature.Category != NomenclatureCategory.water && !nomenclature.IsDisposableTare)
			{
				return;
			}

			//Если номенклатура промонабора добавляется по фиксе (без скидки), то у нового OrderItem убирается поле discountReason
			if(proSet != null && discount == 0) {
				var fixPricedNomenclaturesId = GetNomenclaturesWithFixPrices.Select(n => n.Id);
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
				addProductSource.Products,
				addProductSource.Counterparty,
				addProductSource.DeliveryPoint,
				nomenclature,
				proSet != null,
				HasPermissionsForAlternativePrice,
				count,
				needGetFixedPrice);
			
			AddSaleItem(
				uow,
				addProductSource,
				_saleItemFactory.Create(
					addProductSource.Source,
					count,
					price,
					nomenclature
				)
			);
		}
	
		/// <summary>
		/// Добавление в заказ номенклатуры типа "Сервисное обслуживание"
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="nomenclature">Номенклатура типа "Сервисное обслуживание"</param>
		/// <param name="count">Количество</param>
		/// <param name="quantityOfFollowingNomenclatures">Колличество номенклатуры, указанной в параметрах БД,
		/// которые будут добавлены в заказ вместе с мастером</param>
		public virtual void AddMasterNomenclature(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
			int count,
			Nomenclature nomenclature,
			int quantityOfFollowingNomenclatures = 0)
		{
			if(nomenclature.Category != NomenclatureCategory.master) {
				return;
			}

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			AddSaleItem(
				uow,
				addProductSource,
				_saleItemFactory.Create(addProductSource.Source, count, nomenclature.GetPrice(1, canApplyAlternativePrice), nomenclature)
			);

			if(quantityOfFollowingNomenclatures > 0)
			{
				var followingNomenclature = NomenclatureRepository.GetNomenclatureToAddWithMaster(uow);
				if(!addProductSource.Products.Any(i => i.Nomenclature.Id == followingNomenclature.Id))
				{
					AddAnyGoodsNomenclatureForSale(
						uow,
						addProductSource,
						followingNomenclature,
						false,
						1);
				}
			}
		}
	
		public virtual void AddAnyGoodsNomenclatureForSale(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
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

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			AddSaleItem(
				uow,
				addProductSource,
				_saleItemFactory.Create(
					addProductSource.Source,
					count,
					nomenclature.GetPrice(1, canApplyAlternativePrice),
					nomenclature
				)
			);
		}
	
		public virtual void AddSaleItem(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
			IProduct item,
			bool forceUseAlternativePrice = false)
		{
			if(addProductSource.Products.Contains(item))
			{
				return;
			}

			var curCount = item.Nomenclature.IsWater19L
				? GetTotalWater19LCount(true, true)
				: item.Count;
			
			//TODO: уточнить по поводу альтернативных цен, некоторые пользователи смогут их проставить если будут создавать шаблоны
			var isAlternativePriceCopiedFromUndelivery = item.CopiedFromUndelivery != null && item.IsAlternativePrice;
			var canApplyAlternativePrice =
				isAlternativePriceCopiedFromUndelivery
				|| (HasPermissionsForAlternativePrice
					&& item.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount)
					&& item.GetWaterFixedPrice() == null);

			item.IsAlternativePrice = canApplyAlternativePrice;

			addProductSource.Products.Add(item);
			Recalculate();

			if(addProductSource.Products.Any(x => x.Nomenclature.Id == NomenclatureSettings.MasterCallNomenclatureId))
			{
				NomenclatureService.CalculateMasterCallNomenclaturePriceIfNeeded(uow, addProductSource.Source as Order);
			}
		}

		protected abstract void Recalculate();
	}
}
