using System;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;

namespace Vodovoz.Factories
{
	public class GroupNomenclaturePriceModelFactory
	{
		private readonly NomenclatureCostPurchasePriceModel _nomenclatureCostPurchasePriceModel;
		private readonly NomenclatureInnerDeliveryPriceModel _nomenclatureInnerDeliveryPriceModel;

		public GroupNomenclaturePriceModelFactory(
			NomenclatureCostPurchasePriceModel nomenclatureCostPurchasePriceModel,
			NomenclatureInnerDeliveryPriceModel nomenclatureInnerDeliveryPriceModel)
		{
			_nomenclatureCostPurchasePriceModel = nomenclatureCostPurchasePriceModel ?? throw new ArgumentNullException(nameof(nomenclatureCostPurchasePriceModel));
			_nomenclatureInnerDeliveryPriceModel = nomenclatureInnerDeliveryPriceModel ?? throw new ArgumentNullException(nameof(nomenclatureInnerDeliveryPriceModel));
		}

		public GroupNomenclaturePriceModel CreateModel(DateTime date, Nomenclature nomenclature)
		{
			return new GroupNomenclaturePriceModel(date, nomenclature, _nomenclatureCostPurchasePriceModel, _nomenclatureInnerDeliveryPriceModel);
		}
	}
}
