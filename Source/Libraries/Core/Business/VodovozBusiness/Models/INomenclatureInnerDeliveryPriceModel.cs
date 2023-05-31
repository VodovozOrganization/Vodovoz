using System;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Models
{
	public interface INomenclatureInnerDeliveryPriceModel
	{
		NomenclatureInnerDeliveryPrice CreatePrice(Nomenclature nomenclature, DateTime startDate);
		bool CanCreatePrice(Nomenclature nomenclature, DateTime startDate);
		void ChangeDate(Nomenclature nomenclature, NomenclatureInnerDeliveryPrice price, DateTime startDate);
		bool CanChangeDate(Nomenclature nomenclature, NomenclatureInnerDeliveryPrice price, DateTime startDate);
		void CloseActivePrice(Nomenclature nomenclature, DateTime startDate);
		NomenclatureInnerDeliveryPrice GetPrice(DateTime date, Nomenclature nomenclature);
	}
}
