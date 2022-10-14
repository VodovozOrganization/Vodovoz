using System;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;

namespace Vodovoz.Factories
{
	public class NomenclatureGroupPricingPriceModelFactory
	{
		private readonly NomenclatureCostPriceModel _nomenclatureCostPriceModel;
		private readonly NomenclaturePurchasePriceModel _nomenclaturePurchasePriceModel;
		private readonly NomenclatureInnerDeliveryPriceModel _nomenclatureInnerDeliveryPriceModel;

		public NomenclatureGroupPricingPriceModelFactory(
			NomenclatureCostPriceModel nomenclatureCostPriceModel,
			NomenclaturePurchasePriceModel nomenclaturePurchasePriceModel,
			NomenclatureInnerDeliveryPriceModel nomenclatureInnerDeliveryPriceModel)
		{
			_nomenclatureCostPriceModel = nomenclatureCostPriceModel ?? throw new ArgumentNullException(nameof(nomenclatureCostPriceModel));
			_nomenclaturePurchasePriceModel = nomenclaturePurchasePriceModel ?? throw new ArgumentNullException(nameof(nomenclaturePurchasePriceModel));
			_nomenclatureInnerDeliveryPriceModel = nomenclatureInnerDeliveryPriceModel ?? throw new ArgumentNullException(nameof(nomenclatureInnerDeliveryPriceModel));
		}

		public NomenclatureGroupPricingPriceModel CreateModel(DateTime date, Nomenclature nomenclature)
		{
			return new NomenclatureGroupPricingPriceModel(date, nomenclature, _nomenclatureCostPriceModel, _nomenclaturePurchasePriceModel, _nomenclatureInnerDeliveryPriceModel);
		}
	}
}
