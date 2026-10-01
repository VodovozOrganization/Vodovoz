using System;
using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Factories
{
	/// <inheritdoc/>
	public class OnlineOrderTemplateSaleItemFactory : IOnlineOrderTemplateSaleItemFactory
	{
		/// <inheritdoc/>
		public OnlineOrderTemplateSaleItem Create(object source, NewOrderSaleItem newSaleItem)
		{
			if(!(source is OnlineOrderTemplate template))
			{
				throw new InvalidOperationException("Online order template must be of type OnlineOrderTemplate");
			}

			return OnlineOrderTemplateSaleItem.Create(template.Id, newSaleItem);
		}

		ISaleItem ISaleItemFactory.Create(object source, NewOrderSaleItem newSaleItem)
		{
			return Create(source, newSaleItem);
		}
	}
}
