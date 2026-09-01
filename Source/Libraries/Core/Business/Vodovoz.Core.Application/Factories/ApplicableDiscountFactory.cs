using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Extensions;
using VodovozBusiness.Factories;

namespace Vodovoz.Core.Application.Factories
{
	public class ApplicableDiscountFactory : IApplicableDiscountFactory
	{
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly IGenericRepository<PromotionalSet> _promotionalSetRepository;
		private readonly IDiscountReasonRepository _discountReasonRepository;

		public ApplicableDiscountFactory(
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IGenericRepository<PromotionalSet> promotionalSetRepository,
			IDiscountReasonRepository discountReasonRepository
			)
		{
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
		}

		public IApplicableDiscount CreateApplicableDiscount(
			IUnitOfWork uow,
			IOrderedCartItem orderedCartItem)
		{
			PromotionalSet promotionalSet = null;
			Nomenclature nomenclature = null;
			
			//TODO-5967 продумать момент, если придут кривые идентификаторы и тогда обе сущности могут быть null и упадет в проверке на применимость скидки
			switch(orderedCartItem.ItemType)
			{
				case SaleItemType.PromoSet:
					promotionalSet = _promotionalSetRepository.GetFirstOrDefault(
						uow,
						x => x.Id == orderedCartItem.ErpId);
					break;
				default:
					nomenclature = _nomenclatureRepository.GetFirstOrDefault(
						uow,
						x => x.Id == orderedCartItem.ErpId);
					break;
			}

			return new ApplicableDiscount
			{
				Price = orderedCartItem.Price,
				Count = orderedCartItem.Count,
				IsFixedPrice = orderedCartItem.IsFixedPrice,
				Nomenclature = nomenclature,
				PromoSet = promotionalSet,
				DiscountReasons = orderedCartItem.DiscountIds
					.ToDiscountReasonBases(uow, _discountReasonRepository)
			};
		}
	}
}
