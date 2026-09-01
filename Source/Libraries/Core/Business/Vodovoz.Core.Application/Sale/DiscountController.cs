using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.Orders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class DiscountController : IDiscountController
	{
		public DiscountController(
			ILogger<DiscountController> logger,
			IDiscountReasonSettings discountReasonSettings
			)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			DiscountReasonSettings = discountReasonSettings ?? throw new ArgumentNullException(nameof(discountReasonSettings));
			PersonalDiscountReasonId = DiscountReasonSettings.PersonalDiscountReasonId;
		}

		protected ILogger<DiscountController> Logger { get; }
		protected IDiscountReasonSettings DiscountReasonSettings { get; }
		protected int PersonalDiscountReasonId { get; }

		/// <inheritdoc/>
		public Result IsApplicableDiscount(DiscountReasonBase addingDiscount, IApplicableDiscount saleItem)
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

		/// <inheritdoc/>
		public virtual (decimal TotalDiscount, IEnumerable<IDiscountAmount> DiscountDetails) CalculateTotalDiscountDetails(
			ICalculatingTotalMoneyDiscount saleItem
		)
		{
			if(saleItem is null)
			{
				throw new ArgumentNullException(
					nameof(saleItem),
					$"Продаваемая позиция должна реализовывать интерфейс {nameof(ICalculatingTotalMoneyDiscount)}");
			}
			
			var currentSumWithoutDiscount = saleItem.CurrentRawPrice;
			var discountAmounts = new List<IDiscountAmount>();
			var totalDiscountMoney = 0m;

			foreach(var discountReason in saleItem.DiscountReasons)
			{
				var discountMoney = CalculateMoneyDiscount(currentSumWithoutDiscount, discountReason);
				totalDiscountMoney += discountMoney;

				IDiscountAmount discountAmount;

				if(currentSumWithoutDiscount >= totalDiscountMoney)
				{
					discountAmount = DiscountAmount.Create(discountReason.Id, discountReason.Name, discountMoney);
				}
				else
				{
					var difference = totalDiscountMoney - currentSumWithoutDiscount;
					discountAmount = DiscountAmount.Create(
						discountReason.Id,
						discountReason.Name,
						difference >= discountMoney ? 0m : discountMoney - difference);
					totalDiscountMoney = currentSumWithoutDiscount;
				}

				discountAmounts.Add(discountAmount);
			}

			if(saleItem.PersonalDiscount != null)
			{
				totalDiscountMoney += saleItem.PersonalDiscount.DiscountValue.DiscountMoney;
				discountAmounts.Add(DiscountAmount.Create(
					saleItem.PersonalDiscount.DiscountReason.Id,
					saleItem.PersonalDiscount.DiscountReason.Name,
					saleItem.PersonalDiscount.DiscountValue.DiscountMoney)
				);
			}

			return (totalDiscountMoney, discountAmounts);
		}

		/// <summary>
		/// Расчет итоговой скидки в деньгах по основаниям скидки, исключая персональную скидку
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <returns>Скидка в деньгах</returns>
		protected virtual IDiscountValue CalculateTotalDiscountValueFromDiscountReasonsWithoutPersonalDiscount(
			ICalculatingTotalMoneyDiscount saleItem
		)
		{
			var currentPrice = saleItem.CurrentRawPrice;
			var tempDiscountValue = CalculateDiscountFromDiscountReasonsWithoutPersonalDiscount(saleItem);

			if(tempDiscountValue.Discount < 0)
			{
				tempDiscountValue.SetDiscount(0m, false);
			}

			var discountFromPercent = currentPrice * (tempDiscountValue.Discount / 100);
			var totalDiscountMoney = discountFromPercent + tempDiscountValue.DiscountMoney;
			var totalDiscountPercent =
				currentPrice == 0m
					? 0m
					: 100 * totalDiscountMoney / currentPrice;

			return DiscountValue.Create(
				tempDiscountValue.IsDiscountMoney,
				totalDiscountPercent > 100 ? 100 : totalDiscountPercent,
				totalDiscountMoney > currentPrice ? currentPrice : totalDiscountMoney);
		}

		private decimal CalculateMoneyDiscount(
			decimal currentRawPrice,
			DiscountReasonBase discountReason
			)
		{
			if(discountReason.ValueType == DiscountUnits.money)
			{
				return discountReason.Value;
			}

			return currentRawPrice * discountReason.Value / 100m;
		}

		private IDiscountValue CalculateDiscountFromDiscountReasonsWithoutPersonalDiscount(
			IDiscountReasons saleItem
		)
		{
			var percentDiscount = 0m;
			var moneyDiscount = 0m;
			var isDiscountMoney = false;

			foreach(var reason in saleItem.DiscountReasons)
			{
				if(reason.Id == PersonalDiscountReasonId)
				{
					continue;
				}
				
				if(reason.ValueType == DiscountUnits.money)
				{
					moneyDiscount += reason.Value;
					isDiscountMoney = true;
				}
				else
				{
					percentDiscount += reason.Value;
				}
			}

			return DiscountValue.Create(isDiscountMoney, percentDiscount, moneyDiscount);
		}

		private bool CanApplyByType(DiscountReasonBase addingDiscount, IApplicableDiscount saleItem)
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
		
		private Result CanApplyDiscount(DiscountReasonBase addingDiscount, IApplicableDiscount saleItem)
		{
			if(saleItem.Nomenclature is null && saleItem.PromoSet is null)
			{
				throw new InvalidOperationException("Что-то пошло не так! При применении скидки должна быть заполнена номенклатура или промонабор");
			}

			if(saleItem.CurrentRawPrice == 0)
			{
				return Result.Failure(DiscountErrors.ZeroSaleItemSum);
			}

			if(saleItem.Nomenclature is null)
			{
				//TODO проверить работу с полноценными сущностями
				return CanApplyToPromoSet(saleItem.PromoSet.Id, addingDiscount.PromoSets.Select(x => x.Id).ToArray())
					.ToResult(DiscountErrors.DiscountNotAllowed);
			}

			return (
				CanApplyToNomenclature(saleItem.Nomenclature.Id, addingDiscount.Nomenclatures)
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
		
		private bool CanApplyToPromoSet(int? promoSetId, IEnumerable<int> promoSetIds)
		{
			return promoSetId.HasValue && promoSetIds.Any(id => promoSetId.Value == id);
		}
	}
}
