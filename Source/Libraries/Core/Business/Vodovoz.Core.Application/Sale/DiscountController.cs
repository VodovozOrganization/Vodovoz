using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.Orders;
using VodovozBusiness.Controllers;

namespace Vodovoz.Core.Application.Sale
{
	public class DiscountController : IDiscountController
	{
		public DiscountController(ILogger<DiscountController> logger)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected ILogger<DiscountController> Logger { get; }

		/// <inheritdoc/>
		public Result IsApplicableDiscount(DiscountReason addingDiscount, IApplyDiscountReasonItem saleItem)
		{
			if(addingDiscount is null)
			{
				throw new ArgumentNullException(nameof(addingDiscount));
			}

			var isNotApplicableDiscount = CanApplyByType(addingDiscount, saleItem);

			if(isNotApplicableDiscount)
			{
				return Result.Failure(DiscountErrors.DiscountNotAllowed);
			}
			
			return CanApplyDiscount(addingDiscount, saleItem);
		}

		private bool CanApplyByType(DiscountReason addingDiscount, IApplyDiscountReasonItem saleItem)
		{
			var notApplicableDiscounts = addingDiscount.DiscountApplicabilities
				.Where(x => x.UseDiscountType == UseDiscountType.NotApplicable)
				.ToList();

			var isNotApplicableDiscount = false;
			
			foreach(var notApplicableDiscount in notApplicableDiscounts)
			{
				foreach(var discountReason in saleItem.DiscountReasons)
				{
					if((!saleItem.IsFixedPrice || notApplicableDiscount.DiscountType != DiscountType.FixedPrice)
						&& (int)discountReason.DiscountReasonType != (int)notApplicableDiscount.DiscountType)
					{
						continue;
					}

					isNotApplicableDiscount = true;
					break;
				}
			}

			return isNotApplicableDiscount;
		}
		
		private Result CanApplyDiscount(DiscountReason addingDiscount, IApplyDiscountReasonItem saleItem)
		{
			if(saleItem.Nomenclature is null)
			{
				throw new InvalidOperationException("Что-то пошло не так! При применении скидки должна быть заполнена номенклатура");
			}

			if(saleItem.CurrentRawPrice == 0)
			{
				return Result.Failure(DiscountErrors.ZeroSaleItemSum);
			}

			/*if(saleItem.Nomenclature is null)
			{
				//TODO проверить работу с полноценными сущностями
				return CanApplyToPromoSet(saleItem.PromoSet.Id, addingDiscount.PromoSets.Select(x => x.Id).ToArray())
					.ToResult(DiscountErrors.DiscountNotAllowed);
			}*/

			return (
				CanApplyToNomenclature(saleItem.Nomenclature.Id, addingDiscount.Nomenclatures)
				|| CanApplyToNomenclatureCategory(saleItem.Nomenclature.Category, addingDiscount.NomenclatureCategories)
				|| CanApplyToProductGroup(saleItem.Nomenclature.ProductGroup, addingDiscount.ProductGroups)
				|| CanApplyToPromoSet(saleItem.PromoSet?.Id, addingDiscount.PromoSets.Select(x => x.Id).ToArray()))
				.ToResult(DiscountErrors.DiscountNotAllowed);
		}

		/// <summary>
		/// Содержит ли основание скидки соответствующую категорию номенклатуры 
		/// </summary>
		/// <param name="nomenclatureCategory">Категория номенклатуры</param>
		/// <param name="discountNomenclatureCategories">Список категорий номенклатур у основания скидки</param>
		/// <returns>true/false</returns>
		private bool CanApplyToNomenclatureCategory(
			NomenclatureCategory nomenclatureCategory,
			IEnumerable<DiscountReasonNomenclatureCategory> discountNomenclatureCategories
			)
		{
			return discountNomenclatureCategories.Any(x => x.NomenclatureCategory == nomenclatureCategory);
		}
		
		/// <summary>
		/// Содержит ли основание скидки ссылку на указанную номенклатуру
		/// </summary>
		/// <param name="nomenclatureId">Id номенклатуры</param>
		/// <param name="discountNomenclatures">Список номенклатур основания скидки</param>
		/// <returns>ture/false</returns>
		private bool CanApplyToNomenclature(int nomenclatureId, IEnumerable<Nomenclature> discountNomenclatures) =>
			discountNomenclatures.Any(n => n.Id == nomenclatureId);

		/// <summary>
		/// Содержит ли основание скидки в списке товарную группу строки заказа 
		/// </summary>
		/// <param name="itemProductGroup">Товарная группа строки заказа</param>
		/// <param name="discountProductGroups">Товарные группы основания скидки</param>
		/// <returns>true/false</returns>
		private bool CanApplyToProductGroup(ProductGroup itemProductGroup, IEnumerable<ProductGroup> discountProductGroups) =>
			itemProductGroup != null
			&& discountProductGroups.Any(discountProductGroup => CanApplyToProductGroup(itemProductGroup, discountProductGroup));
		
		/// <summary>
		/// Проверяет соответствие товарных групп у основания скидки и строки заказа,
		/// с обходом всех ее родительских групп
		/// </summary>
		/// <param name="itemProductGroup">Товарная группа строки заказа</param>
		/// <param name="discountProductGroup">Товарная группа основания скидки</param>
		/// <returns>true/false</returns>
		private bool CanApplyToProductGroup(ProductGroup itemProductGroup, ProductGroup discountProductGroup)
		{
			while(true)
			{
				if(itemProductGroup == discountProductGroup)
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
		
		private bool CanApplyToPromoSet(int? promoSetId, IEnumerable<int> promoSetIds)
		{
			return promoSetId.HasValue && promoSetIds.Any(id => promoSetId.Value == id);
		}
	}
}
