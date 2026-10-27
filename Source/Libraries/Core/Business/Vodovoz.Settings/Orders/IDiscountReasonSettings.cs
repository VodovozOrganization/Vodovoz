namespace Vodovoz.Settings.Orders
{
	public interface IDiscountReasonSettings
	{
		int GetSelfDeliveryDiscountReasonId { get; }
		/// <summary>
		/// Идентификатор основания персональной скидки
		/// </summary>
		int PersonalDiscountReasonId { get; }
		/// <summary>
		/// Id основания скидки для первого онлайн заказа
		/// </summary>
		int FirstOnlineOrderDiscountReasonId { get; }
	}
}
