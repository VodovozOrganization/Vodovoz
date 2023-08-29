using System;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Models
{
	public interface INomenclatureCostPriceModel
	{
		NomenclatureCostPrice CreatePrice(Nomenclature nomenclature, DateTime startDate);
		NomenclatureCostPrice CreatePrice(Nomenclature nomenclature, DateTime startDate, decimal price);
		bool CanCreatePrice(Nomenclature nomenclature, DateTime startDate);
		bool CanCreatePrice(Nomenclature nomenclature, DateTime startDate, decimal newPrice);
		void ChangeDate(Nomenclature nomenclature, NomenclatureCostPrice price, DateTime startDate);
		bool CanChangeDate(Nomenclature nomenclature, NomenclatureCostPrice price, DateTime startDate);
		void CloseActivePrice(Nomenclature nomenclature, DateTime startDate);
		NomenclatureCostPrice GetActivePrice(Nomenclature nomenclature);
		NomenclatureCostPrice GetPrice(DateTime date, Nomenclature nomenclature);
	}
}
