namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	/// <summary>
	/// Проверенный пакет аренды онлайн заказа из корзины
	/// </summary>
	public class CheckedOnlineRentPackageDto : OnlineRentPackageDto
	{
		/// <summary>
		/// Статус товара <see cref="CartItemStatus"/>
		/// </summary>
		public CartItemStatus Status { get; set; }
		
		public static CheckedOnlineRentPackageDto Create(OnlineRentPackageDto package, CartItemStatus status = CartItemStatus.Active)
		{
			return new CheckedOnlineRentPackageDto
			{
				Count = package.Count,
				Price = package.Price,
				RentPackageId = package.RentPackageId,
				Status = status,
			};
		}
	}
}
