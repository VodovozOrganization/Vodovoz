using System;
using System.Linq;
using CustomerOrders.Abstractions.V4.Sale;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V4.Factories
{
	internal class ApplicablePromotionFactory : IApplicablePromotionFactory
	{
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;

		public ApplicablePromotionFactory(IGenericRepository<Nomenclature> nomenclatureRepository)
		{
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
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
