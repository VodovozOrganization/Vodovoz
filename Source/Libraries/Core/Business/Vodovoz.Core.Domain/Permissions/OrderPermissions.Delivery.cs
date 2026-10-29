namespace Vodovoz.Core.Domain.Permissions
{
	public partial class OrderPermissions
	{
		/// <summary>
		/// Права на настройки доставки
		/// </summary>
		public static class Delivery
		{
			/// <summary>
			/// Право на изменение признака бесконтактной доставки в заказе
			/// </summary>
			public static string CanSetContactlessDelivery => nameof(CanSetContactlessDelivery);
		}
	}
}
