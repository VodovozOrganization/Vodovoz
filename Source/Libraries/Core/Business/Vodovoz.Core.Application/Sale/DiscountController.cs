using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Errors.Orders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Sale
{
	public class DiscountController : IDiscountController
	{
		public DiscountController(
			IDiscountReasonRepository discountReasonRepository,
			IDiscountReasonSettings discountReasonSettings)
		{
			DiscountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			DiscountReasonSettings = discountReasonSettings ?? throw new ArgumentNullException(nameof(discountReasonSettings));
			PersonalDiscountReasonId = DiscountReasonSettings.PersonalDiscountReasonId;
		}

		protected IDiscountReasonRepository DiscountReasonRepository { get; }
		protected IDiscountReasonSettings DiscountReasonSettings { get; }
		protected int PersonalDiscountReasonId { get; }

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
		
		/// <inheritdoc/>
		public virtual void RemoveDiscount(int discountReasonId, IApplyDiscountReasonItem saleItem)
		{
			var discountReasons = saleItem.DiscountReasons;

			if(!discountReasons.Any())
			{
				return;
			}

			var reasonsToRemove = discountReasons
				.Where(r => r.Id == discountReasonId)
				.ToList();

			RemoveDiscountReasons(saleItem, reasonsToRemove);
			RecalculateTotalDiscount(saleItem);
		}

		public virtual void RecalculateDiscount(IDataContext context)
		{
			if(context.Data is not CommonRecalculateDiscount data)
			{
				throw new InvalidOperationException($"Передаваемый контекст для пересчета скидки должен быть {nameof(CommonRecalculateDiscount)}");
			}
			
			var saleItem = data.SaleItem;
			var newDiscount = data.DiscountValue;
			
			if(saleItem.CurrentCount == 0)
			{
				ClearDiscounts(saleItem);
			}
			else if(saleItem.DiscountReasons.Any())
			{
				RecalculateTotalDiscount(saleItem);
			}
			else
			{
				CalculateAndSetDiscount(
					saleItem,
					newDiscount ?? throw new InvalidOperationException("Не должно было сюда прийти пустого значения скидки"));
			}
		}

		/// <inheritdoc/>
		public Result SetCustomDiscount(
			IUnitOfWork uow,
			IApplyDiscountReasonItem saleItem,
			IDiscountValue receivedDiscountValue
		)
		{
			if(receivedDiscountValue.IsZeroDiscount)
			{
				ClearDiscounts(saleItem);
				return Result.Success();
			}
			
			var personalDiscountReasonId = PersonalDiscountReasonId;
			var personalDiscount = saleItem.PersonalDiscount;

			if(personalDiscount is null)
			{
				var personalDiscountReason = DiscountReasonRepository.GetDiscountReason(uow, personalDiscountReasonId);

				if(personalDiscountReason is null)
				{
					throw new InvalidOperationException(
						"В базе не найдено основание скидки Персональная скидка! Она необходима для установки индивидуальной скидки");
				}

				personalDiscount = PersonalDiscount.Create(personalDiscountReason, DiscountReasonSettings);
				
				var canApplyResult = CanApplyDiscount(personalDiscountReason, saleItem);
				if(canApplyResult.IsFailure)
				{
					return canApplyResult;
				}
			}
			
			var totalDiscountValueFromReasons = CalculateTotalDiscountValueFromDiscountReasonsWithoutPersonalDiscount(saleItem);
			var newDiscountValue = ProcessPersonalDiscount(uow, saleItem, receivedDiscountValue, totalDiscountValueFromReasons, personalDiscount);
			
			SetDiscount(saleItem, newDiscountValue);
			return Result.Success();
		}

		private static IDiscountValue ProcessPersonalDiscount(
			IUnitOfWork uow,
			IApplyDiscountReasonItem saleItem,
			IDiscountValue receivedDiscountValue,
			IDiscountValue totalDiscountValueFromReasons,
			PersonalDiscount personalDiscount)
		{
			IDiscountValue newDiscountValue = null;
			
			if(receivedDiscountValue.IsDiscountMoney)
			{
				newDiscountValue = ProcessPersonalDiscount(
					uow,
					saleItem,
					receivedDiscountValue,
					totalDiscountValueFromReasons,
					receivedDiscountValue.DiscountMoney,
					totalDiscountValueFromReasons.DiscountMoney,
					personalDiscount);
			}
			else
			{
				newDiscountValue = ProcessPersonalDiscount(
					uow,
					saleItem,
					receivedDiscountValue,
					totalDiscountValueFromReasons,
					receivedDiscountValue.Discount,
					totalDiscountValueFromReasons.Discount,
					personalDiscount);
			}

			return newDiscountValue;
		}

		private static IDiscountValue ProcessPersonalDiscount(
			IUnitOfWork uow,
			IApplyDiscountReasonItem saleItem,
			IDiscountValue receivedDiscountValue,
			IDiscountValue totalDiscountValueFromReasons,
			decimal receivedDiscount,
			decimal totalDiscount,
			PersonalDiscount personalDiscount)
		{
			if(receivedDiscount > totalDiscount)
			{
				var differenceValue = receivedDiscount - totalDiscount;
				var newPersonalDiscountValue = CalculateDiscount(
					saleItem,
					DiscountValue.Create(receivedDiscountValue.IsDiscountMoney, differenceValue, differenceValue)
				);
				
				totalDiscountValueFromReasons.AddDiscountValue(newPersonalDiscountValue);
				personalDiscount.SetDiscount(newPersonalDiscountValue);

				if(personalDiscount.Id == 0)
				{
					saleItem.DiscountReasons.Add(personalDiscount.DiscountReason);
					saleItem.PersonalDiscount = personalDiscount;
					uow.Save(personalDiscount);
				}
			}
			else if(receivedDiscount <= totalDiscount)
			{
				if(personalDiscount.Id > 0)
				{
					saleItem.PersonalDiscount = null;
					saleItem.DiscountReasons.Remove(personalDiscount.DiscountReason);
				}
			}
			
			return totalDiscountValueFromReasons;
		}

		protected virtual void CalculateAndSetDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue newDiscount)
		{
			var discountValue = CalculateDiscount(saleItem, newDiscount);
			saleItem.SetDiscount(discountValue);
		}

		/// <summary>
		/// Удаление скидки из строки заказа
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		protected virtual void ClearDiscounts(IApplyDiscountReasonItem saleItem)
		{
			saleItem.DiscountReasons.Clear();
			saleItem.PersonalDiscount = null;
			saleItem.SetDiscount(DiscountValue.CreateZero());
		}

		/// <summary>
		/// Пересчет итоговой скидки из оснований скидок
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		protected virtual void RecalculateTotalDiscount(IApplyDiscountReasonItem saleItem)
		{
			var discountValue = CalculateTotalDiscountValueFromDiscountReasonsWithoutPersonalDiscount(saleItem);
			SetDiscount(saleItem, discountValue);
		}

		/// <summary>
		/// Установка скидки
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <param name="discountValue">Значение скидки <see cref="IDiscountValue"/></param>
		protected virtual void SetDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue discountValue)
		{
			saleItem.SetDiscount(discountValue);
		}
		
		/// <summary>
		/// Расчет итоговой скидки в деньгах по основаниям скидки
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <returns>Скидка в деньгах</returns>
		protected virtual IDiscountValue CalculateTotalDiscountValueFromDiscountReasonsWithoutPersonalDiscount(
			IApplyDiscountReasonItem saleItem
			)
		{
			var currentPrice = saleItem.CurrentRawPrice;
			var tempDiscountValue = CalculateTempDiscountWithoutPersonalDiscount(saleItem);

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
		
		private static IDiscountValue CalculateDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue newDiscount)
		{
			IDiscountValue discountValue = null;

			if(saleItem.CurrentRawPrice == 0 || newDiscount.IsZeroDiscount)
			{
				//TODO-5967 возможно стоит очищать все скидки при нуле ClearDiscounts
				discountValue = DiscountValue.CreateZero(newDiscount.IsDiscountMoney);
			}
			else if(newDiscount.IsDiscountMoney)
			{
				var discountMoney = newDiscount.DiscountMoney > saleItem.CurrentRawPrice
					? saleItem.CurrentRawPrice
					: newDiscount.DiscountMoney < 0
						? 0
						: newDiscount.DiscountMoney;
				
				var discountPercent = 100 * discountMoney / saleItem.CurrentRawPrice;
				
				discountValue = DiscountValue.Create(saleItem.DiscountData.IsDiscountMoney, discountPercent, discountMoney);
			}
			else
			{
				var discountPercent = newDiscount.Discount > 100
					? 100
					: newDiscount.Discount < 0
						? 0
						: newDiscount.Discount;
				
				var discountMoney = saleItem.CurrentRawPrice * discountPercent / 100;
				
				discountValue = DiscountValue.Create(saleItem.DiscountData.IsDiscountMoney, discountPercent, discountMoney);
			}

			return discountValue;
		}
		
		private IDiscountValue CalculateTempDiscountWithoutPersonalDiscount(IApplyDiscountReasonItem saleItem)
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
		
		private void RemoveDiscountReasons(IApplyDiscountReasonItem saleItem, IList<DiscountReason> discountReasons)
		{
			foreach(var reason in discountReasons)
			{
				discountReasons.Remove(reason);

				if(reason.Id == PersonalDiscountReasonId)
				{
					saleItem.PersonalDiscount = null;
				}
			}
		}
		
		private bool CanApplyToPromoSet(int promoSetId, IEnumerable<int> promoSetIds)
		{
			return promoSetIds.Any(id => promoSetId == id);
		}
	}
}
