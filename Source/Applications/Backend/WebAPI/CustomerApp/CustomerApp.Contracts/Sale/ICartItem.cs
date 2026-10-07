namespace CustomerApp.Contracts.Sale
{
	public interface ICartItem : ICartItemBase
	{
		/// <summary>
		/// Id товара/услуги в ДВ
		/// </summary>
		int ErpId { get; }
	}
}
