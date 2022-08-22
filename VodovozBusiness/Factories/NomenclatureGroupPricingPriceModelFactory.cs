using System;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;

namespace Vodovoz.Factories
{
	public class NomenclatureGroupPricingPriceModelFactory
	{
		private readonly NomenclatureCostPurchasePriceModel _nomenclatureCostPurchasePriceModel;
		private readonly NomenclatureInnerDeliveryPriceModel _nomenclatureInnerDeliveryPriceModel;

		public NomenclatureGroupPricingPriceModelFactory(
			NomenclatureCostPurchasePriceModel nomenclatureCostPurchasePriceModel,
			NomenclatureInnerDeliveryPriceModel nomenclatureInnerDeliveryPriceModel)
		{
			_nomenclatureCostPurchasePriceModel = nomenclatureCostPurchasePriceModel ?? throw new ArgumentNullException(nameof(nomenclatureCostPurchasePriceModel));
			_nomenclatureInnerDeliveryPriceModel = nomenclatureInnerDeliveryPriceModel ?? throw new ArgumentNullException(nameof(nomenclatureInnerDeliveryPriceModel));
		}

		public NomenclatureGroupPricingPriceModel CreateModel(DateTime date, Nomenclature nomenclature)
		{
			return new NomenclatureGroupPricingPriceModel(date, nomenclature, _nomenclatureCostPurchasePriceModel, _nomenclatureInnerDeliveryPriceModel);
		}
	}
}
