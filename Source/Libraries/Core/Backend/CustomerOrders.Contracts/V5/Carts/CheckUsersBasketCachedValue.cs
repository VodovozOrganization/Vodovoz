using System;

namespace CustomerOrders.Contracts.V5.Carts
{
	/// <summary>
	/// Данные по проверке в корзине
	/// </summary>
	[Serializable]
	public class CheckUsersBasketCachedValue
	{
		/// <summary>
		/// Идентификатор проверки
		/// </summary>
		public Guid CheckId { get; set; }
		/// <summary>
		/// Данные запроса проверки в корзине
		/// </summary>
		public CheckUsersBasketRequest Request { get; set; }
		/// <summary>
		/// Данные ответа проверки в корзине
		/// </summary>
		public CheckUsersBasketResponse Response { get; set; }

		public static CheckUsersBasketCachedValue Create(
			Guid checkId,
			CheckUsersBasketRequest request,
			CheckUsersBasketResponse response) =>
			new CheckUsersBasketCachedValue
			{
				CheckId = checkId,
				Request = request,
				Response = response
			};
	}
}
