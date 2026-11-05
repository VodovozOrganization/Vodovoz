using CustomerApp.Contracts.Sale;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders.Cart;

namespace Vodovoz.Core.Application.Sale.Cart
{
	public class NomenclatureCartItem : INomenclatureCartItem
	{
		/// <inheritdoc/>
		public Nomenclature Nomenclature { get; set; }
		/// <inheritdoc/>
		public decimal Count { get; set; }
		/// <inheritdoc/>
		public SaleItemType ItemType => Nomenclature.Category.ToSaleItemType();

		public static NomenclatureCartItem Create(Nomenclature nomenclature, decimal count) =>
			new NomenclatureCartItem
			{
				Nomenclature = nomenclature,
				Count = count
			};
	}
}
