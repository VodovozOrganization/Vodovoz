using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Factories
{
	public interface ISaleItemFactory
	{
		IProduct Create(object source, decimal count, decimal price, Nomenclature nomenclature);
	}
}
