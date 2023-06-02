using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Models
{
	public interface INomenclatureGroupPricingModel
	{
		IEnumerable<NomenclatureGroupPricingPriceModel> PriceModels { get; }
		void LoadPrices(DateTime dateTime);
		void SavePrices();
	}
}
