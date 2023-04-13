using System;
using Vodovoz.Domain.Goods;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Models
{
	public interface IFixedPricesModel
	{
		GenericObservableList<NomenclatureFixedPrice> FixedPrices { get; }
		
		event EventHandler FixedPricesUpdated;

		void AddOrUpdateFixedPrice(Nomenclature nomenclature, decimal fixedPrice, int minCount, int nomenclatureFixedPriceId);

		void RemoveFixedPrice(NomenclatureFixedPrice nomenclatureFixedPrice);

	}
}
