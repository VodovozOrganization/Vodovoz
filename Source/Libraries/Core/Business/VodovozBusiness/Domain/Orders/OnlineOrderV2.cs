using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public class OnlineOrderV2 : OnlineOrder
	{
		private IList<OnlineOrderPromoSet> _promoSets = new List<OnlineOrderPromoSet>();
		
		/// <summary>
		/// Промонаборы
		/// </summary>
		[Display(Name = "Промонаборы")]
		public virtual IList<OnlineOrderPromoSet> PromoSets
		{
			get => _promoSets;
			set => SetField(ref _promoSets, value);
		}

		/// <inheritdoc/>>
		public override OnlineOrderVersion OrderVersion => OnlineOrderVersion.V2;
	}
}
