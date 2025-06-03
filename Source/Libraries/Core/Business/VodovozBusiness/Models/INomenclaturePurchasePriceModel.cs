using System;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Models
{
	public interface INomenclaturePurchasePriceModel
	{
		bool CanChangeDate(Nomenclature nomenclature, NomenclaturePurchasePrice price, DateTime startDate);
		bool CanCreatePrice(Nomenclature nomenclature, DateTime startDate);
		bool CanCreatePrice(Nomenclature nomenclature, DateTime startDate, decimal newPrice);
		void ChangeDate(Nomenclature nomenclature, NomenclaturePurchasePrice price, DateTime startDate);
		void CloseActivePrice(Nomenclature nomenclature, DateTime startDate);
		NomenclaturePurchasePrice CreatePrice(Nomenclature nomenclature, DateTime startDate);
		NomenclaturePurchasePrice CreatePrice(Nomenclature nomenclature, DateTime startDate, decimal price);
		NomenclaturePurchasePrice GetActivePrice(Nomenclature nomenclature);
		NomenclaturePurchasePrice GetPrice(DateTime date, Nomenclature nomenclature);
	}
}
