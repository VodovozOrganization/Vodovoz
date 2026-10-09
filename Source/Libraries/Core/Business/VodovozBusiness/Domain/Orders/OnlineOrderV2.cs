using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders.Delivery;

namespace VodovozBusiness.Domain.Orders
{
	public class OnlineOrderV2 : OnlineOrder, IOnlineOrderV2FreeDeliveryPrice
	{
		private bool _isAutoOrderEnabled;
		private IList<OnlineOrderPromoSet> _promoSets = new List<OnlineOrderPromoSet>();
		
		/// <summary>
		/// Включен автозаказ
		/// </summary>
		[Display(Name = "Включен автозаказ")]
		public virtual bool IsAutoOrderEnabled
		{
			get => _isAutoOrderEnabled;
			set => SetField(ref _isAutoOrderEnabled, value);
		}
		
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

		public virtual IEnumerable<OnlineOrderPromoSet> OnlinePromoSets => PromoSets;

		public void SetStatus(bool isAutoOrderEnabled)
		{
			if(isAutoOrderEnabled)
			{
				OnlineOrderStatus = OnlineOrderStatus.PendingAutoOrderSettings;
			}
			else
			{
				if(OnlineOrderPaymentStatus == Vodovoz.Core.Domain.Orders.OnlineOrderPaymentStatus.UnPaid
					&& OnlineOrderPaymentType == Vodovoz.Core.Domain.Orders.OnlineOrderPaymentType.PaidOnline)
				{
					OnlineOrderStatus = OnlineOrderStatus.WaitingForPayment;
				}
				else
				{
					OnlineOrderStatus = OnlineOrderStatus.New;
				}
			}
		}
	}
}
