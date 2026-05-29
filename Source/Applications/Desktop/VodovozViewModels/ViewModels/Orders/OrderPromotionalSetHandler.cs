using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Factories;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OrderPromotionalSetHandler : PromotionalSetHandler
	{
		public OrderPromotionalSetHandler(
			ISaleItemFactory saleItemFactory,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureService nomenclatureService,
			INomenclatureRepository nomenclatureRepository,
			IGoodsPriceCalculator goodsPriceCalculator,
			IAddProductValidator addProductValidator
			) : base(saleItemFactory, nomenclatureSettings, nomenclatureService, nomenclatureRepository, goodsPriceCalculator, addProductValidator)
		{
			
		}
		
		public override void ActivatePromotionalSet(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
			IPromoSet proSet
		)
		{
			//Добавление спец. действий промонабора
			foreach(var action in proSet.PromotionalSetActions)
			{
				action.Activate(addProductSource.Source as Order);
			}

			base.TryAddNomenclatureFromPromoSet(uow, addProductSource, proSet);
			addProductSource.PromoSets.Add(proSet);
		}
	}
}
