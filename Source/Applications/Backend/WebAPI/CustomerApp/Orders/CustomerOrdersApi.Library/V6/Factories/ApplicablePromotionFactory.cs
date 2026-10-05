using System;
using System.Linq;
using CustomerOrders.Abstractions.V6.Sale;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V6.Factories
{
	internal class ApplicablePromotionFactory : IApplicablePromotionFactory
	{
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly IGenericRepository<PromotionalSet> _promotionalSetRepository;
		private readonly IDiscountReasonRepository _discountReasonRepository;

		public ApplicablePromotionFactory(
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IGenericRepository<PromotionalSet> promotionalSetRepository,
			IDiscountReasonRepository discountReasonRepository
			)
		{
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
		}
		
		/// <inheritdoc/>
		public IApplicablePromotion CreateApplicablePromotion(
			IUnitOfWork uow,
			IOnlineOrderedProduct orderedCartItem)
		{
			var nomenclature = _nomenclatureRepository.GetFirstOrDefault(
				uow,
				x => x.Id == orderedCartItem.NomenclatureId);

			return new ApplicablePromotion
			{
				Price = orderedCartItem.Price,
				Count = orderedCartItem.Count,
				IsFixedPrice = orderedCartItem.IsFixedPrice,
				Nomenclature = nomenclature,
				PromoSet = null,
				DiscountReasons = Enumerable.Empty<DiscountReasonBase>()
			};
		}
	}
}
