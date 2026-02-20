using System;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Models
{
	public interface INomenclaturePurchasePriceModel
	{
		bool CanChangeDate(Nomenclature nomenclature, NomenclaturePurchasePrice price, DateTime startDate);
		bool CanCreatePrice(NomenclatureEntity nomenclature, DateTime startDate);
		bool CanCreatePrice(NomenclatureEntity nomenclature, DateTime startDate, decimal newPrice);
		void ChangeDate(Nomenclature nomenclature, NomenclaturePurchasePrice price, DateTime startDate);
		void CloseActivePrice(NomenclatureEntity nomenclature, DateTime startDate);
		NomenclaturePurchasePrice CreatePrice(NomenclatureEntity nomenclature, DateTime startDate);
		NomenclaturePurchasePrice CreatePrice(NomenclatureEntity nomenclature, DateTime startDate, decimal price);
		NomenclaturePurchasePrice GetActivePrice(NomenclatureEntity nomenclature);
		NomenclaturePurchasePrice GetPrice(DateTime date, Nomenclature nomenclature);
	}
}
