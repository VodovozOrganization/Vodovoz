using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Factories
{
	public class OnlineOrderTemplateSaleItemFactory : ISaleItemFactory
	{
		public IProduct Create(
			object source,
			decimal count,
			decimal price,
			Nomenclature nomenclature
		)
		{
			return OnlineOrderTemplateProduct.Create(
				(source as OnlineOrderTemplate).Id, //templateId,
				count,
				price,
				nomenclature,
				proSet,
				new ObservableList<OnlineOrderTemplateProductDiscount>()
			);
		}
	}
}
