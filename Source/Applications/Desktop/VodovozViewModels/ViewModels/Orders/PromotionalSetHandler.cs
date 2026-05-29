using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Factories;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public abstract class PromotionalSetHandler : ProductHandler
	{
		protected PromotionalSetHandler(
			ISaleItemFactory saleItemFactory,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureService nomenclatureService,
			INomenclatureRepository nomenclatureRepository,
			IGoodsPriceCalculator goodsPriceCalculator,
			IAddProductValidator addProductValidator
			) : base(saleItemFactory, nomenclatureSettings, nomenclatureService, nomenclatureRepository, goodsPriceCalculator, addProductValidator)
		{
		}

		/// <summary>
		/// Активация промонабора при добавлении в продажу(заказ, шаблон)
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="addProductSource">Данные источника(заказ, шаблон)</param>
		/// <param name="proSet">Добавляемый промонабор</param>
		public virtual void ActivatePromotionalSet(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
			PromotionalSet proSet
		)
		{
			TryAddNomenclatureFromPromoSet(uow, addProductSource, proSet);
		}

		protected virtual Result TryAddNomenclatureFromPromoSet(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
			PromotionalSet proSet
			)
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
				var addProductResult = AddProductValidator.Validate(nomenclature, addProductSource);

				if(addProductResult.IsFailure)
				{
					return addProductResult;
				}

				AddProduct(
					uow,
					addProductSource,
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

		protected override void Recalculate()
		{
			
		}
	}
}
