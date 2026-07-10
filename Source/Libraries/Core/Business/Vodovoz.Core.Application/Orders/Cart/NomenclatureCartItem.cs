using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders.Cart;

namespace Vodovoz.Core.Application.Orders.Cart
{
	public class NomenclatureCartItem : INomenclatureCartItem
	{
		/// <inheritdoc/>
		public Nomenclature Nomenclature { get; set; }
		/// <inheritdoc/>
		public decimal Count { get; set; }
	}
}
