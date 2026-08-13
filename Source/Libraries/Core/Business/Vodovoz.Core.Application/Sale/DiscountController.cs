using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Sale
{
	public class DiscountController : IDiscountController
	{
		/// <inheritdoc/>
		public Result IsApplicableDiscount(DiscountReason addingDiscount, IApplyDiscountReasonItem saleItem)
		{
			if(addingDiscount is null)
			{
				throw new ArgumentNullException(nameof(addingDiscount));
			}

			if(saleItem.DiscountReasons.Any(discountReason => discountReason.Id == addingDiscount.Id))
			{
				return Result.Failure(DiscountErrors.DiscountAlreadyApplied);
			}

			var isNotApplicableDiscount = CanApplyByType(addingDiscount, saleItem);

			if(isNotApplicableDiscount)
			{
				return Result.Failure(DiscountErrors.DiscountNotAllowed);
			}
			
			return CanApplyDiscount(addingDiscount, saleItem);
		}

		/// <summary>
		/// Пересчет итоговой скидки из оснований скидок
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		protected virtual void RecalculateTotalDiscountFromReasons(IApplyDiscountReasonItem saleItem)
		{
			var currentPrice = saleItem.CurrentRawPrice;
			var discountReasons = saleItem.DiscountReasons;
			var totalDiscountMoney = CalculateTotalDiscountInMoneyFromAddedReasons(saleItem);

			var discountMoney =
				discountReasons.All(x => x.ValueType == DiscountUnits.money)
					? discountReasons.Sum(x => x.Value)
					: totalDiscountMoney;

			var discount =
				discountReasons.All(x => x.ValueType == DiscountUnits.percent)
					? discountReasons.Sum(x => x.Value)
					: currentPrice > 0 ? (100 * discountMoney) / currentPrice : 0;

			var isDiscountInMoney = discountReasons.Any(x => x.ValueType == DiscountUnits.money);

			if(discountMoney > currentPrice)
			{
				discountMoney = currentPrice;
			}

			if(discount > 100)
			{
				discount = 100;
			}

			SetDiscount(saleItem, DiscountValue.Create(isDiscountInMoney, discount, discountMoney));
		}

		/// <summary>
		/// Установка скидки
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <param name="discountValue">Значение скидки <see cref="IDiscountValue"/></param>
		protected virtual void SetDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue discountValue) { }

		/// <summary>
		/// Расчет итоговой скидки в деньгах по основаниям скидки
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <returns>Скидка в деньгах</returns>
		protected virtual decimal CalculateTotalDiscountInMoneyFromAddedReasons(IApplyDiscountReasonItem saleItem)
		{
			decimal currentPrice = saleItem.CurrentRawPrice;

			var totalPercentDiscount = 0m;
			var totalMoneyDiscount = 0m;

			foreach(var reason in saleItem.DiscountReasons)
			{
				if(reason.ValueType == DiscountUnits.money)
				{
					totalMoneyDiscount += reason.Value;
				}
				else
				{
					totalPercentDiscount += reason.Value;
				}
			}

			var discountFromPercent = currentPrice * (totalPercentDiscount / 100);
			var totalDiscountMoney = discountFromPercent + totalMoneyDiscount;

			return totalDiscountMoney;
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
			if(saleItem.Nomenclature is null && saleItem.PromoSet is null)
			{
				throw new InvalidOperationException("Что-то пошло не так! При применении скидки должна быть заполнена номенклатура или промонабор");
			}
			
			if(saleItem.Nomenclature is null)
			{
				//TODO проверить работу с полноценными сущностями
				return CanApplyToPromoSet(saleItem.PromoSet.Id, addingDiscount.PromoSets.Select(x => x.Id).ToArray())
					.ToResult(DiscountErrors.DiscountNotAllowed);
			}

			return (CanApplyToNomenclature(saleItem.Nomenclature.Id, addingDiscount.Nomenclatures)
				|| CanApplyToNomenclatureCategory(saleItem.Nomenclature.Category, addingDiscount.NomenclatureCategories)
				|| CanApplyToProductGroup(saleItem.Nomenclature.ProductGroup, addingDiscount.ProductGroups))
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
		
		private bool CanApplyToPromoSet(int promoSetId, IEnumerable<int> promoSetIds)
		{
			return promoSetIds.Any(id => promoSetId == id);
		}
	}
}
