using System;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;

namespace Vodovoz.Factories
{
	public interface INomenclatureGroupPricingPriceModelFactory
	{
		NomenclatureGroupPricingPriceModel CreateModel(DateTime date, Nomenclature nomenclature);
	}
}
